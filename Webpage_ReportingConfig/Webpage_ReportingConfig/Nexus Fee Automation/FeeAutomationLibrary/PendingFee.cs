using HouseOfTravel.AutoMapper;
using System;

namespace FeeAutomationLibrary
{
    /// <summary>
    /// Represents a fee that needs to potentially be added to a dolphin folder
    /// </summary>
    public class PendingFee
    {
        /// <summary>
        /// The Id from the dolphin database
        /// </summary>
        [AutoMapperColumn]
        public long Id { get; set; }

        /// <summary>
        /// The Id from the DolphinDatabase
        /// </summary>
        [AutoMapperColumn]
        public long DolphinDatabaseId { get; set; }

        /// <summary>
        /// The date and time that this record was created
        /// </summary>
        [AutoMapperColumn]
        public DateTime DateStamp { get; set; }

        /// <summary>
        /// the Pricing Profile (DPE code) for the customer
        /// </summary>
        [AutoMapperColumn]
        public string DPECode { get; set; }

        /// <summary>
        /// the OfficeId for this outlet
        /// </summary>
        [AutoMapperColumn]
        public string OfficeId { get; set; }

        /// <summary>
        /// The BranchCode 
        /// </summary>
        [AutoMapperColumn]
        public string BranchCode { get; set; }

        /// <summary>
        /// The Folder number that this fee needs to be applied to
        /// </summary>
        [AutoMapperColumn(FieldName = "FolderNo")]
        public long FolderNumber { get; set; }

        /// <summary>
        /// The item on the folder that generated the fee
        /// </summary>
        [AutoMapperColumn]
        public int ItemId { get; set; }

        /// <summary>
        /// The type of item that generated the fee
        /// </summary>
        [AutoMapperColumn]
        public string ItemType { get; set; }

        /// <summary>
        /// The Fee Type that we will use to find the applicable fee
        /// </summary>
        [AutoMapperColumn]
        public string FeeType { get; set; }

        /// <summary>
        /// The description of the item
        /// </summary>
        [AutoMapperColumn]
        public string Description { get; set; }

        /// <summary>
        /// The dolphin company id
        /// </summary>
        [AutoMapperColumn]
        public long CompanyId { get; set; }

        /// <summary>
        /// The company name for the folder
        /// </summary>
        [AutoMapperColumn]
        public string CompanyName { get; set; }

        /// <summary>
        /// the outletcode
        /// </summary>
        [AutoMapperColumn(FieldName = "Outlet")]
        public string OutletCode { get; set; }

        /// <summary>
        /// The date and time that the specific item was created
        /// </summary>
        [AutoMapperColumn("ItemCreationDate")]
        public DateTime ItemCreation { get; set; }

        /// <summary>
        /// The date and time that the folder was created
        /// </summary>
        [AutoMapperColumn("FolderCreationDate")]
        public DateTime FolderCreation { get; set; }

        /// <summary>
        /// The name of the folder owner
        /// </summary>
        [AutoMapperColumn]
        public string FolderOwner { get; set; }

        /// <summary>
        /// The type of booking (offline, after hours, etc)
        /// </summary>
        [AutoMapperColumn]
        public string BookingType { get; set; }

        /// <summary>
        /// The folder status
        /// </summary>
        [AutoMapperColumn]
        public string FolderStatus { get; set; }
        /// <summary>
        /// The Fee status - Pending,Synced,New,Invoiced
        /// </summary>
        [AutoMapperColumn]
        public string Status { get; set; }
        /// <summary>
        /// Duration (Number of Nights in case of Hotel, Number of Days in case of car)
        /// </summary>
        [AutoMapperColumn]
        public int Duration { get; set; }
        /// <summary>
        /// value of Fee to be applied
        /// </summary>
        [AutoMapperColumn]
        public decimal FeeValue { get; set; }
        /// <summary>
        /// used to enable user selection for Check Box based on Status Used in FeeAutomation Website selection
        /// </summary>
        [AutoMapperColumn]
        public bool EnableRowSelection { get; set; }
        /// <summary>
        /// Product Code to be used to create Fee item
        /// </summary>
        [AutoMapperColumn]
        public string ProductCode { get; set; }
        /// <summary>
        /// Reason of Cancellation 
        /// </summary>
        [AutoMapperColumn]
        public string CancellationReason { get; set; }
        /// <summary>
        /// Category
        /// </summary>
        [AutoMapperColumn]
        public string Category { get; set; }
        /// <summary>
        /// Scope - It is per folder or per Segment
        /// </summary>
        [AutoMapperColumn]
        public string Scope { get; set; }
        // <summary>
        /// Auto Add Fee
        /// </summary>
        [AutoMapperColumn]
        public bool AutoApply { get; set; }
    }
}
