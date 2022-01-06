using FeeAutomationLibrary;
using FeesAutomationWebsite.Models;
using FeesAutomationWebsite.Models.DataTable;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FeesAutomationWebsite.DataStores
{
    /// <summary>
    /// Pending Fee store to manage Pending Fee ta 
    /// </summary>
    public class PendingFeeStore : BaseStore
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PendingFeeStore));

        /// <summary>
        /// <returns>Get the tableResponse with Pending fees List, or blank list if no match is found</returns>
        /// </summary>
        /// <param name="pendingFeeSearchModel"></param>
        /// <returns></returns>
        public static DataTableResponse<PendingFeeExtended> PendingFee_Search(PendingFeeFilterDataTableAjaxPostModel pendingFeeSearchModel)
        {
            string searchText = pendingFeeSearchModel?.Search?.Value != null ? pendingFeeSearchModel.Search.Value : string.Empty;
            int orderByColumnIndex = pendingFeeSearchModel.Order == null ? 0 : pendingFeeSearchModel.Order[0].Column;
            string orderDirection = pendingFeeSearchModel.Order[0].Dir;
            string feeStatus = string.IsNullOrEmpty(pendingFeeSearchModel.FeeStatus) ? "" : pendingFeeSearchModel.FeeStatus;
            string category = string.IsNullOrEmpty(pendingFeeSearchModel.Category) ? "" : pendingFeeSearchModel.Category;
            int indexFeeStatus = searchText.IndexOf("feeStatus=", StringComparison.CurrentCultureIgnoreCase);
            if (indexFeeStatus >= 0)
            {
                feeStatus = searchText.ToLower().Replace("feestatus=", "");
                feeStatus = searchText.ToLower().Contains("all") ? "" : feeStatus;
                searchText = "";
            }
            int resultsCount = 0;
            int filteredResultsCount = 0;

            List<PendingFeeExtended> pendingFees = new List<PendingFeeExtended>();
            using (SqlConnection connection = GetFeeAutomationConnection())
            {
                connection.Open();
                // create the GetFee stored proc
                try
                {
                    // setup the sp
                    string storedProcedureName = "PendingFee_Search";
                    // call the sp, and get the result
                    SqlParameter[] sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter("@accessKey", (string)HttpContext.Current.Session["AccessKey"] ?? ""),
                        new SqlParameter("@searchPhrase", searchText),
                        new SqlParameter("@orderByColumnIndex", orderByColumnIndex),
                        new SqlParameter("@orderDirection", orderDirection),
                        new SqlParameter("@FeeStatus", feeStatus),
                        new SqlParameter("@category", category)
                    };

                    using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, storedProcedureName, sqlParameters))
                    {
                        while (reader.Read())
                        {
                            // prepare and return the value
                            PendingFeeExtended pendingFee = reader.AutoMap<PendingFeeExtended>();
                            pendingFee.FolderCreation = pendingFee.FolderCreation;
                            //match status with FeeStatus Enum to get matching FeeStatus
                            string status = Enum.GetNames(typeof(FeeStatus)).ToList().Find(s => s.Equals(pendingFee.Status, StringComparison.CurrentCultureIgnoreCase));
                            pendingFee.Status = string.IsNullOrEmpty(status) ? pendingFee.Status : status;
                            pendingFees.Add(pendingFee);
                            resultsCount = (int)reader["UnFilteredRowCount"];
                            filteredResultsCount = (int)reader["FilteredRowCount"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in FeeAutomation in PendingFee_Search {ex.ToString()} ");
                }
                connection.Close();
            }
            DataTableResponse<PendingFeeExtended> tableResponse = new DataTableResponse<PendingFeeExtended>();
            tableResponse.data = pendingFees;
            tableResponse.draw = pendingFeeSearchModel.Draw;
            tableResponse.recordsTotal = resultsCount;
            tableResponse.recordsFiltered = filteredResultsCount;

            return tableResponse;
        }
       
        /// <summary>
        /// </summary>
        /// <returns>Update the Status to be Pending/Queued Or Cancelled
        /// <param name="id"></param>
        /// <param name="status"></param>
        public static void PendingFee_UpdateStatus(int id, FeeStatus status)
        {
            try
            {
                using (SqlConnection connection = GetFeeAutomationConnection())
                {
                    SqlHelper.ExecuteNonQuery(connection, "PendingFee_UpdateStatus",
                        new SqlParameter("id", id),
                        new SqlParameter("newFeeStatus", status.ToString()));
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in FeeAutomation in PendingFee_UpdateStatus {ex.ToString()}");
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>Get the PendFee Details 
        /// <param name="id"></param>
        public static PendingFee PendingFee_Get(int id)
        {
            try
            {
                SqlConnection connection = GetFeeAutomationConnection();
                // setup the sp
                string storedProcedureName = "PendingFee_Get";
                // call the sp, and get the result
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                         new SqlParameter("id", id)
                };
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, storedProcedureName, sqlParameters))
                {
                    while (reader.Read())
                    {
                        // prepare and return the value
                        return reader.AutoMap<PendingFee>();
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _log.Error($"Error in FeeAutomation in PendingFee_Get {ex.ToString()} ");
            }

            return null;
        }
        /// <summary>
        /// Get Fee Categories to used in dropdown to filter Pending fee Search
        /// </summary>
        /// <returns></returns>
        public static List<FeeCategory> GetFeeTypeList(bool addEmptyItem = true)
        {
            using (SqlConnection connection = GetFeeAutomationConnection())
            {
                connection.Open();
                List<FeeCategory> feeCategoryList = new List<FeeCategory>();
                if (addEmptyItem)
                {
                    feeCategoryList.Add(new FeeCategory { Category = string.Empty, Description = "All Categories" });
                }
                try
                {
                    string sql = "GetFeeCategory";

                    using (
                        SqlDataReader reader = SqlHelper.ExecuteReader(connection, sql))
                    {
                        while (reader.Read())
                        {
                            // prepare and return the value using Auto Mapper
                            feeCategoryList.Add(reader.AutoMap<FeeCategory>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in FeeAutomation GetFeeCategory {ex.ToString()}");
                }
                connection.Close();
                return feeCategoryList;
            }
        }
    }
}
