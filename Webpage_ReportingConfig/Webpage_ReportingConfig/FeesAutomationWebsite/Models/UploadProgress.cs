using HouseOfTravel.AutoMapper;
using System.Collections.Generic;

namespace FeesAutomationWebsite.Models
{
    public class UploadProgress<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UploadProgress(string fileName, List<T> rows)
        {
            Filename = fileName;
            Rows = rows;
        }

        /// <summary>
        /// The filename of the file that was uploaded
        /// </summary>
        [AutoMapperColumn]
        public string Filename { get; set; }

        /// <summary>
        /// the number of rows
        /// </summary>
        [AutoMapperColumn]
        public int? Count { get; set; }

        /// <summary>
        /// the number of records that succeeded
        /// </summary>
        [AutoMapperColumn]
        public int? Succeeded { get; set; }

        /// <summary>
        /// the number of records that failed
        /// </summary>
        [AutoMapperColumn]
        public int? Failed { get; set; }

        /// <summary>
        /// the status of the upload
        /// </summary>
        [AutoMapperColumn]
        public string Error { get; set; }

        /// <summary>
        /// the rows that have been updated since the last request
        /// </summary>
        public List<T> Rows;
    }
}