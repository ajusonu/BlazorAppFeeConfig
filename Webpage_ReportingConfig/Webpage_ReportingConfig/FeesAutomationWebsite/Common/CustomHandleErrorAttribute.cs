using log4net;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Common
{
    /// <summary>
    /// Log Unhandled Exceptions Errors
    /// </summary>
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Custom Impliment OnException to handle UnauthorizedAccessException exception and show UnAuthoriseError View
        /// else call the base OnException 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            System.Exception exception = filterContext.Exception;
            _log.Error($"Unhandled Exception in FeeAutomation in  {exception.ToString()} ");
            switch (exception.GetType().ToString())
            {
                case "UnauthorizedAccessException":
                    filterContext.ExceptionHandled = true;
                    HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, "Error", "UnAuthoriseError");
                    filterContext.Result = new ViewResult()
                    {
                        ViewName = "UnAuthoriseError",
                        ViewData = new ViewDataDictionary(model)
                    };

                    break;
                default:
                    base.OnException(filterContext);
                    break;
            }
        }
    }
}