using FeeAutomationLibrary;
using FeesAutomationWebsite.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FeesAutomationWebsite.DataStores
{

    /// <summary>
    /// Calling Fee Automation Config API in Nexus Fee Automation plugin to setup Fee configuration
    /// </summary>
    public class FeeTypeMappingStore : BaseStore
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FeeTypeMappingStore));

        /// <summary>
        /// Get Fee Type Mapping based on Access Level
        /// </summary>
        /// <param name="outletCode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<FeeTypeMapping>> FeeTypeMapping_Get(string outletCode, int id = 0)
        {
            List<FeeTypeMapping> feeTypeMappings = new List<FeeTypeMapping>();

            Uri feeAutomationConfigURL = GetFeeAutomatinConfigUri();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = feeAutomationConfigURL;
                client.DefaultRequestHeaders.Authorization = GetFeeAutomationConfigAuthenticationHeaderValue(); 

                using (HttpResponseMessage httpResponse = await client.GetAsync($"{feeAutomationConfigURL.ToString()}?outletCode={outletCode}&id={id}"))
                {

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        feeTypeMappings = await httpResponse.Content.ReadAsAsync<List<FeeTypeMapping>>();
                    }
                    else
                    {
                        _log.Error($"Error in FeeAutomation FeeTypeMapping_Get {httpResponse.ReasonPhrase}");
                        throw new Exception(httpResponse.ReasonPhrase);
                    }
                }
            }

            return feeTypeMappings;
        }

        /// <summary>
        /// gets the Fee Automatin Config url from web Config or throws an exception if not specified.
        /// </summary>
        /// <returns></returns>
        public static Uri GetFeeAutomatinConfigUri()
        {
            string feeAutomationConfigURL = ConfigurationManager.AppSettings["FeeAutomationConfigBaseUri"];
            //Get Fee Automation Config URL
            if (!string.IsNullOrEmpty(feeAutomationConfigURL))
            {
                return new Uri(feeAutomationConfigURL);
            }
            _log.Error($"FeeAutomationConfigURL missing from Fee Automation Website configuration");
            throw new ApplicationException($"FeeAutomationConfigURL missing from Fee Automation Website configuration");
        }

        /// <summary>
        /// Delete Fee Type Mapping Item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<bool> FeeTypeMapping_Delete(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"{GetFeeAutomatinConfigUri()}{id}/");
                    client.DefaultRequestHeaders.Authorization = GetFeeAutomationConfigAuthenticationHeaderValue();
                    using (HttpResponseMessage httpResponse = await client.DeleteAsync(""))
                    {
                        return httpResponse.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in FeeAutomation FeeTypeMapping_Delete {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Get FeeAutomation Config Token from App Settings
        /// </summary>
        /// <returns></returns>
        private static AuthenticationHeaderValue GetFeeAutomationConfigAuthenticationHeaderValue()
        {
            //For Token Length =30 - Scheme should be  "Nexus" otherwise "Bearer"
            string token = ConfigurationManager.AppSettings["FeeAutomationConfigToken"];
            return new AuthenticationHeaderValue(token.Length == 30 ? "Nexus" : "Bearer", token);
        }

        /// <summary>
        /// Save Fee Type Mapping item
        /// </summary>
        /// <param name="feeTypeMapping"></param>
        /// <returns>Error if any</returns>
        public static async Task FeeTypeMapping_Save(FeeTypeMapping feeTypeMapping)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = GetFeeAutomatinConfigUri();
                    client.DefaultRequestHeaders.Authorization = GetFeeAutomationConfigAuthenticationHeaderValue();
                    using (HttpResponseMessage httpResponse = await client.PostAsJsonAsync<FeeTypeMapping>("", feeTypeMapping))
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            feeTypeMapping = await httpResponse.Content.ReadAsAsync<FeeTypeMapping>();
                        }
                        else
                        {
                            string error = await httpResponse.Content.ReadAsStringAsync();
                            _log.Error($"Error in FeeAutomation FeeTypeMapping_Save {error}");
                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in FeeAutomation FeeTypeMapping_Save {ex.ToString()}");
                throw new Exception(ex.ToString());
            }
        }
        /// <summary>
        /// Get Booking Type List to use in Lookup
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetBookingTypeList()
        {
            List<SelectListItem> bookingTypeList = new List<SelectListItem>();
            foreach (BookingType val in Enum.GetValues(typeof(FeesAutomationWebsite.Models.BookingType)))
            {
                bookingTypeList.Add(new SelectListItem() { Text = val.ToDescription(), Value = val.ToDescription() });
            }
            return bookingTypeList;
        }
    }

}