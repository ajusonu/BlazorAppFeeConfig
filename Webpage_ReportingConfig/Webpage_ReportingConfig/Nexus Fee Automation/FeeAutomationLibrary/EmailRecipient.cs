using HouseOfTravel.AutoMapper;

namespace FeeAutomationLibrary
{
    /// <summary>
    /// A recipient that needs to get an email regarding pending fees
    /// </summary>
    public class EmailRecipient
    {
        /// <summary>
        /// The outletcode for the email
        /// </summary>
        [AutoMapperColumn]
        public string OutletCode { get; set; }

        /// <summary>
        /// The branch code for the email
        /// </summary>
        [AutoMapperColumn]
        public string BranchCode { get; set; }

        /// <summary>
        /// The access key that will be used in the email
        /// </summary>
        [AutoMapperColumn]
        public string AccessKey { get; set; }

        /// <summary>
        /// the email that will be used
        /// </summary>
        [AutoMapperColumn]
        public string Email { get; set; }

        /// <summary>
        /// Count of New Fee items which is not processed yet 
        /// </summary>
        [AutoMapperColumn]
        public int UnProcessedFeeCount { get; set; }


    }
}
