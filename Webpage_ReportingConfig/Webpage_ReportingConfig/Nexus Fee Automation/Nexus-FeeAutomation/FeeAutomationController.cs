using FeeAutomationLibrary;
using NexusLibrary;
using NexusLibrary.Authorisation;
using NexusListenerHost.OAuth;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Nexus_FeeAutomation
{
    public class FeeAutomationController : NexusController
    {
        public override string ControllerName => nameof(FeeAutomationController);

        /// <summary>
        /// Get a list of Fee Type Mappings
        /// </summary>
        /// <param name="outletCode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("feeAutomation/config/{outletCode=outletCode}/{id=id}")]
        [HttpGet]
        [Authorize]
        [CheckTokenValidity]
        [ClaimRequired(ClaimType = "FeeAutomation")]
        public HttpResponseMessage ConfigGet(string outletCode, int id)
        {
            HttpResponseMessage response;
            // make a list of Fee type mappings
            List<FeeTypeMapping> mappings = new List<FeeTypeMapping>();
            mappings = DataStores.DolphinStore.GetFeeTypeMappingList(outletCode, id);
            if (mappings != null)
            {
                // prepare and send the response
                response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<List<FeeTypeMapping>>(mappings, new JsonMediaTypeFormatter(), "application/json")
                };
                return response;
            }
            else
            {
                // return any errors as the content. 
                log.Error($"Error Getting fees Config Outlet Code {outletCode} id:{id}");
                response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Error Getting fees Config Outlet Code {outletCode} id:{id}")
                };
                return response;
            }
        }

        /// <summary>
        /// Save or Edit Fee Type Mapping
        /// </summary>
        /// <param name="feeTypeMapping"></param>
        /// <returns></returns>
        [Route("feeAutomation/config")]
        [HttpPost]
        [Authorize]
        [CheckTokenValidity]
        [ClaimRequired(ClaimType = "FeeAutomation")]
        public HttpResponseMessage ConfigSave(FeeTypeMapping feeTypeMapping)
        {
            HttpResponseMessage response;
            feeTypeMapping.Id = DataStores.DolphinStore.SaveFeeMapping(feeTypeMapping);
            if (feeTypeMapping.Id != 0)
            {
                response = Request.CreateResponse<FeeTypeMapping>(HttpStatusCode.Created, feeTypeMapping);
                // prepare and send the response
                return response;
            }
            else
            {
                // return any errors as the content. 
                string errorContent = $"Error Saving fees Config: {new ObjectContent<FeeTypeMapping>(feeTypeMapping, new JsonMediaTypeFormatter(), "application/json")}";
                log.Error(errorContent);
                response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(errorContent)
                };
                return response;
            }
        }

        /// <summary>
        /// Delete Config with given Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("feeAutomation/config/{id}")]
        [HttpDelete]
        [Authorize]
        [CheckTokenValidity]
        [ClaimRequired(ClaimType = "FeeAutomation")]
        public HttpResponseMessage ConfigDelete(int id)
        {
            HttpResponseMessage response;
            if (DataStores.DolphinStore.DeleteFeeMappingItem(id))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                log.Error($"Error Deleting fees Config with Id: {id}");
                response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Error Deleting fees Config with Id: {id}")
                };
                return response;
            }


        }
    }
}
