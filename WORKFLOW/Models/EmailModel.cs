using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WORKFLOW.Models
{
    public class EmailModel
    {
        internal object username;

        [Required(ErrorMessage = "Email to is required.")]
        public string EmailTo { get; set; }

       
        public string CC { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime  ToDate { get;  set; }
        public HttpPostedFileBase Attachment { get; set; }
       
    }
}