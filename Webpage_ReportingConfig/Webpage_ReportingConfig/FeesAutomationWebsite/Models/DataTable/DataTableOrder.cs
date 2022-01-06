using Newtonsoft.Json;
// See: https://datatables.net/forums/discussion/40690/sample-implementation-of-serverside-processing-in-c-mvc-ef-with-paging-sorting-searching

namespace FeesAutomationWebsite.Models.DataTable
{
    public class DataTableOrder
    {
        [JsonProperty("column")]
        public int Column { get; set; }

        [JsonProperty("dir")]
        public string Dir { get; set; }
    }
}