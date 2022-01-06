using Newtonsoft.Json;
using System.Web.Mvc;
// See: https://datatables.net/forums/discussion/40690/sample-implementation-of-serverside-processing-in-c-mvc-ef-with-paging-sorting-searching

namespace FeesAutomationWebsite.Models.DataTable
{
    public class DataTableSearch
    {
        [AllowHtml]
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("regex")]
        public string Regex { get; set; }
    }
}