using log4net;
using System.Web;
using System.Web.Mvc;

namespace FeesAutomationWebsite.Common
{
    /// <summary>
    /// Checks for the presence of an access key from either the query string, or the session
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        // the logger
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Implementation of the base Authorisation
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string accessKey = httpContext.Request.QueryString["AccessKey"];
            if (string.IsNullOrEmpty(accessKey))
            {
                // make sure that we dont already have the access key in the session
                accessKey = (string)HttpContext.Current.Session["AccessKey"];
                if (string.IsNullOrEmpty(accessKey))
                {
                    _log.Error("Fee Automation - Inside method CustomAuthorizeAttribute - Invalid AccessKey or Session Expired");
                    return false;
                }
            }
            HttpContext.Current.Session.Add("AccessKey", accessKey);
            return true;
        }
    }
}