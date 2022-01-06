//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace FeesAutomationWebsite.Models
//{
//    public class PendingFee
//    {
//        /// <summary>
//        /// used to enable user selection for Check Box based on Status
//        /// </summary>
//        public bool EnableRowSelection { get; set; }
//        /// <summary>
//        /// Parent OutletCode Code
//        /// </summary>
//        public string OutletCode { get; set; }
//        /// <summary>
//        /// Unique Row id
//        /// </summary>
//        public long Id { get; set; }
//        /// <summary>
//        /// Datetime when added to Pending Fee Dolphin table
//        /// </summary>
//        public System.DateTime DateStamp { get; set; }
//        public string DPECode { get; set; }
//        public string OfficeId { get; set; }
//        /// <summary>
//        /// Branch
//        /// </summary>
//        public string BranchCode { get; set; }
//        /// <summary>
//        /// Booking Id
//        /// </summary>
//        public long FolderNumber { get; set; }
//        /// <summary>
//        /// Item Id -1 mean not applicable
//        /// </summary>
//        public int ItemId { get; set; }

//        /// <summary>
//        /// Item Type HOT for Hotel, 
//        /// </summary>
//        public string ItemType { get; set; }
//        /// <summary>
//        /// Type of fee applied Amendmend fee or Land only Fee
//        /// </summary>

//        public string FeeType { get; set; }
//        /// <summary>
//        /// 
//        /// </summary>
//        public string Status { get; set; }
//        public string Description { get; set; }
//        public Nullable<long> CompanyId { get; set; }
//        public string CompanyName { get; set; }
//        public Nullable<System.DateTime> ItemCreationDate { get; set; }
//        public Nullable<System.DateTime> FolderCreation { get; set; }
//        public string FolderOwner { get; set; }
//        public string BookingType { get; set; }
//        public string FolderStatus { get; set; }
//        public decimal FeeValue { get; set; }


//    }
//}