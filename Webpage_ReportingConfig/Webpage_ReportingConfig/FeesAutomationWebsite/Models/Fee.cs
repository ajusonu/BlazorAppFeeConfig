using HouseOfTravel.AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Fee Model to Setup Fee Configuration for Branch/Outlet/Pricing/CompanyId
    /// </summary>
    public class Fee : IValidatableObject
    {
        /// <summary>
        /// Unique entry id
        /// </summary>
        [AutoMapperColumn]
        public long Id { get; set; }

        /// <summary>
        /// outlet
        /// </summary>
        [Required]
        [Display(Name = "Outlet")]
        [AutoMapperColumn]
        public string OutletCode { get; set; }

        /// <summary>
        /// Branch
        /// </summary>
        [Display(Name = "Branch")]
        [AutoMapperColumn]
        public string BranchCode { get; set; }

        /// <summary>
        /// Pricing Profile Code - Represent group of company to apply fee
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Profile")]
        [MaxLength(5, ErrorMessage = "Profile Code cannot be longer than 5 characters.")]
        [RegularExpression("([A-Za-z]){2}[A-Za-z0-9]+", ErrorMessage = "Invalid Pricing Profile Code - Please Enter starting with two letters. eg VO1H1")]
        public string PricingProfile { get; set; }

        /// <summary>
        /// Company Id - Individual Company to apply fee
        /// </summary>
        [Display(Name = "Company")]
        [AutoMapperColumn]
        public long CompanyId { get; set; }

        /// <summary>
        /// FeeType- Type of Fee to apply
        /// </summary>
        [Required]
        [Display(Name = "Type")]
        [AutoMapperColumn]
        public string FeeType { get; set; }

        /// <summary>
        /// Fee Type Description
        /// </summary>
        [AutoMapperColumn]
        public string Description { get; set; }

        /// <summary>
        /// FeePerPNR- Amount of fee per PNR to apply
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Per Folder")]
        [Range(typeof(decimal), "00.0", "500.49")]
        public decimal FeePerFolder { get; set; }

        /// <summary>
        /// FeePerPNR- Amount of fee per PNR to apply
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Per PNR")]
        [Range(typeof(decimal), "00.0", "500.49")]
        public decimal FeePerPNR { get; set; }

        /// <summary>
        /// FeePerSegment- Amount of fee per Segment to apply
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Per Segment")]
        [Range(typeof(decimal), "00.00", "500.49")]
        public decimal FeePerSegment { get; set; }

        /// <summary>
        /// FeePerSegment- Amount of fee per Segment to apply
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Per Duration")]
        [Range(typeof(decimal), "00.00", "100.49")]
        public decimal FeePerDuration { get; set; }


        /// <summary>
        /// Is Active - Fee Deleted or not
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
        /// <summary>
        /// AutoApply - Used for Full Automation
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Auto Apply")]
        public bool AutoApply { get; set; }
        /// <summary>
        /// ActionPerformed - When Importing fee -- Inserted or Updated
        /// </summary>
        [AutoMapperColumn]
        [Display(Name = "Action Performed")]
        public string ActionPerformed { get; set; }

        /// <summary>
        /// Number Of Fee Values - Valid is only one should be Entered
        /// </summary>
        public int NumberOfFeeValues
        {
            get
            {
                return
                  (FeePerSegment != 0 ? 1 : 0) + (FeePerPNR != 0 ? 1 : 0) + (FeePerFolder != 0 ? 1 : 0) + (FeePerDuration != 0 ? 1 : 0);
            }
        }
        /// <summary>
        /// Used in UI - Fee Type List to show in Dropdown
        /// </summary>
        public List<FeeType> FeeTypes = new List<FeeType>();

        /// <summary>
        /// Used in UI - Error while creating or saving
        /// </summary>
        [Display(Name = "Error Description")]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Used in UI - To check if it is create new record mode or Editing existing record mode
        /// </summary>
        public bool EditMode { get; set; }
        /// <summary>
        /// Used in UI - To check Edit Auto Apply is Allowed or not
        /// </summary>
        public bool EditAutoApply { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NumberOfFeeValues != 1)
            {
                yield return new ValidationResult(NumberOfFeeValues == 0 ? "Please enter one fee value." : "Please enter only one fee value.");
            }
        }
        public Fee()
        {
            EditAutoApply = false;
        }
    }
}


