using HouseOfTravel.AutoMapper;

namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Category of Fee to further classified the fee
    /// </summary>
    public class FeeCategory
    {
        /// <summary>
        /// Category - eg LandOnlyFee
        /// </summary>
        [AutoMapperColumn]
        public string Category { get; set; }
        /// <summary>
        /// Description - Land Only Booking, Room Nights Amended etc
        /// </summary>
        [AutoMapperColumn]
        public string Description { get; set; }
    }
}