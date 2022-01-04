using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.DataStore;
using Webpage_ReportingConfig.Models;

namespace Webpage_ReportingConfig.Service
{
    /// <summary>
    /// To perform CRUD operation to add/delete Fee Names
    /// </summary>
    public class FeeNameService
    {
        IConfiguration _Configuration;
        /// <summary>
        /// Fee Name with Configuration to cooncet to SQL Server
        /// </summary>
        /// <param name="configuration"></param>
        public FeeNameService(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        /// <summary>
        /// Get list of Fee Names
        /// </summary>
        /// <returns></returns>
        public async Task<List<FeeName>> GetFeeNames(int? id)
        {
            return await (new FeeNameStore(_Configuration)).ProfitAndLoss_FeeName_Get(id);
        }
        /// <summary>
        /// Get Specific Fee name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<FeeName> GetFeeName(int? id)
        {
            FeeName feeName = (await GetFeeNames(id))?.FirstOrDefault();
            return feeName;
        }
        /// <summary>
        /// Save Fee Name
        /// </summary>
        /// <param name="feeName"></param>
        /// <returns></returns>
        public async Task<bool> SaveFeeName(FeeName feeName)
        {
            return await (new FeeNameStore(_Configuration)).ProfitAndLoss_FeeName_Save(feeName);
        }
    }
}
