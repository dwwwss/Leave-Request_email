using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WORKFLOW.Models
{
    public class Loginmodel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subject { get; internal set; }
        public string Message { get; internal set; }
    }
}