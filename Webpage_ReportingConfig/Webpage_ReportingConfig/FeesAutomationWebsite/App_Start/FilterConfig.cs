using FeesAutomationWebsite.Common;
using System.Web.Mvc;

namespace FeesAutomationWebsite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}
