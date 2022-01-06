using HouseOfTravel.DataSources.BusinessLogic.Dolphin;
using HouseOfTravel.DataSources.Models.Itinerary;
using System;

namespace Nexus_FeeAutomation
{
    /// <summary>
    /// Implimenting the Fee Adjustment logic 
    /// </summary>
    public class FeeAutomationAdjustmentLogic : IAdjustmentLogic
    {
        /// <summary>
        /// return true - include Price Adjustment for Fee 
        /// </summary>
        /// <param name="adjustment"></param>
        /// <param name="seatOnly"></param>
        /// <returns></returns>
        public bool DoIncludeAdjustment(PriceAdjustment adjustment, bool seatOnly)
        {
            return true;
        }

        /// <summary>
        /// Retruns adjustment code
        /// </summary>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public string GetCode(PriceAdjustment adjustment)
        {
            return adjustment.Code;
        }

        /// <summary>
        /// Not Implemented - Not required for Fee Automation
        /// </summary>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public string GetDescription(PriceAdjustment adjustment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Not required for Fee Automation
        /// </summary>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public string GetFinancialVendorName(PriceAdjustment adjustment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Not required for Fee Automation
        /// </summary>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public string GetVendorName(PriceAdjustment adjustment)
        {
            throw new NotImplementedException();
        }
    }
}
