using FeeAutomationLibrary;
using FeesAutomationWebsite.Common;
using FeesAutomationWebsite.DataStores;
using FeesAutomationWebsite.Models;
using FeesAutomationWebsite.Models.DataTable;
using log4net;
using NexusLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Controllers
{
    /// <summary>
    /// the main PendingFee controller
    /// </summary>
    [CustomAuthorize]
    public class PendingFeeController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AccessStore));

        /// <summary>
        /// Checking the Access passed and Set Acess to OutletCode Branch based on access key
        /// </summary>
        /// <param name="outlet"></param>
        /// <param name="feeStatus"></param>
        /// <returns></returns>
        [HandleError()]
        public ActionResult Index(string outlet = null, string feeStatus = null)
        {
            List<Access> branchOutletAccess = AccessStore.Access_Search((string)Session["AccessKey"]);
            if (branchOutletAccess?.Count == 0)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }

            PendingFeeFilterDataTableAjaxPostModel outletSelected = new PendingFeeFilterDataTableAjaxPostModel()
            { OutletCode = outlet, FeeStatus = feeStatus };
            _log.Info($"Access Key Matched for Branch {branchOutletAccess.FirstOrDefault<Access>().BranchCode}");
            ViewBag.BranchOutletAccess = branchOutletAccess;
            ViewBag.FeeCategories = PendingFeeStore.GetFeeTypeList(true);
            return View(outletSelected);
        }

        /// <summary>
        /// Get Response Object for Data table for Pending Fee List based on Extra Filter Selected by User
        /// </summary>
        /// <param name="dataTableAjaxPostModel"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult PendingFee_Search(PendingFeeFilterDataTableAjaxPostModel pendingFeeSearchModel)
        {
            DataTableResponse<PendingFeeExtended> pendingFeesDataTableResponse = PendingFeeStore.PendingFee_Search(pendingFeeSearchModel);
            _log.Info($"Got Pending Fees List in FeeAutomation using PendingFee_PendingFee_Search");
            return Json(pendingFeesDataTableResponse);
        }

        /// <summary>
        /// Processes the list of selected fees (received as a list of id's from the website)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Action Result of Plain Text with Error message to show if any</returns>
        /// Returns OK if everything is done successfully notify Datatable to reload 
        [System.Web.Http.HttpPost]
        public async Task<ActionResult> ProcessSelectedAsync(List<int> ids)
        {
            // If no ids were specified
            if (ids == null || ids.Count == 0)
            {
                // Invalid Operation Exception
                return Content("No Row selected for processing", MediaTypeNames.Text.Plain);
            }
            foreach (int id in ids)
            {
                long? nexusMessageId = null;
                try
                {
                    nexusMessageId = await QueueSelectedAsync(id);
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to Apply Fee - Message Queued Remotely Failed {ex.Message}");
                    return Content($"Failed to Queue selected rows to Apply Fee", MediaTypeNames.Text.Plain);
                }
                if (nexusMessageId == null)
                {

                    return Content("Failed to Queue selected rows to Apply Fee", MediaTypeNames.Text.Plain);
                }
            }
            //Returns OK if everything is done successfully notify Datatable to reload 
            return Content(HttpStatusCode.OK.ToString(), MediaTypeNames.Text.Plain);
        }

        /// <summary>
        /// Queue Message to Nexus to add a specific fee 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Nexus Message id if successfully message queued - null in case of any error</returns>
        private async Task<long?> QueueSelectedAsync(int id)
        {
            // Get the Pending fee Details 
            PendingFee pendingFee = PendingFeeStore.PendingFee_Get(id);
            if (pendingFee.Status.Equals(FeeStatus.New.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                //Queue in Nexus Handler
                string nexusQueueMessageBaseUri = ConfigurationManager.AppSettings["NexusQueueMessageBaseUri"];
                string nexusQueueMessageToken = ConfigurationManager.AppSettings["NexusQueueMessageToken"];
                string serializedContent = NexusHandlerBase.SerializeObject(pendingFee);
                // Manually Applying Fee
                pendingFee.AutoApply = false; 
                long? nexusMessageId = await NexusHelper.QueueMessageRemote(nexusQueueMessageBaseUri, nexusQueueMessageToken, serializedContent, "AddDolphinFee");

                if (nexusMessageId != null)
                {
                    _log.Info($"FeeAutomation - Message Queued Remotely Succesfully with Nexus Message Id {nexusMessageId}");
                    // update the record in the database
                    PendingFeeStore.PendingFee_UpdateStatus(id, FeeStatus.Pending);
                }
                else
                {
                    _log.Error($"FeeAutomation - Message Queued Remotely Failed");
                }
                return await Task.FromResult<long?>(nexusMessageId);
            }

            return await Task.FromResult<long?>(null);
        }


        /// <summary>
        /// Cancels the selected fees (i.e. sets them to the "Cancelled" state and does not process them)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Action Result of Plain Text with Error to show if any</returns>
        /// Returns OK if everything is done successfully notify Datatable to reload 
        [System.Web.Http.HttpPost]
        [HttpPost]
        public ActionResult CancelSelected(List<int> ids)
        {
            // If no ids were specified
            if (ids == null || ids.Count == 0)
            {
                // Invalid Operation Exception
                return Content("No Row selected for processing", MediaTypeNames.Text.Plain);
            }
            foreach (int id in ids)
            {
                PendingFeeStore.PendingFee_UpdateStatus(id, FeeStatus.Cancelled);
            }
            return Content(HttpStatusCode.OK.ToString(), MediaTypeNames.Text.Plain);
        }
    }
}