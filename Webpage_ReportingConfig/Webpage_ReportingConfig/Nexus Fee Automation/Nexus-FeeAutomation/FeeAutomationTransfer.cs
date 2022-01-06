using FeeAutomationLibrary;
using Nexus_FeeAutomation.DataStores;
using NexusLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Nexus_FeeAutomation
{
    /// <summary>
    /// Handler responsible for taking pending fees and moving them to the database in Azure
    /// </summary>
    [NexusQueue(QueueName = "FeeAutomation", Action = "TransferPendingFees", Source = "*")]
    public class FeeAutomationTransfer : NexusHandler<List<PendingFee>>
    {
        /// <summary>
        /// Processes the Payload and posts the individual fees to the table in Azure that the website presents to the user
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async override Task<bool> ProcessMessage(CancellationToken token)
        {
            try
            {
                foreach (PendingFee pendingFee in Payload)
                {
                    //Save fees in Pending Fee table
                    await FeeAutomationStore.PendingFeeSave(pendingFee);
                }
                //For Fee for AutoApply - Send to nexus queue to apply fee
                //Only fee with status "NEW" need AutoApply Cancelled and Voided item are ignored

                foreach (PendingFee pendingFee in Payload.FindAll(p => p.AutoApply && p.Status.Equals("NEW", StringComparison.CurrentCultureIgnoreCase)))
                {
                   await SetFeeValueAndQueueAddFeeMessageToNexus(pendingFee);
                }

                // Archive Old Pending Fee Rows all the other process done
                await FeeAutomationStore.PendingFee_Archive();

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                log.Error($"{ProcessorName}: Error calling PendingFee: {ex}");
                await AddHistory($"{ProcessorName}: Error calling PendingFee: {ex}", MessageState.Failed);
            }

            // if we get here, we failed :(
            return await Task.FromResult(false);
        }
        /// <summary>
        /// Set Required Fee Value and send to Nexus Message queue to apply fee 
        /// </summary>
        /// <param name="pendingFee"></param>
        /// <returns></returns>
        private async Task SetFeeValueAndQueueAddFeeMessageToNexus(PendingFee pendingFee)
        {
            PendingFee pendingFeeWithFeeValue = FeeAutomationStore.PendingFee_Get_WithFeeValue(pendingFee.FolderNumber).Find(p => p.Id == pendingFee.Id);
            if (pendingFeeWithFeeValue == null)
            {
                log.Error($"In SetFeeValueAndQueueAddFeeMessageToNexus could not set pendingFeeWithFeeValue object for {pendingFee.FeeType} Pending Fee Id: {pendingFee.Id} ");
                return;
            }

            // reconfirm that only fee item which are of NEW status need fee applying
            // cancelled and voided item are ignored
            // matches auto apply criteria based on company/profile for FeeType 
            if (!pendingFeeWithFeeValue.Status.Equals("NEW", StringComparison.OrdinalIgnoreCase) ||
                pendingFeeWithFeeValue.FeeValue == 0 ||
                !pendingFeeWithFeeValue.AutoApply 
                )
            {
                log.Info($"Pending Fee Id {pendingFee.Id} Fee Type {pendingFeeWithFeeValue.FeeType} OR Fee Status {pendingFeeWithFeeValue.Status} OR Fee Value=0 not applicable to Auto Apply Fee");
                return;
            }

            pendingFeeWithFeeValue.ProductCode = pendingFee.FeeType;
            FeeAutomationStore.PendingFee_UpdateStatus(pendingFee.Id, "Pending Automation");
            long messageId = await NexusHelper.QueueMessage("FeeAutomation", "AddDolphinFee", pendingFeeWithFeeValue);
            log.Info($"MessageId for AddDolphinFee Queued {messageId}");
            if (messageId == 0)
            {
                //Fee Need to applied Manually Setting status to "NEW" to be available in UI 
                FeeAutomationStore.PendingFee_UpdateStatus(pendingFee.Id, "NEW");
                log.Error($"Queue Message Failed for Auto Applying Fee. Apply Manually in UI Fee Type: {pendingFee.FeeType} Pending Fee Id: {pendingFee.Id} ");
            }
        }
    }
}
