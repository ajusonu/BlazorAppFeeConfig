using HouseOfTravel.AutoMapper;

namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Access Model to manage Access for Branch/Outlet based on Access Key
    /// </summary>
    public class Access
    {
        /// <summary>
        /// Unique entry id
        /// </summary>
        [AutoMapperColumn]
        public long Id { get; set; }
        /// <summary>
        /// AcessKey Secrect key for OutletCode
        /// </summary>
        [AutoMapperColumn]
        public string AccessKey { get; set; }
        /// <summary>
        /// outlet
        /// </summary>
        [AutoMapperColumn]
        public string OutletCode { get; set; }
        /// <summary>
        /// Branch
        /// </summary>
        [AutoMapperColumn]
        public string BranchCode { get; set; }
        /// <summary>
        /// Email address to send the link
        /// </summary>
        [AutoMapperColumn]
        public string Email { get; set; }
    }
}