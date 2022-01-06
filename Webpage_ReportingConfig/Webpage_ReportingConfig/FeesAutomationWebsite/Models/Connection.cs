using System.Collections.Generic;

namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Access Model to manage Access for Branch/Outlet based on Access Key
    /// </summary>

    public class Connection
    {

        #region Properties

        public string ServerName { get; set; }
        public string ConnectionString { get; set; }

        public string FailOrSuccess { get; set; }

        #endregion

        #region Public Methods
        public static List<Connection> GetConnectionList()
        {
            List<Connection> connectionList = new List<Connection>();
            System.Configuration.ConnectionStringSettingsCollection connections = System.Configuration.ConfigurationManager.ConnectionStrings;

            if (connections.Count != 0)
            {
                foreach (System.Configuration.ConnectionStringSettings connection in connections)
                {
                    string name = connection.Name;

                    if (!name.ToLower().Contains("local"))
                    {
                        Connection conn = new Connection();

                        conn.ServerName = connection.Name;
                        conn.ConnectionString = connection.ConnectionString;
                        connectionList.Add(conn);
                    }

                }
            }

            return connectionList;
        }




        #endregion

    }
}

