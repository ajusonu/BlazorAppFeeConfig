namespace FeesAutomationWebsite.Models
{
    /// <summary>
    /// Custom data added to the data table post model so we can filter on the Pending Fee
    /// </summary>
    public class PendingFeeFilterDataTableAjaxPostModel : DataTable.DataTableAjaxPostModel
    {
        /// <summary>
        /// OutletCode To Filter on Pending Fees
        /// </summary>
        public string OutletCode { get; set; }
        /// <summary>
        /// Pending Fee Status to Filter
        /// </summary>
        public string FeeStatus { get; set; }
        /// <summary>
        /// Search Text
        /// </summary>
        public string SelectedText { get; set; }
        /// <summary>
        /// Fee Category
        /// </summary>
        public string Category { get; set; }
    }
}