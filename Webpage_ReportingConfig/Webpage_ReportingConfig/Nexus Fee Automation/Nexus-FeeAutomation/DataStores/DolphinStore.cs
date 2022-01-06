using FeeAutomationLibrary;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Nexus_FeeAutomation.DataStores
{
    public class DolphinStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DolphinStore));

        /// <summary>
        /// a reference to the Dolphin connection string name
        /// </summary>
        public static string ConnectionStringName => "Dolphin";

        /// <summary>
        /// Helper function to get the connection 
        /// </summary>
        /// <returns></returns>
        private static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                log.Error($"Application connection string {ConnectionStringName} not found or empty");
                throw new ApplicationException($"Application connection string {ConnectionStringName} not found or empty");
            }
            try
            {
                return new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                log.Error($"Error getting {ConnectionStringName} DB connection", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get a list of Fee Type Mappings
        /// </summary>
        /// <param name="branchCode"></param>
        /// <returns></returns>
        public static List<FeeTypeMapping> GetFeeTypeMappingList(string outletCode, int id)
        {
            // make a list of Fee type mappings
            List<FeeTypeMapping> mappings = new List<FeeTypeMapping>();
            try
            {
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    string sql = "DBA.proc_HOT_FeeTypeMapping_Get";
                    // read all mappings
                    using (SqlDataReader reader = SqlHelper.ExecuteReader(conn, sql,
                        new SqlParameter[] {
                            new SqlParameter("outletCode", outletCode),
                            new SqlParameter("id", id) }
                        ))
                    {
                        while (reader.Read())
                        {
                            mappings.Add(reader.AutoMap<FeeTypeMapping>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // return any errors as the content. 
                log.Error($"Error in GetFeeTypeMappingList: {ex}");
                return null;

            }
            return mappings;
        }

        /// <summary>
        /// Get Fee Type Product List
        /// </summary>
        /// <param name="FeeType"></param>
        /// <returns></returns>
        public static List<string> GetFeeTypeProductList(string FeeType)
        {
            // make a list of Fee type mappings
            List<string> productList = new List<string>();
            try
            {
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    string sql = "DBA.proc_HOT_FeeTypeProductList_Get";
                    // read all mappings
                    using (SqlDataReader reader = SqlHelper.ExecuteReader(conn, sql,
                        new SqlParameter[] {
                            new SqlParameter("FeeType", FeeType)
                        }
                        ))
                    {
                        while (reader.Read())
                        {
                            productList.Add(reader["ProductCode_FD"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // return any errors as the content. 
                log.Error($"Error in proc_HOT_FeeTypeProductList_Get: {ex}");
                throw new ApplicationException($"Error Getting Fee Product List Config: {ex}");

            }
            return productList;
        }

        /// <summary>
        /// Save or Edit Fee Type Mapping
        /// </summary>
        /// <param name="feeTypeMapping"></param>
        /// <returns>Id of Fee Type Mapping Item</returns>
        public static int SaveFeeMapping(FeeTypeMapping feeTypeMapping)
        {
            try
            {
                ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                if (string.IsNullOrEmpty(connectionString?.ConnectionString))
                {
                    throw new ApplicationException($"Application connection string '{ConnectionStringName}' not found or empty");
                }
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand sql = new SqlCommand("DBA.proc_HOT_FeeTypeMapping_Save", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("id", feeTypeMapping.Id);
                    sql.Parameters.AddWithValue("outletCode", feeTypeMapping.OutletCode);
                    sql.Parameters.AddWithValue("feeType", feeTypeMapping.FeeType);
                    sql.Parameters.AddWithValue("bookingType", feeTypeMapping.BookingType);
                    sql.Parameters.AddWithValue("description", feeTypeMapping.Description);
                    sql.Parameters.AddWithValue("queryType", feeTypeMapping.QueryType);
                    sql.Parameters.AddWithValue("scope", feeTypeMapping.Scope);
                    sql.Parameters.AddWithValue("exclusionCode", feeTypeMapping.ExclusionCode);
                    sql.Parameters.AddWithValue("AutoApply", feeTypeMapping.AutoApply);
                    string id = sql.ExecuteScalar().ToString();
                    return int.Parse(id);
                }
                // prepare and send the response
            }
            catch (Exception ex)
            {
                // return any errors as the content. 
                log.Error($"Error in SaveFeeMappingSaving fees Config: {ex}");
            }
            // if we get here, we failed :(
            return 0;
        }

        /// <summary>
        /// Delete Fee Type Mapping
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteFeeMappingItem(int id)
        {
            try
            {
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand sql = new SqlCommand("DBA.proc_HOT_FeeTypeMapping_Delete", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("id", id);
                    sql.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // return any errors as the content. 
                log.Error($"Error Deleting in DeleteFeeMappingItem fees Config: {ex}");
            }
            // if we get here, we failed :(
            return false;

        }
        /// <summary>
        /// Update Fee Type Into Folder EnhanceData Field 
        /// </summary>
        /// <param name="pendingFeeItem"></param>
        public static async Task<bool> UpdateFeeTypeIntoFolderEnhanceData(PendingFee pendingFeeItem, int retryAttempts)
        {
            string processComment = $"Update Folder Enhanced Data with Fee Type {pendingFeeItem.FeeType} Booking number {pendingFeeItem.BranchCode}{pendingFeeItem.FolderNumber}";

            try
            {
                ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                if (string.IsNullOrEmpty(connectionString?.ConnectionString))
                {
                    throw new ApplicationException($"Application connection string '{ConnectionStringName}' not found or empty");
                }
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlCommand sql = new SqlCommand("DBA.proc_HOT_UpdateFeeTypeIntoFolderEnhanceData", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    sql.Parameters.AddWithValue("BranchCode", pendingFeeItem.BranchCode);
                    sql.Parameters.AddWithValue("FolderNumber", pendingFeeItem.FolderNumber);
                    sql.Parameters.AddWithValue("FeeType", pendingFeeItem.FeeType);
                    sql.Parameters.AddWithValue("CompanyId", pendingFeeItem.CompanyId);
                    sql.Parameters.AddWithValue("HistoryComment", processComment);
                    sql.Parameters.AddWithValue("HistoryAction", "Update Enhance Data");
                    await sql.ExecuteNonQueryAsync();
                    // all done, success!
                    return await Task.FromResult(true);

                }
                // prepare and send the response
            }
            catch (Exception ex)
            {
                // return any errors as the content. 
                log.Error($"Error in {processComment} Retry Attempt {retryAttempts} in Dolphin Store: {ex}");
            }
            // if we get here, we failed :(
            // all done, success!
            return await Task.FromResult(false);
        }
    }
}
