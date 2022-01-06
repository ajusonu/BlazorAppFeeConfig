using Newtonsoft.Json;
// See: https://datatables.net/forums/discussion/40690/sample-implementation-of-serverside-processing-in-c-mvc-ef-with-paging-sorting-searching

namespace FeesAutomationWebsite.Models.DataTable
{
    public class DataTableColumn
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("orderable")]
        public bool Orderable { get; set; }

        [JsonProperty("search")]
        public DataTableSearch Search { get; set; }
    }
}