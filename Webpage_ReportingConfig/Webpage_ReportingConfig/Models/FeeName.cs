using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Webpage_ReportingConfig.Models
{
    /// <summary>
    /// Fee name
    /// </summary>
    public class FeeName
    {
        /// <summary>
        /// Id 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Fee Name
        /// </summary>
        [Required]
        [RegularExpression(@"^[A-Z]+[A-Za-z\s\d_]+", ErrorMessage = "Start with Capital letter with Minimun length of 5 ")]
        [MinLength(5, ErrorMessage = "This is required field with Minimum length of 5")]
        [MaxLength(50)]
        public string Name { get; set; }
        /// <summary>
        /// Type of fee
        /// </summary>
        /// 
        public FeeType Type { get; set; }
    }
}
