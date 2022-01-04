using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Webpage_ReportingConfig.Models;

namespace Webpage_ReportingConfig.Data
{
    public class ProfitAndLossFee
    {
        [Key]
        public int Id { get; set; }
        [Required]

        public int FeeNameId { get; set; }
       /// <summary>
       /// Not required as it used to display on the screen
       /// </summary>
        public string Name { get; set; }
        [Required]
        public Brand Brand { get; set; }
        [Range(0, 100)]
        [DataType(DataType.Currency)]
        public float FeeValue { get; set; }
        [Range(0, 100)]
        public float FeePercentage { get; set; }
        /// <summary>
        /// Fee Type - Percentage or Value
        /// </summary>
        public FeeType Type { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FeeValue != 0 && FeePercentage != 0)
            {
                yield return new ValidationResult("Can not have Fee Percentage and Fee Value both filled");
            }
        }

    }
}
