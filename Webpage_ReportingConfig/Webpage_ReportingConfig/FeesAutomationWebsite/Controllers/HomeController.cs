using FeesAutomationWebsite.Common;
using System.Web.Mvc;

namespace HOT.Mobile.Api.Controllers
{
    /// <summary>
    /// Default controller
    /// </summary>
    [CustomAuthorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Default controller. just redirects the Pending Fee controller
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("Index", "PendingFee");
        }
    }
}
