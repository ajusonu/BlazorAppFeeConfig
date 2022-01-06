using FeeAutomationLibrary;
using FeesAutomationWebsite.Models;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace FeesAutomationWebsite.DataStores
{
    /// <summary>
    /// Fee store to manage Fee Configuation based on AssessKey
    /// </summary>
    public class FeeStore : BaseStore
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FeeStore));
        /// <summary>
        /// Get the Fee Setup for a Branch Code and OutletCode based on accessKey
        /// </summary>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        public static async Task<List<Fee>> Fee_Search(string accessKey)
        {
            using (SqlConnection connection = GetFeeAutomationConnection())
            {
                connection.Open();
                List<Fee> feeList = new List<Fee>();
                // Fee_Search proc get fee configuration list for selected branch outlet
                try
                {
                    string sql = "Fee_Search";

                    using (
                        SqlDataReader reader = SqlHelper.ExecuteReader(connection, sql,
                        new SqlParameter("@accessKey", string.IsNullOrEmpty(accessKey) ? "" : accessKey)))
                    {
                        List<FeeTypeMapping> feeTypeMappings = null;
                        while (reader.Read())
                        {
                            // prepare and return the value using Auto Mapper
                            Fee fee = reader.AutoMap<Fee>();
                            try
                            {
                                if (feeTypeMappings == null)
                                {
                                    feeTypeMappings = await FeeTypeMappingStore.FeeTypeMapping_Get(fee.OutletCode);
                                }
                                if (feeTypeMappings != null)
                                {
                                    fee.EditAutoApply = feeTypeMappings.Any(f => f.AutoApply && f.FeeType.Equals(fee.FeeType, StringComparison.OrdinalIgnoreCase));
                                }
                            }
                            catch (Exception Ex)
                            {
                                _log.Error($"Setting EditAutoApply failed in Fee_Search {Ex.Message}");
                            }
                            feeList.Add(fee);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in FeeAutomation Fee_Search {ex.ToString()}");
                }
                connection.Close();
                return feeList;
            }
        }

        /// <summary>
        /// Add or Update the fee record based on user input
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        public static string Fee_Save(Fee fee)
        {
            string error = string.Empty;
            try
            {
                using (SqlConnection conn = GetFeeAutomationConnection())
                {
                    conn.Open();
                    // call the sp to save the record
                    SqlCommand sql = new SqlCommand("Fee_Save", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("Id", fee.Id);
                    sql.Parameters.AddWithValue("OutletCode", fee.OutletCode);
                    sql.Parameters.AddWithValue("PricingProfile", fee.PricingProfile == null ? "" : fee.PricingProfile);
                    sql.Parameters.AddWithValue("BranchCode", fee.BranchCode == null ? "" : fee.BranchCode);
                    sql.Parameters.AddWithValue("FeeType", fee.FeeType);
                    sql.Parameters.AddWithValue("CompanyId", fee.CompanyId);
                    sql.Parameters.AddWithValue("FeePerSegment", fee.FeePerSegment);
                    sql.Parameters.AddWithValue("FeePerPNR", fee.FeePerPNR);
                    sql.Parameters.AddWithValue("FeePerFolder", fee.FeePerFolder);
                    sql.Parameters.AddWithValue("FeePerDuration", fee.FeePerDuration);
                    sql.Parameters.AddWithValue("AutoApply", fee.AutoApply);
                    sql.ExecuteNonQuery();
                }
                // all done, success!
                return error;
            }
            catch (Exception ex)
            {
                _log.Error($"Error in Fee Automation Fee Save: {ex}");
                error = ex.Message;


            }
            // if we get here, we failed :(
            return error;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Fee_Recalculateasync(string branch)
        {
            try
            {
                using (SqlConnection conn = GetFeeAutomationConnection())
                {
                    conn.Open();
                    // call the sp to save the record
                    SqlCommand sql = new SqlCommand("Fee_Recalculate", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("Branch", branch);
                    await sql.ExecuteNonQueryAsync();
                }
                // all done, success!
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.Error($"Error in Fee Automation Fee Recalculate {ex}");

            }
            // if we get here, we failed :(
            return await Task.FromResult(false);
        }
        /// <summary>
        /// Set Reset Auto Apply based on multiple selection
        /// </summary>
        /// <param name="idsTicked"></param>
        /// <param name="idsUnTicked"></param>
        /// <returns></returns>
        public static async Task<bool> Fee_update_autoapplyasync(string idsTicked, string idsUnTicked)
        {
            try
            {
                using (SqlConnection conn = GetFeeAutomationConnection())
                {
                    conn.Open();
                    // call the sp to save the record
                    SqlCommand sql = new SqlCommand("Fee_update_autoapply", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("IdsTicked", idsTicked);
                    sql.Parameters.AddWithValue("IdsUnTicked", idsUnTicked);
                    await sql.ExecuteNonQueryAsync();
                }
                // all done, success!
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.Error($"Error in Fee Automation Fee Update Autoapply: {ex}");
           
            }
            // if we get here, we failed :(
            return await Task.FromResult(false);
        }
        /// <summary>
        /// Add or Update the fee record based on CSV file
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        public async static Task<Fee> Fee_ImportAsync(Fee fee)
        {
            string error = string.Empty;
            try
            {
                using (SqlConnection conn = GetFeeAutomationConnection())
                {
                    conn.Open();
                    // call the sp to save the record
                    SqlCommand sql = new SqlCommand("Fee_Import", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("Id", fee.Id);
                    sql.Parameters.AddWithValue("OutletCode", fee.OutletCode);
                    sql.Parameters.AddWithValue("PricingProfile", fee.PricingProfile == null ? "" : fee.PricingProfile);
                    sql.Parameters.AddWithValue("BranchCode", fee.BranchCode == null ? "" : fee.BranchCode);
                    sql.Parameters.AddWithValue("FeeType", fee.FeeType);
                    sql.Parameters.AddWithValue("CompanyId", fee.CompanyId);
                    sql.Parameters.AddWithValue("FeePerSegment", fee.FeePerSegment);
                    sql.Parameters.AddWithValue("FeePerPNR", fee.FeePerPNR);
                    sql.Parameters.AddWithValue("FeePerFolder", fee.FeePerFolder);
                    sql.Parameters.AddWithValue("IsActive", fee.IsActive);

                    using (SqlDataReader reader = await sql.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            fee = reader.AutoMap<Fee>();
                        }
                    }
                    // all done, success!
                    return fee;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in Fee Automation Fee Import: {ex}");
                fee.ErrorDescription = ex.Message;


            }
            // if we get here, we failed :(
            return fee;
        }
        /// <summary>
        /// Delete the selected Fee Entry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Fee_Delete(long id)
        {
            try
            {
                using (SqlConnection conn = GetFeeAutomationConnection())
                {
                    conn.Open();
                    // call sp to delete the record
                    SqlCommand sql = new SqlCommand("Fee_Delete", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("Id", id);
                    sql.ExecuteNonQuery();
                }
                // all done, success!
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Error in Fee Automation Fee Delete: {ex}");

            }
            // if we get here, we failed :(
            return false;
        }
    }
}
