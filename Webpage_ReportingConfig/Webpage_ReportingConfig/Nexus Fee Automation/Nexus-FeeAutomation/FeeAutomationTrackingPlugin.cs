using FeeAutomationLibrary;
using HouseOfTravel.AutoMapper;
using HouseOfTravel.Data;
using NexusLibrary;
using NexusLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus_FeeAutomation
{
    /// <summary>
    /// listens for new records in the DBA.HOT_PendingFees table that are in a state of "NEW". These messages are then queued to be placed into the Fee table in Azure
    /// </summary>
    public class FeeAutomationTrackingPlugin : SqlDependencyHandlerPlugin
    {
        /// <summary>
        /// a reference to the Dolphin connection string name
        /// </summary>
        public override string ConnectionStringName => "Dolphin";

        /// <summary>
        /// the name of the dependency sp that will be used to listen for new fees to be processed
        /// </summary>
        public override string DependencyStoredProcName => "DBA.proc_HOT_PendingFees_Dependency";

        /// <summary>
        /// the friendly name of the plugin for loggins purposes
        /// </summary>
        public override string Name => "FeeAutomationChangeTrackingPlugin";

        /// <summary>
        /// Deal with any new fees (as a group, not single records) that need to be inserted into the Fee table.
        /// </summary>
        /// <returns></returns>
        public async override Task ProcessingRequired()
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (string.IsNullOrEmpty(connectionString?.ConnectionString))
            {
                throw new ApplicationException($"Application connection string '{ConnectionStringName}' not found or empty");
            }

            // create the new list of fees
            List<PendingFee> fees = new List<PendingFee>();

            // flag indicating if we need to send the email
            bool sendEmail = false;

            // make a connection and dispose of it when done
            using (SqlConnection conn = new SqlConnection(connectionString.ConnectionString))
            {
                // make a reader and get ALL of the messages in state "NEW"
                using (SqlDataReader reader = SqlHelper.ExecuteReader(conn, "DBA.proc_HOT_PendingFees_Dequeue"))
                {

                    // this has read ALL of the fees with a state of "NEW" so dump them into a message in Nexus which will put them into the table in Azure (allowing for retries along the way)
                    while (reader.Read())
                    {
                        try
                        {
                            fees.Add(reader.AutoMap<PendingFee>());

                            if (fees.Count > 0 && (fees.Count % 500 == 0))
                            {
                                // if we hit a multiple of 500 fees, then save them so that we dont overload the content field in the database
                                // Queue the new message for the next process to pick up and deal with.
                                await NexusHelper.QueueMessage("FeeAutomation", "TransferPendingFees", fees);
                                fees.Clear();
                                if (fees.Any(f => f.Status.Equals("New", StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    sendEmail = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error($"{Name}:Error dequeing fees: {ex}");
                        }
                    }
                }
            }

            // if there are any remaining fees, then save them.
            if (fees.Count > 0)
            {
                // Queue the new message for the next process to pick up and deal with.
                await NexusHelper.QueueMessage("FeeAutomation", "TransferPendingFees", fees);
                if (fees.Any(f => f.Status.Equals("New", StringComparison.CurrentCultureIgnoreCase)))
                {
                    sendEmail = true;
                }
            }

            // if we need to send the pending fee email, then do it after all the other fees have been transferred
            if (sendEmail)
            {
                await NexusHelper.QueueMessage("FeeAutomation", "SendPendingFeeEmail", string.Empty);
            }
        }
    }
}
