using HouseOfTravel.DataSources.BusinessLogic.Dolphin;
using HouseOfTravel.DataSources.Models.Itinerary;

namespace Nexus_FeeAutomation.BusinessLogic
{
    /// <summary>
    /// Implimenting standard fee Automation Adjustment Price Business logic
    /// </summary>
    public class FeeAutomationAdjustmentPriceLogic : IAdjustmentPriceLogic
    {
        /// <summary>
        /// retrun price currency code for fee 
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetCurrencyCode(Cost price)
        {
            return price.Currency;
        }

        /// <summary>
        /// returning the price description for fee
        /// </summary>
        /// <param name="adjustment"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetDescription(PriceAdjustment adjustment, Cost price)
        {
            return price.Description;
        }

        /// <summary>
        /// Get Net Price - should be 0 for Fee to Auto Apply Nett Amount
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public decimal GetNettAmount(Cost price)
        {
            return 0.0M;
        }

        /// <summary>
        /// Get the cost price total amount
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public decimal GetTotalAmount(Cost price)
        {
            return price.Value;
        }
    }
}
