using FeeAutomationLibrary;
using HouseOfTravel.AutoMapper;
using NexusLibrary;
using NexusLibrary.Models.EmailPayload;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus_FeeAutomation
{
    [NexusQueue(QueueName = "FeeAutomation", Source = "*", Action = "SendPendingFeeEmail")]
    public class FeeAutomationEmailer : NexusHandler<object>
    {
        // temporary! 
        private string sqlUsername = "azure_db_user@hot.co.nz";
        private string sqlPassword = "w6Rvi*IK!VHzZGWME8J";

        /// <summary>
        /// tell the handler that we dont care about the payload
        /// </summary>
        public override bool PayloadConversionRequired => false;

        /// <summary>
        /// Building the email payload to Queue the Email message   
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async override Task<bool> ProcessMessage(CancellationToken token)
        {
            try
            {
                // temporary username/pwd for connecting to Azure DB
                string connectionString = ConfigurationManager.ConnectionStrings["FeeAutomation"].ConnectionString;
                connectionString = connectionString.Replace("{username}", sqlUsername);
                connectionString = connectionString.Replace("{password}", sqlPassword);

                // Connect to the fee automation db
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    // get the recipients
                    using (SqlCommand cmd = new SqlCommand("PendingFee_GetEmailRecipients", conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        // go through all recipients
                        while (reader.Read())
                        {
                            // send the email for each person in the list
                            EmailRecipient recipient = reader.AutoMap<EmailRecipient>();
                            Email payload = new Email();
                            payload.To = recipient.Email.Split(';').ToList();
                            string feeAutomationURL = LocalConfiguration<FeeAutomationEmailer>.AppSettings("FeeAutomationURL")?.Replace("[ACCESSKEY]", recipient.AccessKey);
                            string environment = feeAutomationURL.ToLower().Contains("dev") ? "DEV - Testing" : feeAutomationURL.ToLower().Contains("uat") ? "UAT - Testing" : "";

                            //Creating Dictionary object to be used in email HTML body 
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary.Add("{OutletCode}", recipient.OutletCode);
                            dictionary.Add("{BranchCode}", recipient.BranchCode);
                            dictionary.Add("{FeeAutomationURL}", feeAutomationURL);
                            dictionary.Add("{UnProcessedFeeCount}", recipient.UnProcessedFeeCount.ToString());
                            payload.Subject = $"{environment} Pending Fees available online for {recipient.OutletCode}/{recipient.BranchCode}";
                            payload.Body = GetEmailBody(dictionary);
                            payload.IsHtmlBody = true;
                            // queue the message into nexus to be sent
                            await NexusHelper.QueueMessage(ProcessorName, "Email", payload);
                        }
                    }
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                await AddHistory($"{ProcessorName}: Error in SendPendingFeeEmail: {ex}", MessageState.Failed);
                log.Error($"{ProcessorName}: Error in SendPendingFeeEmail: {ex}");
            }
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Get Email Body for Fee Automation Notification by filling template with the dictionary Key values
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string GetEmailBody(Dictionary<string, string> dictionary)
        {
            string emailBody = LocalConfiguration<FeeAutomationEmailer>.AppSettings("EmailTemplate");
          
            foreach (string key in dictionary.Keys)
            {
                emailBody = emailBody.Replace(key, dictionary[key]);
            }
            return emailBody;
        }
    }
}
