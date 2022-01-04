using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.Models;

namespace Webpage_ReportingConfig.DataStore
{
    /// <summary>
    /// Fee Name Store to Get fee Name list and add new Fee names
    /// </summary>
    public class FeeNameStore
    {
        IConfiguration _Configuration;
        public FeeNameStore(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        /// <summary>
        /// Get Fee Names
        /// </summary>
        /// <returns></returns>
        public async Task<List<FeeName>> ProfitAndLoss_FeeName_Get(int? id)
        {
            List<FeeName> feeNames = new List<FeeName>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_Configuration.GetConnectionString("EDWServer")))
                {
                    await conn.OpenAsync();
                    using (SqlCommand comm = new SqlCommand("proc_ProfitAndLossFeeName_Get", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("Id", id);
                        using (SqlDataReader reader = await comm.ExecuteReaderAsync())
                        {
                          
                            while (reader.Read())
                            {
                                feeNames.Add(MapFeeName(reader));
                            }
                           

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                // todo: log errors
            }
            return feeNames;
        }
        /// <summary>
        /// Save Fee Name
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ProfitAndLoss_FeeName_Save(FeeName feeName)
        {
            List<FeeName> feeNames = new List<FeeName>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_Configuration.GetConnectionString("EDWServer")))
                {
                    await conn.OpenAsync();
                    using (SqlCommand comm = new SqlCommand("proc_ProfitAndLossFeeName_Save", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("Id", feeName.Id);
                        comm.Parameters.AddWithValue("Name", feeName.Name);
                        comm.Parameters.AddWithValue("Type", (int)feeName.Type);

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
        /// <summary>
        /// Map Fee Name
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private FeeName MapFeeName(SqlDataReader reader)
        {
            FeeName feeName = new FeeName();

            feeName.Id = int.Parse(reader["Id"].ToString());
            feeName.Name = reader["Name"].ToString();
            feeName.Type = (Models.FeeType)reader["Type"];
            return feeName;
        }

    }
}
