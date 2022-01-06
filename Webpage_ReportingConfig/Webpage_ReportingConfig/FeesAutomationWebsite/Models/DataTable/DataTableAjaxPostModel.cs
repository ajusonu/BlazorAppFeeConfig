using Newtonsoft.Json;
using System.Collections.Generic;
// See: https://datatables.net/forums/discussion/40690/sample-implementation-of-serverside-processing-in-c-mvc-ef-with-paging-sorting-searching

namespace FeesAutomationWebsite.Models.DataTable
{
    /// <summary>
    /// Model that represents the DataTables Ajax Posting for searching/sorting/pagination
    /// </summary>
    public class DataTableAjaxPostModel
    {
        [JsonProperty("draw")]
        public int Draw { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("columns")]
        public List<DataTableColumn> Columns { get; set; }

        [JsonProperty("search")]
        public DataTableSearch Search { get; set; }

        [JsonProperty("order")]
        public List<DataTableOrder> Order { get; set; }
    }
}