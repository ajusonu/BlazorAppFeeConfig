using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.Data;
using Webpage_ReportingConfig.DataStore;

namespace Webpage_ReportingConfig.Service
{
    public class ProfitAndLossService : IProfitAndLossFeeService
    {
        static SQLEdwServerStore SQLEdwServerStore;
        public ProfitAndLossService(SQLEdwServerStore store)
        {
            SQLEdwServerStore = store;
        }
        /// <summary>
        /// Get Fee
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProfitAndLossFee> GetProfitAndLossFee(int id = 0)
        {
            ProfitAndLossFeeStore feeStore = new ProfitAndLossFeeStore(SQLEdwServerStore);
            return (await feeStore.GetProfitAndLossFees(id)).FirstOrDefault();

        }

        /// <summary>
        /// Get list of Profit and Loss configs
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProfitAndLossFee>> GetProfitAndLossFees(string SearchText= "")
        {
            List<ProfitAndLossFee> fees = new List<ProfitAndLossFee>();
            ProfitAndLossFeeStore feeStore = new ProfitAndLossFeeStore(SQLEdwServerStore);
            fees = await feeStore.GetProfitAndLossFees(0, SearchText);
            return await Task.FromResult(fees);
        }
        /// <summary>
        /// Save
        /// </summary>
        /// <param name="profitAndLossFee"></param>
        /// <returns></returns>
        public async Task<bool> SaveProfitAndLossFee(ProfitAndLossFee profitAndLossFee)
        {
            ProfitAndLossFeeStore feeStore = new ProfitAndLossFeeStore(SQLEdwServerStore);
            return await feeStore.ProfitAndLossFee_Save(profitAndLossFee);
            

        }
        /// <summary>
        /// Delete Selected Fee row 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteSelectedProfitAndLossFee(List<int> ids)
        {
            ProfitAndLossFeeStore feeStore = new ProfitAndLossFeeStore(SQLEdwServerStore);
            return await feeStore.ProfitAndLossFee_Delete(ids);
        }
    }
}
