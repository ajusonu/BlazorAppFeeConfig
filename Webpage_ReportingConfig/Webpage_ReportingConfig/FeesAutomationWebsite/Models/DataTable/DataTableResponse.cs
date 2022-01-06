using System.Collections.Generic;
// See: https://datatables.net/forums/discussion/40690/sample-implementation-of-serverside-processing-in-c-mvc-ef-with-paging-sorting-searching

namespace FeesAutomationWebsite.Models.DataTable
{
    // lowercase items as the Json(T) call does not use the [JsonProperty] attributes
    // note also that this means that the T object must also use lower-case properties
    public class DataTableResponse<T>
    {
        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public List<T> data { get; set; }
    }
}