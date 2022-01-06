using FeeAutomationLibrary;
using FeesAutomationWebsite.Common;
using FeesAutomationWebsite.DataStores;
using FeesAutomationWebsite.Models;
using FeesAutomationWebsite.Models.DataTable;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Controllers
{
    /// <summary>
    /// the main PendingFee controller
    /// </summary>
    [CustomAuthorize]
    public class FeeTypeMappingController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FeeController));

        /// <summary>
        /// Check for AccessKey to authorise access
        /// </summary>
        /// <returns></returns>
        [HandleError()]
        // GET: FeeTypeMapping
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.GetAccess(accessKey);
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }

            List<FeeTypeMapping> feeTypeMappings;
            try
            {
                _log.Info($"Calling FeeTypeMappingStore.FeeTypeMapping_Get in Index Fee Automation Config Automation Controller API");
                feeTypeMappings = await FeeTypeMappingStore.FeeTypeMapping_Get(branchOutletAccess.OutletCode);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to get Mapping List in FeeAutomation API Error - {ex.InnerException}");
                return Content($"Failed to get Mapping List in FeeAutomation API Error", MediaTypeNames.Text.Plain);
            }
            _log.Info($"Got Fee Mapping List in Index FeeAutomation from Fee Automation Config Automation Controller API");

            return View(feeTypeMappings);
        }

        /// <summary>
        /// Get Response Object for Data table for Fee List based on Access Key used by User and stored in session
        /// </summary>
        /// <param name="dataTableAjaxPostModel"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<ActionResult> FeeTypeMapping_Search(DataTableAjaxPostModel dataTableAjaxPostModel = null)
        {
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.GetAccess(accessKey);
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }
            List<FeeTypeMapping> feeTypeMappings;
            try
            {
                _log.Info($"Calling FeeTypeMappingStore.FeeTypeMapping_Get Fee Automation Config Automation Controller API");
                feeTypeMappings = await FeeTypeMappingStore.FeeTypeMapping_Get(branchOutletAccess.OutletCode);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to get Mapping List in FeeAutomation API Error - {ex.InnerException}");
                return Content($"Failed to get Mapping List in FeeAutomation API Error", MediaTypeNames.Text.Plain);
            }
            _log.Info($"Got Fee Mapping List in FeeAutomation from Fee Automation Config Automation Controller API");

            return Json(new
            {
                data = feeTypeMappings,
                draw = Request["draw"],
                recordsTotal = feeTypeMappings.Count,
                recordsFiltered = feeTypeMappings.Count
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Create new Fee Type Mapping Item
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            FeeTypeMapping feeTypeMapping = new FeeTypeMapping();
            if (PrepareFeeTypeMappingObject(ref feeTypeMapping))
            {
                return View("Create", feeTypeMapping);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        /// <summary>
        /// Prepare Fee Type Mapping Object to be used for UI based on Access 
        /// return false if Access Key does not match
        /// </summary>
        /// <param name="feeTypeMapping"></param>
        /// <returns></returns>
        private bool PrepareFeeTypeMappingObject(ref FeeTypeMapping feeTypeMapping)
        {
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.GetAccess(accessKey);
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return false;
            }
            ViewBag.BookingTypeList = FeeTypeMappingStore.GetBookingTypeList();
            feeTypeMapping.BranchCode = branchOutletAccess.BranchCode;
            feeTypeMapping.OutletCode = branchOutletAccess.OutletCode;
            feeTypeMapping.ScopeList = Properties.Settings.Default.FeeScopeList.Cast<string>().ToList();
            feeTypeMapping.QueryList = Properties.Settings.Default.QueryTypeList.Cast<string>().ToList();

            return true;
        }

        /// <summary>
        /// Prepare for Edit Fee Mapping for Selected Row
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int id)
        {

            FeeTypeMapping feeTypeMapping = new FeeTypeMapping();
            if (!PrepareFeeTypeMappingObject(ref feeTypeMapping))
            {
                return new HttpUnauthorizedResult();
            }
            try
            {
                feeTypeMapping = (await FeeTypeMappingStore.FeeTypeMapping_Get(feeTypeMapping.OutletCode, id)).FirstOrDefault();
                //Prepare Object for UI
                PrepareFeeTypeMappingObject(ref feeTypeMapping);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to get Mapping id {id} in FeeAutomation API Error - {ex.InnerException}");
                return Content($"Failed to get Mapping {id} List in FeeAutomation API Error", MediaTypeNames.Text.Plain);
            }
            // show the page
            return View("Create", feeTypeMapping);
        }

        /// <summary>
        /// Save Fee type Mapping item by sending create request 
        /// </summary>
        /// <param name="feeTypeMapping"></param>
        /// <returns></returns>
        // POST: FeeTypeMapping/Create
        [HttpPost]
        public async Task<ActionResult> Save(FeeTypeMapping feeTypeMapping)
        {
            try
            {
                await FeeTypeMappingStore.FeeTypeMapping_Save(feeTypeMapping);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //prepare object to show error
                PrepareFeeTypeMappingObject(ref feeTypeMapping);
                feeTypeMapping.ErrorDescription = ex.ToString();
                return View("Create", feeTypeMapping);
            }
        }

        /// <summary>
        /// Delete Fee type Mapping item by sending delete request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            await FeeTypeMappingStore.FeeTypeMapping_Delete(id);

            return RedirectToAction("Index");

        }
        /// <summary>
        /// Update AutoApply based on Selected UnSelected Ids
        /// </summary>
        /// <param name="selectedIds"></param>
        /// <param name="unSelectedIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UpdateAutoApply(int[] selectedIds, int[] unSelectedIds)
        {

            try
            {
                string accessKey = (string)Session["AccessKey"];
                Access branchOutletAccess = AccessStore.GetAccess(accessKey);
                if (branchOutletAccess == null)
                {
                    _log.Error($"In FeeTypeMapping Controller UpdateAutoApply - Access Key Not Found or not matched");
                    return Content($"Failed to UpdateAutoApply- Access Key Not Found or not matched", MediaTypeNames.Text.Plain);
                }
                if (selectedIds != null)
                {
                    foreach (int id in selectedIds)
                    {
                        await UpdateAutoApplyFeeTypeMappingById(branchOutletAccess.OutletCode, (id, true));
                    }
                }
                if (unSelectedIds != null)
                {
                    foreach (int id in unSelectedIds)
                    {
                        await UpdateAutoApplyFeeTypeMappingById(branchOutletAccess.OutletCode, (id, false));

                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                string error = $"Failed to Update Auto Apply List";
                _log.Error($"Error: {ex.ToString()} - {error} ");

                //prepare object to show error
                return Content(error, MediaTypeNames.Text.Plain);
            }
        }
        /// <summary>
        /// Update Auto Apply if existing AutoApply does not match with new auto apply to update
        /// </summary>
        /// <param name="outletCode"></param>
        /// <param name="toUpdate"></param>
        /// <returns></returns>
        private async Task UpdateAutoApplyFeeTypeMappingById(string outletCode, (int id, bool autoApply) toUpdate)
        {
            FeeTypeMapping feeTypeMapping = (await FeeTypeMappingStore.FeeTypeMapping_Get(outletCode, toUpdate.id)).FirstOrDefault();
            if (feeTypeMapping.AutoApply != toUpdate.autoApply)
            {
                feeTypeMapping.AutoApply = toUpdate.autoApply;
                await FeeTypeMappingStore.FeeTypeMapping_Save(feeTypeMapping);
            }

        }
    }
}
