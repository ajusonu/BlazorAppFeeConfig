using System.Configuration;
using System.Data.SqlClient;

namespace FeesAutomationWebsite.DataStores
{
    public class BaseStore
    {
        /// <summary>
        /// Get FeeAutomation Connection to be used in Sub Classes
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetFeeAutomationConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["FeeAutomation"].ConnectionString);
        }
    }
}