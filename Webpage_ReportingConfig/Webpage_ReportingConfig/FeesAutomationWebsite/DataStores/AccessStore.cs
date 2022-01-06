using FeesAutomationWebsite.Models;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace FeesAutomationWebsite.DataStores
{
    /// <summary>
    /// Access store to manage Access table based on AssessKey
    /// </summary>
    public class AccessStore : BaseStore
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AccessStore));
        /// <summary>
        /// Get the Access Branch Code and OutletCode based on accessKey
        /// </summary>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        public static List<Access> Access_Search(string accessKey)
        {
            using (SqlConnection connection = GetFeeAutomationConnection())
            {
                connection.Open();
                List<Access> accessList = new List<Access>();
                // create the GetFee stored proc
                try
                {
                    string sql = "Access_Search";

                    using (
                        SqlDataReader reader = SqlHelper.ExecuteReader(connection, sql,
                        new SqlParameter("@accessKey", string.IsNullOrEmpty(accessKey) ? "" : accessKey)))
                    {
                        while (reader.Read())
                        {
                            // prepare and return the value using Auto Mapper
                            accessList.Add(reader.AutoMap<Access>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in FeeAutomation Access_Search {ex.ToString()}");
                }
                connection.Close();
                return accessList;
            }
        }
        /// <summary>
        /// Check if matching access Key exists return Access Object else null
        /// </summary>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        public static Access GetAccess(string accessKey)
        {
            List<Access> branchOutletAccess = AccessStore.Access_Search(accessKey);
            if (branchOutletAccess == null || branchOutletAccess?.Count == 0)
            {
                _log.Error($"Access Key Not Found or not matched");
                return null;
            }
            return branchOutletAccess.FirstOrDefault();
        }
    }
}
