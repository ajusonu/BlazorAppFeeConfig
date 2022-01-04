using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.Data;

namespace Webpage_ReportingConfig.DataStore
{
    public class ProfitAndLossFeeStore
    {
        public static SQLEdwServerStore _conn;
        public ProfitAndLossFeeStore(SQLEdwServerStore conn)
        {
            _conn = conn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static SqlConnection GetConnection()
        {
            return new SqlConnection(_conn.ConnectionString);
        }
        /// <summary>
        /// Get all or specific Orbit Outlet from database
        /// </summary>
        public async Task<List<ProfitAndLossFee>> GetProfitAndLossFees(int brand = 0)
        {
            List<ProfitAndLossFee> fee = new List<ProfitAndLossFee>();
            try
            {
                // Connect to the apiconfig database
                using (SqlConnection conn = GetConnection())
                {
                    await conn.OpenAsync();

                    // prepare the SP
                    using (SqlCommand comm = new SqlCommand("proc_ProfitAndLossFee_Get", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("Id", brand);

                        // execute
                        using (SqlDataReader reader = await comm.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                fee.Add(MapFee(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_log.Error($" Get Outlets Error {ex.ToString()}");
                return null;
            }
            return fee;
        }
        /// <summary>
        /// Map Outlet to Model field
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Condition</returns>
        private ProfitAndLossFee MapFee(SqlDataReader reader)
        {
            ProfitAndLossFee profitAndLossFee = new ProfitAndLossFee();

            profitAndLossFee.Id = int.Parse(reader["Id"].ToString());
            profitAndLossFee.FeeValue = float.Parse(reader["FeeValue"].ToString());
            profitAndLossFee.FeePercentage = float.Parse(reader["FeePercentage"].ToString());
            profitAndLossFee.FeeNameId = int.Parse(reader["FeeNameId"].ToString());
            profitAndLossFee.Brand = (Data.Brand)reader["Brand"];
            profitAndLossFee.Name = reader["Name"].ToString();
            profitAndLossFee.Type = (Models.FeeType)reader["Type"];

            return profitAndLossFee;
        }
        /// <summary>
        /// Save Fee 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ProfitAndLossFee_Save(ProfitAndLossFee profitAndLossFee)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (SqlCommand comm = new SqlCommand("proc_ProfitAndLossFee_Save", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("Id", profitAndLossFee.Id);
                        comm.Parameters.AddWithValue("Brand", profitAndLossFee.Brand);
                        comm.Parameters.AddWithValue("FeeNameId", (int)profitAndLossFee.FeeNameId);
                        comm.Parameters.AddWithValue("FeePercentage", profitAndLossFee.FeePercentage);
                        comm.Parameters.AddWithValue("FeeValue", profitAndLossFee.FeeValue);

                        await comm.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
                // todo: log errors
            }
            return true;
        }

    }
}
