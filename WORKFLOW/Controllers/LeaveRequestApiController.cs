/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WORKFLOW.Models;

namespace WORKFLOW.Controllers
{
    public class LeaveRequestApiController : Controller
    {
        // GET: /api/leaverequests/{user_id}
        public ActionResult GetLeaveRequests(int user_id, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Create an instance of your ADO.NET context
            var context = new WorkFlowEntities2();

            // Fetch leave requests based on user_id, status, fromDate, toDate, and return as JSON
            var leaveRequests = context.LeaveRequests.Where(lr => lr.UserId == user_id);

            if (status != null)
            {
                leaveRequests = leaveRequests.Where(lr => lr.Status == status);
            }

            if (fromDate != null)
            {
                leaveRequests = leaveRequests.Where(lr => lr.FromDate >= fromDate);
            }

            if (toDate != null)
            {
                leaveRequests = leaveRequests.Where(lr => lr.ToDate <= toDate);
            }

            var leaveRequestsList = leaveRequests.ToList();
            return Json(leaveRequestsList, JsonRequestBehavior.AllowGet);
        }
    }
}
*/