using FeeAutomationLibrary;
using HouseOfTravel.AutoMapper;
using System;

namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Extended Pending Fee so that extra properties can be used in Website Page 
    /// </summary>
    public class PendingFeeExtended : PendingFee
    {
        /// <summary>
        /// Fee AppliedDate
        /// </summary>
        [AutoMapperColumn]
        public DateTime? ActionDate { get; set; }

    }
}