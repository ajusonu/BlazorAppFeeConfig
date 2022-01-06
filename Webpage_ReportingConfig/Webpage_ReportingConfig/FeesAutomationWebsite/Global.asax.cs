using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FeesAutomationWebsite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        // the logger
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // app startup
        protected void Application_Start()
        {
            _log.Debug("FeeAutomation website Startup.");   // This will ensure that log4net loads our appenders!
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Gets a keyvault secret from the configured keyvault
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public static async Task<string> GetKeyVaultSecret(string secretName)
        {
            AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient kvClient = new KeyVaultClient((a, r, s) => tokenProvider.KeyVaultTokenCallback(a, r, s)); ;
            string url = $"https://{ConfigurationManager.AppSettings["KeyVaultName"]}.vault.azure.net/secrets/{secretName}";
            SecretBundle secret = await kvClient.GetSecretAsync(url);
            return secret?.Value;
        }
    }
}
