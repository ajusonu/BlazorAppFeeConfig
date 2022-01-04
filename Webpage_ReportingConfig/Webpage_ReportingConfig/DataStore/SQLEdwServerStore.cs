using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Webpage_ReportingConfig.DataStore
{
    /// <summary>
    /// Connection String to EDW Server Datastore
    /// </summary>
    public class SQLEdwServerStore
    {
        public string ConnectionString { get; set; }
        public SQLEdwServerStore(string connStr)
        {
            ConnectionString = connStr;
        }
    }
}
