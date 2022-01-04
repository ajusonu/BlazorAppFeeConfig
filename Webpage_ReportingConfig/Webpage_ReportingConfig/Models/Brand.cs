using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Webpage_ReportingConfig.Data
{
    public enum Brand { MixAndMatchNZ, MixAndMatchAU, MixAndMatchUK };
    /// <summary>
    /// Get the brand list for drop down in Create and Update View
    /// </summary>
    /// <returns></returns>

    public class BrandList
    {
        public static List<Brand> GetBrandList()
        {
            List<Brand> brandList = new List<Brand>();
            foreach (Brand brand in Enum.GetValues(typeof(Brand)))
            {
                brandList.Add(brand);
            }

            return brandList;
        }
    }
}
