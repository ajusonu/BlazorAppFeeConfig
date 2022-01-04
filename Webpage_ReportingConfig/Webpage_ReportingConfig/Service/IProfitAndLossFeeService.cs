using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.Data;

namespace Webpage_ReportingConfig.Service
{
    /// <summary>
    /// 
    /// </summary>
    interface IProfitAndLossFeeService
    {
        /// <summary>
        /// Get all the Profit and loss settings
        /// </summary>
        /// <returns></returns>
        Task<List<ProfitAndLossFee>> GetProfitAndLossFees();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProfitAndLossFee> GetProfitAndLossFee(int id = 0);
        /// <summary>
        /// Save
        /// </summary>
        /// <param name="profitAndLossFee"></param>
        /// <returns></returns>
        Task<bool> SaveProfitAndLossFee(ProfitAndLossFee profitAndLossFee);
    }
}
