using HouseOfTravel.AutoMapper;

namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FeeType
    {
        /// <summary>
        /// Unique entry id
        /// </summary>
        [AutoMapperColumn]
        public int Id { get; set; }
        /// <summary>
        /// Fee Type - eg OLOF matching Product Code
        /// </summary>
        [AutoMapperColumn]
        public string Code { get; set; }
        /// <summary>
        /// Description - Online Domestic Fee, Online Hotel Fee
        /// </summary>
        [AutoMapperColumn]
        public string Description { get; set; }
    }
}