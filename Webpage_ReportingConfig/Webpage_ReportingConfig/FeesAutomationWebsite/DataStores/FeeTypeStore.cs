using FeesAutomationWebsite.Models;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FeesAutomationWebsite.DataStores
{
    /// <summary>
    /// Fee Type store to manage Fee Configuation based 
    /// </summary>
    public class FeeTypeStore : BaseStore
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AccessStore));
        /// <summary>
        /// Get Fee Type list to be used in lookup for adding fee
        /// </summary>
        /// <returns></returns>
        /// <param name="addEmptyItem"></param>
        /// <returns></returns>
        public static List<FeeType> GetFeeTypeList(bool addEmptyItem = true)
        {
            using (SqlConnection connection = GetFeeAutomationConnection())
            {
                connection.Open();
                List<FeeType> feeTypeList = new List<FeeType>();
                if (addEmptyItem)
                {
                    feeTypeList.Add(new FeeType { Id = 0, Code = string.Empty, Description = "- - - Please Select - - -" });
                }
                try
                {
                    string sql = "GetFeeType";

                    using (
                        SqlDataReader reader = SqlHelper.ExecuteReader(connection, sql))
                    {
                        while (reader.Read())
                        {
                            // prepare and return the value using Auto Mapper
                            feeTypeList.Add(reader.AutoMap<FeeType>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in FeeAutomation GetFeeType {ex.ToString()}");
                }
                connection.Close();
                return feeTypeList;
            }
        }
    }
}
