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
    /// <summary>
    /// For Fee Automatin SQL Data Source 
    /// </summary>
    internal class FeeAutomationStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DolphinStore));
        // temporary! 
        private static string sqlUsername = "azure_db_user@hot.co.nz";
        private static string sqlPassword = "w6Rvi*IK!VHzZGWME8J";

        /// <summary>
        /// a reference to the Dolphin connection string name
        /// </summary>
        public static string ConnectionStringName => "FeeAutomation";

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
            // temporary username/pwd for connecting to Azure DB
            try
            {
                connectionString = connectionString.Replace("{username}", sqlUsername);
                connectionString = connectionString.Replace("{password}", sqlPassword);
                return new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                log.Error($"In FeeAutomationStore Error getting {ConnectionStringName} DB connection", ex);
                throw ex;
            }
        }
        /// <summary>
        /// Save Pending Fee in PendingFee table
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        public static async Task<bool> PendingFeeSave(PendingFee fee)
        {
            try
            {
                // Connect to the fee automation db
                using (SqlConnection conn = GetConnection())
                {
                    await conn.OpenAsync();
                    // call the sp to save the record
                    SqlCommand sql = new SqlCommand("PendingFee_Save", conn);
                    sql.CommandType = CommandType.StoredProcedure;
                    sql.Parameters.AddWithValue("id", fee.Id);
                    sql.Parameters.AddWithValue("dateStamp", fee.DateStamp);
                    sql.Parameters.AddWithValue("dpeCode", fee.DPECode);
                    sql.Parameters.AddWithValue("officeId", fee.OfficeId);
                    sql.Parameters.AddWithValue("outletCode", fee.OutletCode);
                    sql.Parameters.AddWithValue("branchCode", fee.BranchCode);
                    sql.Parameters.AddWithValue("folderNo", fee.FolderNumber);
                    sql.Parameters.AddWithValue("itemId", fee.ItemId);
                    sql.Parameters.AddWithValue("itemType", fee.ItemType);
                    sql.Parameters.AddWithValue("feeType", fee.FeeType);
                    sql.Parameters.AddWithValue("duration", fee.Duration);
                    sql.Parameters.AddWithValue("status", fee.Status);
                    sql.Parameters.AddWithValue("description", fee.Description);
                    sql.Parameters.AddWithValue("companyId", fee.CompanyId);
                    sql.Parameters.AddWithValue("companyName", fee.CompanyName);
                    sql.Parameters.AddWithValue("folderCreationDate", fee.FolderCreation);
                    sql.Parameters.AddWithValue("folderOwner", fee.FolderOwner);
                    sql.Parameters.AddWithValue("bookingType", fee.BookingType);
                    sql.Parameters.AddWithValue("cancellationReason", fee.CancellationReason);
                    sql.Parameters.AddWithValue("category", fee.Category);
                    sql.Parameters.AddWithValue("scope", fee.Scope);
                    sql.Parameters.AddWithValue("autoApply", fee.AutoApply);
                    await sql.ExecuteNonQueryAsync();
                }
                // all done, success!
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                log.Error($"In FeeAutomationStore Error saving PendingFee Id:{fee.Id} {ex}");
            }

            // if we get here, we failed :(
            return await Task.FromResult(false);
        }
        /// <summary>
        /// Get the Pending Fees for given folder with Fee value to apply fee
        /// </summary>
        /// <param name="pendingFee"></param>
        /// <returns></returns>
        public static List<PendingFee> PendingFee_Get_WithFeeValue(long folderNumber)
        {
            List<PendingFee> PendingFees = new List<PendingFee>();
            try
            {
                // make a connection and dispose of it when done
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    // make a reader and get ALL of the messages in state "NEW"
                    using (SqlDataReader reader = SqlHelper.ExecuteReader(conn, "PendingFee_Get_WithFeeValue",
                        new SqlParameter("folderNo", folderNumber)))
                    {
                        while (reader.Read())
                        {
                            PendingFees.Add(reader.AutoMap<PendingFee>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"In FeeAutomationStore Error Get With Fee Value: Pending Fee Folder no {folderNumber}  {ex}"); 
                return null;

            }
            return PendingFees;



        }
       /// <summary>
       /// Update Pending Fee Row Status eg (Fee Applied or Cancelled) with Date Actioned on 
       /// </summary>
       /// <param name="id"></param>
       /// <param name="status"></param>
       /// <param name="reason"></param>
       /// <returns></returns>
        public static bool PendingFee_UpdateStatus(long id, string status, string reason = "")
        {
            try
            {
                // Connect to the fee automation db
                using (SqlConnection conn = GetConnection())
                {
                    SqlHelper.ExecuteNonQuery(conn, "PendingFee_UpdateStatus",
                        new SqlParameter("id", id),
                        new SqlParameter("newFeeStatus", status.ToString()),
                        new SqlParameter("Reason", reason),
                        new SqlParameter("ActionDate", DateTime.Now)
                        );
                }
                return true;
            }
            catch (Exception ex)
            {
                string error = $"Fee Automation Error: Failed to update Fee Status {status} for Pending Fee id {id} {ex.ToString()}.";
                log.Error(error);
                return false;
            }
        }

        /// <summary>
        /// Archive Old Pending Fee rows
        /// </summary>
        /// <returns></returns>
        public static async Task<int> PendingFee_Archive()
        {
            int rowsArchived = 0;
            log.Info($"Fee Automation: Archiving Peding old Rows");
            try
            {
                // Connect to the fee automation db
                using (SqlConnection conn = GetConnection())
                {
                    await conn.OpenAsync();
                    // call the sp to Archive old rows
                    SqlCommand sql = new SqlCommand("PendingFee_Archive", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    rowsArchived = (int)await sql.ExecuteScalarAsync();
                    log.Info($"Fee Automation: Total rows Archived {rowsArchived}");
                }
                // all done, success!
                return await Task.FromResult(rowsArchived);
            }
            catch (Exception ex)
            {
                string error = $"Fee Automation Error: Failed to Archive Old Pending Fees {ex.ToString()}.";
                log.Error(error);
                return await Task.FromResult(rowsArchived);
            }
        }
    }
}
