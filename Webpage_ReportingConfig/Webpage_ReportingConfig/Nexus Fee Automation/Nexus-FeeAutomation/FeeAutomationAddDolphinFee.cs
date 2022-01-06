using FeeAutomationLibrary;
using HouseOfTravel.Data;
using HouseOfTravel.DataSources.BusinessLogic.Dolphin.Mappers;
using HouseOfTravel.DataSources.Models.DolphinDtm;
using HouseOfTravel.DataSources.Models.Itinerary;
using Nexus_FeeAutomation.BusinessLogic;
using Nexus_FeeAutomation.DataStores;
using NexusLibrary;
using NexusLibrary.Dolphin;
using NexusLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus_FeeAutomation
{
    /// <summary>
    /// Handler that takes a single fee, and adds it to the Dolphin Folder
    /// </summary>
    [NexusQueue(QueueName = "FeeAutomation", Source = "*", Action = "AddDolphinFee")]
    public class FeeAutomationAddDolphinFee : DolphinHandler_FolderCreateUpdate<PendingFee>
    {
        private DolphinHelper helper;
        private DTM_TravelFolder folder;
        /// <summary>
        /// Validate whether we should process the message or not.
        /// </summary>
        /// <returns>True to process the message, false to skip</returns>
        public async override Task<bool> PreprocessMessage()
        {
            helper = new DolphinHelper(Message);
            try
            {
                folder = await helper.GetExistingFolder(Payload.BranchCode, Payload.FolderNumber);
            }
            catch (Exception ex)
            {
                // if folder we are unable to find the folder, then we bail
                string errorMessage = $"Error: Unable to find existing booking or booking voided {Payload.BranchCode}{Payload.FolderNumber}";
                //Process Complete with no retry
                await AddHistory($"{errorMessage} Nexus Error: {ex}", MessageState.Failed);
                await PendingFee_UpdateStatusAsync(Payload.Id, "Failed", errorMessage);
                return await Task.FromResult(false);
            }
            //If we didn't get folder object then raise exception and bail 
            if (folder == null)
            {
                await PendingFee_UpdateStatusAsync(Payload.Id, "Failed", $"Unable to find existing booking {Payload.BranchCode}{Payload.FolderNumber}");
                throw new PayloadFormatException($"Unable to find existing booking {Payload.BranchCode}{Payload.FolderNumber}");
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Get the travel folder and Add New Selected Fee and return to the folder to be updated to Dolphin
        /// </summary>
        /// <returns></returns>
        public override Task<DTM_TravelFolder> GetTravelFolder()
        {
            AdjustmentMapper mapper = new AdjustmentMapper(new FeeAutomationAdjustmentLogic(), new AdjustmentPriceMapper(new FeeAutomationAdjustmentPriceLogic()));
            PriceAdjustment priceAdjustment = new PriceAdjustment() { Code = Payload.ProductCode, Type = AdjustmentType.Fee, Price = { new Cost("FEE", Payload.Description, Payload.FeeValue) } };
            Fee newFee = mapper.Map<Fee>(priceAdjustment);

            //Add Existing Fees if exists
            List<Fee> fees = folder.TravelFolder.Fees?.Fee == null ? new List<Fee>() : folder.TravelFolder.Fees.Fee.ToList();
            //Add New fee
            fees.Add(newFee);
            folder.TravelFolder.Fees = new Fees() { Fee = fees.ToArray() };
            folder.TravelFolder.SourceSystemName = string.IsNullOrEmpty(LocalConfiguration<FeeAutomationAddDolphinFee>.AppSettings("FeeAutomationFolderSourceSystemName")) ? folder.TravelFolder.SourceSystemName : LocalConfiguration<FeeAutomationAddDolphinFee>.AppSettings("FeeAutomationFolderSourceSystemName");
            // Clear out the properties we aren't interested in so Dolphin won't attempt to update (null = don't update) so we don't mess with
            // the other fields unnecessarily
            // We HAVE to do this as Dolphin sends back the wrong values (GST exclusive instead of inclusive) for pricing etc from their API, which
            // causes us to overwrite with the wrong values!
            ClearFolderPropertiesToSkip(folder);

            return Task.FromResult(folder);
        }

        /// <summary>
        /// Override Abstract method SavePolicyComplianceInformation - Skipping Policy Compliance saving as not required
        /// </summary>
        /// <returns></returns>
        public override Task SavePolicyComplianceInformation()
        {
            // Skipping Policy Compliance saving as not required
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears out the properties we aren't interested in so Dolphin won't attempt to update (null = don't update)
        /// </summary>
        /// <param name="folder"></param>
        public void ClearFolderPropertiesToSkip(DTM_TravelFolder folder)
        {
            // Clear the other properties out
            DTM_TravelFolderTravelFolder travelFolder = folder.TravelFolder;
            travelFolder.CustomerForBooking = null;
            travelFolder.PassengerListItems = null;
            travelFolder.FlightSegmentGroups = null;
            travelFolder.VehicleSegments = null;
            travelFolder.AccommodationSegments = null;
            travelFolder.FlightPricingInfos = null;
            travelFolder.Discounts = null;
            travelFolder.OtherSegments = null;
            travelFolder.CruiseSegments = null;
            travelFolder.InsuranceSegments = null;
            travelFolder.TailorMadePackages = null;
            travelFolder.ReservationCommentItems = null;
            travelFolder.CustomerPayments = null;
            travelFolder.VendorPayments = null;
        }

        /// <summary>
        /// Override onSuccess to Change the Fee Status to FeeApplied 
        /// </summary>
        /// <returns></returns>
        public async override Task OnSuccess()
        {
            await PendingFee_UpdateStatusAsync(Payload.Id, Payload.AutoApply ? "AutoApplied" : "FeeApplied" );
            await UpdateFeeTypeIntoFolderEnhanceDataWithReTry();
            await base.OnSuccess();
        }

        /// <summary>
        /// Update FeeType Into Folder Enhance Data With ReTry
        /// </summary>
        /// <param name="retryAttempts"></param>
        /// <returns></returns>
        private async Task UpdateFeeTypeIntoFolderEnhanceDataWithReTry(int retryAttempts = 0)
        {
            int MaxRetryAttempts = 3;
            if(!await DataStores.DolphinStore.UpdateFeeTypeIntoFolderEnhanceData(Payload, retryAttempts))
            {
                if (retryAttempts < MaxRetryAttempts)
                {
                    await UpdateFeeTypeIntoFolderEnhanceDataWithReTry(++retryAttempts);
                }
            }
          
        }
        /// <summary>
        /// Any error while adding fee, change the message state to  MessageState.Failed
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public async override Task OnFailure(string errorMessage)
        {
            //Retry after 2 mins if Attempts are less than 50 OR For Add Fee Errors other then "...is not Read only" error on Fee Segment
            await AddHistory(errorMessage, MessageState.Failed, Message.Attempts > 50 || errorMessage.ToLower().Contains("could not set is interfaced flag on fee segment object because it is read only") ? null : (int?)2);

            await PendingFee_UpdateStatusAsync(Payload.Id, "Failed", errorMessage);
            // call the base method
            await base.OnFailure(errorMessage);
        }

        /// <summary>
        /// </summary>
        /// <returns>Update the Status to be Pending/Queued Or Cancelled and Reason of Failure 
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task PendingFee_UpdateStatusAsync(long id, string status, string reason = "")
        {
            if(!FeeAutomationStore.PendingFee_UpdateStatus(id, status, reason))
            { 
                string error = $"Fee Automation Error: Failed to update Fee Status {status} for Pending Fee id {id}.";
                // Add Error History
                await AddHistory(error, MessageState.Processed);
            }
        }
    }
}
