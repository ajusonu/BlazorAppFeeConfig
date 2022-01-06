using FeeAutomationLibrary;
using FeesAutomationWebsite.Common;
using FeesAutomationWebsite.DataStores;
using FeesAutomationWebsite.Models;
using FeesAutomationWebsite.Models.DataTable;
using log4net;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Controllers
{
    /// <summary>
    /// Fee Setup controller
    /// </summary>
    [CustomAuthorize]
    public class FeeController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FeeController));
        /// <summary>
        /// Check for AccessKey to authorise access
        /// </summary>
        /// <returns></returns>
        [HandleError()]
        public async Task<ActionResult> Index()
        {
            string accessKey = (string)Session["AccessKey"];
            List<Access> branchOutletAccess = AccessStore.Access_Search(accessKey);
            if (branchOutletAccess?.Count == 0)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }
            return View(await FeeStore.Fee_Search(accessKey));
        }

        /// <summary>
        /// Get Response Object for Data table for Fee List based on Access Key used by User and stored in session
        /// </summary>
        /// <param name="dataTableAjaxPostModel"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<ActionResult> Fee_Search(DataTableAjaxPostModel dataTableAjaxPostModel)
        {
            string accessKey = (string)Session["AccessKey"];
            List<Fee> feesDataTableResponse = await FeeStore.Fee_Search(accessKey);
            _log.Info($"Got Fees List in FeeAutomation using Fee_Search");

            return await Task.FromResult(Json(new
            {
                data = feesDataTableResponse,
                draw = Request["draw"],
                recordsTotal = feesDataTableResponse.Count,
                recordsFiltered = feesDataTableResponse.Count
            }, JsonRequestBehavior.AllowGet));
        }

        /// <summary>
        /// Create new fee record to be saved
        /// </summary>
        /// <param name="id">This Pending Fee Item to Add UnMatch Fee</param>
        /// <returns></returns>
        public ActionResult Create(int id = 0)
        {
            // prepare a new, blank model
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.Access_Search(accessKey).FirstOrDefault<Access>();
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }
            Fee model = new Fee() { OutletCode = branchOutletAccess.OutletCode, BranchCode = branchOutletAccess.BranchCode };
            if (id != 0)
            {
                PendingFee pendingFee = PendingFeeStore.PendingFee_Get(id);
                if (pendingFee != null)
                {
                    model.FeeType = pendingFee?.ProductCode;
                    model.PricingProfile = pendingFee?.DPECode;
                    model.CompanyId = pendingFee.CompanyId;
                }
            }
            model.FeeTypes = FeeTypeStore.GetFeeTypeList();
            return View("Create", model);
        }

        /// <summary>
        /// Save new fee or update existing fee
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Fee_Save(Fee fee)
        {
            // prepare a new, blank model
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.Access_Search(accessKey).FirstOrDefault<Access>();
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }
            fee.FeeTypes = FeeTypeStore.GetFeeTypeList();
            // return the user the user listing
            if (!ModelState.IsValid)
            {
                // show the page
                return View("Create", fee);
            }
            try
            {
                if (!fee.EditMode)
                {
                    fee.Id = 0;
                }
                // Save Fee
                string error = FeeStore.Fee_Save(fee);
                if (string.IsNullOrEmpty(error))
                {
                    await FeeStore.Fee_Recalculateasync(branchOutletAccess.BranchCode);
                    return RedirectToAction("Index");
                }
                else
                {
                    fee.ErrorDescription = error;
                    return View("Create", fee);
                }
            }
            catch (Exception ex)
            {
                fee.ErrorDescription = ex.Message;
                return View("Create", fee);
            }
        }

        /// <summary>
        /// Prepare for Edit Fee for Selected Row
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int id, FormCollection formCollection)
        {
            // prepare a new, blank model
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.Access_Search(accessKey).FirstOrDefault<Access>();
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return new HttpUnauthorizedResult();
            }
            Fee model = new Fee() { OutletCode = branchOutletAccess.OutletCode, BranchCode = branchOutletAccess.BranchCode };
            if (id != 0)
            {
                model = (await FeeStore.Fee_Search(accessKey)).Find(f => f.Id == id);
            }
            model.FeeTypes = FeeTypeStore.GetFeeTypeList();
            model.BranchCode = branchOutletAccess.BranchCode;
            model.EditMode = true;
            // show the page
            return View("Create", model);
        }

        /// <summary>
        /// Delete the selected fee record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            FeeStore.Fee_Delete(id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Validating Fee entry for duplicate entry 
        /// </summary>
        /// <returns>Json object string if duplicate entry already exists </returns>
        /// 
        [HttpPost]
        public async Task<JsonResult> ValidatingFee(string pricingProfile, string companyId, string outletCode, string branchCode, string feeType)
        {
            string accessKey = (string)Session["AccessKey"];
            List<Fee> outletFees = (await FeeStore.Fee_Search(accessKey))?.FindAll(f => outletCode.Equals(f.OutletCode, StringComparison.CurrentCultureIgnoreCase) &&
                               feeType.Equals(f.FeeType, StringComparison.CurrentCultureIgnoreCase));
            if (outletFees == null || outletFees.Count() == 0)
            {
                return Json(string.Empty);
            }
            if (!string.IsNullOrEmpty(pricingProfile) || !companyId.Equals("0"))
            {
                //Match Profile or Company
                outletFees = outletFees.FindAll(f => f.CompanyId == long.Parse(companyId) && (string.IsNullOrEmpty(pricingProfile) || pricingProfile.Equals(f.PricingProfile, StringComparison.CurrentCultureIgnoreCase)));
            }
            else
            {
                //Match Branch Specific Fee
                outletFees = outletFees.FindAll(f => branchCode.Equals(f.BranchCode, StringComparison.CurrentCultureIgnoreCase) &&
                                                        f.CompanyId == 0 && string.IsNullOrEmpty(f.PricingProfile));
            }

            if (outletFees == null || outletFees.Count() == 0)
            {
                return Json(string.Empty);
            }

            return Json($"Fee Type {feeType} already Exists for selected Branch Code '{branchCode}' Profile Code '{pricingProfile}' Company Id '{companyId}' ");

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
            if (selectedIds == null && unSelectedIds == null)
            {
                return RedirectToAction("Index");
            }
            if (!await FeeStore.Fee_update_autoapplyasync(selectedIds?.Length > 0 ? string.Join(",", selectedIds) : "",
                                                      unSelectedIds?.Length > 0 ? string.Join(",", unSelectedIds) : ""))
            {
                string error = $"Failed to Update Auto Apply List";
                _log.Error(error);

                //prepare object to show error
                return Content(error, MediaTypeNames.Text.Plain);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Recalculate Fee
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Recalculate()
        {

            await RecalculateFolderFee();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Recalculate Folders Fee where fees are still to be applied
        /// For the selected Branch
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RecalculateFolderFee()
        {
            string accessKey = (string)Session["AccessKey"];
            Access branchOutletAccess = AccessStore.Access_Search(accessKey).FirstOrDefault<Access>();
            if (branchOutletAccess == null)
            {
                _log.Error($"Access Key Not Found or not matched");
                return false;
            }

            return await FeeStore.Fee_Recalculateasync(branchOutletAccess.BranchCode);
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

        /// <summary>
        /// Update the Fee file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            if (file != null)
            {
                // prepare a new, blank model
                string accessKey = (string)Session["AccessKey"];
                Access branchOutletAccess = AccessStore.Access_Search(accessKey).FirstOrDefault<Access>();
                if (branchOutletAccess == null)
                {
                    _log.Error($"Access Key Not Found or not matched");
                    throw new UnauthorizedAccessException("Access Key Not Found or not matched");
                }

                List<Fee> fees = new List<Fee>();

                // try to load the uploaded file into the spreadsheet
                UploadProgress<Fee> uploadProgress = new UploadProgress<Fee>(file.FileName, fees);

                try
                {
                    byte[] bytes = new byte[file.ContentLength];
                    file.InputStream.Read(bytes, 0, file.ContentLength);

                    string csvText = Encoding.UTF8.GetString(bytes);
                    ExcelPackage package = new ExcelPackage();
                    ExcelTextFormat csvFormat = new ExcelTextFormat()
                    {
                        TextQualifier = '"',
                        Delimiter = ',',
                        // Must set the culture to currentculture as otherwise epplus converts all dates to en-US format!!
                        Culture = CultureInfo.CurrentCulture
                    };
                    // Peek at the header so we can set the data types before loading the CSV (only way to override the
                    // default data types in EPPlus is to set the types beforehand by index)
                    // number strips them always with no way to get them back
                    int headerNewlineIndex = csvText.IndexOf(Environment.NewLine);
                    string[] headerRow = csvText.Substring(0, headerNewlineIndex).Split(',');

                    package.Workbook.Worksheets.Add("feeList");
                    package.Workbook.Worksheets[1].Cells["A1"].LoadFromText(csvText);
                    // get a reference to the sheet for easy reference
                    using (ExcelWorksheet feeExcelWorksheet = package.Workbook.Worksheets[1])
                    {
                        ColumnMapper feeColumnMapper = new ColumnMapper();
                        feeColumnMapper.Initialize(feeExcelWorksheet);

                        // setup the count (-1 because we have a header row)
                        int count = feeExcelWorksheet.Dimension.Rows - 1;
                        int columnIdRowId = feeColumnMapper.GetColumnId("Id");
                        int columnIdOutlet = feeColumnMapper.GetColumnId("Outlet");
                        int columnIdBranch = feeColumnMapper.GetColumnId("Branch");
                        int columnIdProfileCode = feeColumnMapper.GetColumnId("Profile Code");
                        int columnIdCompanyId = feeColumnMapper.GetColumnId("CompanyId");
                        int columnIdType = feeColumnMapper.GetColumnId("Type");
                        int columnIdDescription = feeColumnMapper.GetColumnId("Description");
                        int columnIdPerSegment = feeColumnMapper.GetColumnId("PerSegment");
                        int columnIdPerFolder = feeColumnMapper.GetColumnId("PerFolder");
                        int columnIdIsActive = feeColumnMapper.GetColumnId("IsActive");

                        uploadProgress.Succeeded = 0;
                        uploadProgress.Failed = 0;

                        for (int rowId = 2; rowId < feeExcelWorksheet.Dimension.Rows; rowId++)
                        {
                            Fee fee = new Fee();
                            try
                            {
                                fee.Id = feeExcelWorksheet.GetCellLongValue(rowId, columnIdRowId);
                                fee.OutletCode = feeExcelWorksheet.GetCellValue(rowId, columnIdOutlet);
                                fee.BranchCode = feeExcelWorksheet.GetCellValue(rowId, columnIdBranch);
                                fee.CompanyId = feeExcelWorksheet.GetCellLongValue(rowId, columnIdCompanyId);
                                fee.PricingProfile = feeExcelWorksheet.GetCellValue(rowId, columnIdProfileCode);
                                fee.FeeType = feeExcelWorksheet.GetCellValue(rowId, columnIdType);
                                fee.Description = feeExcelWorksheet.GetCellValue(rowId, columnIdDescription);
                                fee.FeePerFolder = feeExcelWorksheet.GetCellDollarValue(rowId, columnIdPerSegment);
                                fee.FeePerSegment = feeExcelWorksheet.GetCellDollarValue(rowId, columnIdPerFolder);
                                fee.FeePerSegment = feeExcelWorksheet.GetCellDollarValue(rowId, columnIdPerFolder);
                                fee.IsActive = feeExcelWorksheet.GetCellBooleanValue(rowId, columnIdIsActive);

                                if (!fee.OutletCode.Equals(branchOutletAccess.OutletCode, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    fee.ErrorDescription = $"Unauthorized Access for Outlet {fee.OutletCode} Exception for the AccessKey";
                                }
                                else
                                if (fee.NumberOfFeeValues != 1)
                                {
                                    fee.ErrorDescription = $"Only accept only one fee value Error Invalid FeePerFolder {fee.FeePerFolder} FeePerSegment {fee.FeePerSegment}";
                                }
                                else
                                {
                                    fee = await FeeStore.Fee_ImportAsync(fee);
                                }
                            }
                            catch (Exception ex)
                            {
                                fee.ErrorDescription = ex.ToString();
                            }
                            if (string.IsNullOrEmpty(fee.ErrorDescription))
                            {
                                uploadProgress.Succeeded += 1;
                            }
                            else
                            {
                                uploadProgress.Failed += 1;
                            }
                            fees.Add(fee);

                        }
                        uploadProgress.Filename = file.FileName;
                        uploadProgress.Count = fees.Count;
                    }

                    return Json(uploadProgress, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    uploadProgress.Failed = 0;
                    uploadProgress.Error = ex.ToString();
                    _log.Error($"Upload Error in Fee Automation {ex.ToString()}");
                    return Json(uploadProgress, JsonRequestBehavior.AllowGet);
                }
            }
            return View();
        }

    }
}
