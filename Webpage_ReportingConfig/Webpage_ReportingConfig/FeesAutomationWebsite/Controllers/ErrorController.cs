using System.Web.Mvc;

namespace FeesAutomationWebsite.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UnAuthoriseError()
        {
            return View();
        }
    }
}