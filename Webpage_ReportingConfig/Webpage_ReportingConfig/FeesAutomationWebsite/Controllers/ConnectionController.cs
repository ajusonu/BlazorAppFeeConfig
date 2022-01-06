using FeesAutomationWebsite.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Controllers
{
    public class ConnectionController : Controller
    {
        //GET: Connections Tested
        public async Task<ActionResult> Index()
        {
            List<Connection> connections = Connection.GetConnectionList();
            //foreach (Connection connection in connections)
            //{
            //    connection.FailOrSuccess = await TestConnection(connection.ConnectionString);
            //}
            return View(await RunMultipleTaskParallel(connections));
        }

        // method to run tasks in a parallel 
        public async Task<List<Connection>> RunMultipleTaskParallel(List<Connection> connections)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // normal connection strings
            List<Task> tasks = new List<Task>();
            foreach (Connection connection in connections)
            {
                connection.FailOrSuccess = "testing..";
                tasks.Add(Task.Run(async () => connection.FailOrSuccess = await TestConnectionAsync(connection.ConnectionString)));

            }

            // try all the connection strings.
            Task.WaitAll(tasks.ToArray());

            // sql deploy username / pwd are stored in the keyvault
            string sqlUser = ConfigurationManager.AppSettings["sql-deploy-username"];

            // get the sql-deploy-username from the keyvault
            string token = await MvcApplication.GetKeyVaultSecret(sqlUser);
            try
            {
                connections.Add(new Connection() { ServerName = sqlUser, FailOrSuccess = token });
            }
            catch (Exception ex)
            {
                connections.Add(new Connection() { ServerName = sqlUser, FailOrSuccess = $"Failure: {ex.Message} {Environment.NewLine} {ex.InnerException} {Environment.NewLine} {ex.ToString()}" });
            }

            // try to see if we can talk to Nexus
            string uri = ConfigurationManager.AppSettings["NexusQueueMessageBaseUri"];
            token = ConfigurationManager.AppSettings["NexusQueueMessageToken"];
#if DEBUG
            uri = uri.Replace("https://", "http://");
#endif
            try
            {
                long? id = await NexusLibrary.NexusHelper.QueueMessageRemote(uri, token, "this will fail and thats ok", "Email");
                connections.Add(new Connection() { ServerName = uri, ConnectionString = "Email", FailOrSuccess = id.HasValue ? id.Value.ToString() : "failed" });
            }
            catch (Exception ex)
            {
                connections.Add(new Connection() { ServerName = uri, ConnectionString = ex.ToString(), FailOrSuccess = "Failed" });
            }

            // check a bunch of the app settings
            connections.Add(new Connection() { ServerName = "Environment", ConnectionString = ConfigurationManager.AppSettings["Environment"] });
            connections.Add(new Connection() { ServerName = "KeyVaultName", ConnectionString = ConfigurationManager.AppSettings["KeyVaultName"] });
            connections.Add(new Connection() { ServerName = "NexusQueueMessageBaseUri", ConnectionString = ConfigurationManager.AppSettings["NexusQueueMessageBaseUri"] });
            connections.Add(new Connection() { ServerName = "FeeAutomationConfigBaseUri", ConnectionString = ConfigurationManager.AppSettings["FeeAutomationConfigBaseUri"] });
            // add a summary line
            connections.Add(new Connection() { ConnectionString = $"Time elapsed when all complete...{stopwatch.Elapsed} " });

            return await Task.FromResult(connections);
        }

        public async Task<string> TestConnectionAsync(string connectionString)
        {
            string output = "Start testing";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "select 1";

                    SqlCommand command = new SqlCommand(query, connection);

                    await connection.OpenAsync();
                    output = "SQL Connection successful.";

                    await command.ExecuteNonQueryAsync();
                    output = "SQL Query execution successful.";

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                output = $"Failure: {ex.Message} {Environment.NewLine} {ex.InnerException} {Environment.NewLine} {ex.ToString()}";
            }
            return await Task.FromResult(output);
        }

    }
}