using HouseOfTravel.AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FeeAutomationLibrary
{
    /// <summary>
    /// Represents a fee config that defines what type of fee is applicable
    /// </summary>
    public class FeeTypeMapping
    {
        /// <summary>
        /// The Id
        /// </summary>
        [AutoMapperColumn]
        public long Id { get; set; }

        /// <summary>
        /// the outletcode
        /// </summary>
        [Required]
        [AutoMapperColumn(FieldName = "OutletCode")]
        [Display(Name = "Outlet")]
        public string OutletCode { get; set; }

        /// <summary>
        /// The BranchCode 
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; }

        /// <summary>
        /// The Fee Type that we will use to find the applicable fee
        /// </summary>
        [AutoMapperColumn]
        [Required]
        [Display(Name = "Fee Type")]
        public string FeeType { get; set; }

        /// <summary>
        /// The description of the Fee
        /// </summary>
        [AutoMapperColumn]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// The type of Fee (offline, after hours, etc)
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Booking Type")]
        public string BookingType { get; set; }

        /// <summary>
        /// The type of Fee (offline, after hours, etc)
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Query Type")]
        public int QueryType { get; set; }

        // <summary>
        /// Query Scope is Fee charge Scope is it Per Folder or Per Segment etc
        /// </summary>
        [AutoMapperColumn]
        public string Scope { get; set; }

        // <summary>
        /// Exclusion Code is Fee Exclusion Code for Managment Fee matching Preference
        /// </summary>
        [Display(Name = "Exclusion Code")]
        [AutoMapperColumn]
        public string ExclusionCode { get; set; }
        ///AutoApply
         // <summary>
        /// Auto Add Fee
        /// </summary>
        [Display(Name = "Auto Apply")]
        [AutoMapperColumn]
        public bool AutoApply { get; set; }
        /// <summary>
        /// Used in UI - Error while creating or saving
        /// </summary>
        [Display(Name = "Error Description")]
        public string ErrorDescription { get; set; }
        /// <summary>
        /// Scope List
        /// </summary>
        public List<string> ScopeList = new List<string>();
        /// <summary>
        /// Query List
        /// </summary>
        public List<string> QueryList = new List<string>();
    }
}
