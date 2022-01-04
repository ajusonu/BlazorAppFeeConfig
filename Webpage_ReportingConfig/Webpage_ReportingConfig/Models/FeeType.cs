using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Webpage_ReportingConfig.Models
{
    /// <summary>
    /// Fee Types
    /// </summary>
    public enum FeeType
    {
        Percentage=1,
        Value=2
    }
    /// <summary>
    /// Get the Fee Type list for drop down in Create and Update View
    /// </summary>
    /// <returns></returns>

    public class FeeTypeList
    {
        public static List<FeeType> GetFeeTypes()
        {
            List<FeeType> feeTypes = new List<FeeType>();
            foreach (FeeType feeType in Enum.GetValues(typeof(FeeType)))
            {
                feeTypes.Add(feeType);
            }

            return feeTypes;
        }
    }
}
