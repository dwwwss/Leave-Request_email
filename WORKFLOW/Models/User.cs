using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WORKFLOW.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static implicit operator User(WORKFLOW.User v)
        {
            throw new NotImplementedException();
        }
        // Additional properties
    }
}