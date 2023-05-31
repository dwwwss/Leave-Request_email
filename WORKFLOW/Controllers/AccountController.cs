using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WORKFLOW.Models;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace WORKFLOW.Controllers
{
    public class AccountController : Controller
    {
        private readonly WorkFlowEntities2 dbContext;

        public AccountController()
        {
            dbContext = new WorkFlowEntities2();
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var user = Session["User"] as User;
            if (user != null)
            {
                // Retrieve leave requests for the user
                var leaveRequests = dbContext.LeaveRequests.Where(r => r.fk_user_id == user.user_id).ToList();

                ViewBag.User = user;
                ViewBag.LeaveRequests = leaveRequests;

                return View(user);
            }
            else
            {
                // User is not logged in, redirect to the Login action
                return RedirectToAction("Login");
            }


        }

        public new ActionResult Request()
        {
            return View();
        }
        // Assuming you have a database context called 'DbContext' and a 'LeaveRequest' table/entity
        public ActionResult Admin()
        {
            // Check if the user is logged in as an admin
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
            {
                // Render the admin page
                using (var dbContext = new WorkFlowEntities2())
                {
                    var leaveRequests = dbContext.LeaveRequests.ToList();
                    var model = new List<Tuple<WORKFLOW.User, WORKFLOW.LeaveRequest>>();

                    foreach (var request in leaveRequests)
                    {
                        // Assuming you have a user object associated with each leave request
                        var user = dbContext.Users.FirstOrDefault(u => u.user_id == request.fk_user_id);
                        var tuple = new Tuple<WORKFLOW.User, WORKFLOW.LeaveRequest>(user, request);
                        model.Add(tuple);
                    }

                    return View(model);
                }
            }

            // If the user is not logged in as an admin, redirect to the login page
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult UpdateStatus(int leaveRequestId, string status)
        {
            using (var dbContext = new WorkFlowEntities2())
            {
                var leaveRequest = dbContext.LeaveRequests.FirstOrDefault(r => r.leave_request_Id == leaveRequestId);

                if (leaveRequest != null)
                {
                    // Update the leave request status
                    leaveRequest.leave_status = status;
                    dbContext.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Leave request not found." });
                }
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Loginmodel model)
        {
            // Check if the entered credentials match the admin credentials
            if (model.Username == "admin" && model.Password == "12345")
            {
                // Successful login as admin
                // Store the user role in Session for later use
                Session["UserRole"] = "Admin";

                // Redirect to the admin page
                return RedirectToAction("Admin", "Account");
            }
            else
            {
                // Perform authentication logic using the DbContext for normal users
                var user = dbContext.Users.FirstOrDefault(u => u.email == model.Username && u.password == model.Password);

                if (user != null)
                {
                    // Successful login as a normal user
                    // Store the user in Session for later use
                    Session["User"] = user;

                    // Redirect to the index page after successful login
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    // Failed login
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }

            return View(model);
        }



        private List<LeaveRequest> GetLeaveRequestsFromAPI()
        {
            try
            {
                var apiUrl = "<https://localhost:44398/api/LeaveRequest>"; // Replace with the actual API endpoint URL
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(apiUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var leaveRequestsJson = response.Content.ReadAsStringAsync().Result;
                        var leaveRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LeaveRequest>>(leaveRequestsJson);
                        return leaveRequests;
                    }
                    else
                    {
                        // Handle the API error response
                        // You can log the error or handle it as per your requirements
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API request
                // You can log the exception or handle it as per your requirements
            }

            return new List<LeaveRequest>(); // Return an empty list if there was an error or no leave requests were retrieved
        }


        [HttpPost]
        public async Task<ActionResult> SendEmail(EmailModel model, HttpPostedFileBase attachment)
        {
            if (!ModelState.IsValid)
            {
                // If model validation fails, return the view with validation errors
                return View(model);
            }

            try
            {
                // Retrieve the logged-in user
                var loggedInUser = Session["User"] as User;

                // Send email using the user and other relevant data
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(loggedInUser?.email);
                mailMessage.To.Add(model.EmailTo);
                mailMessage.Subject = model.Subject;
                mailMessage.Body = model.Message;

                // Add attachment if provided
                if (attachment != null && attachment.ContentLength > 0)
                {
                    // Save the file to a temporary location
                    var fileName = Path.GetFileName(attachment.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Attachments"), fileName);
                    attachment.SaveAs(filePath);

                    // Create the attachment
                    Attachment emailAttachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(emailAttachment);
                }

                SmtpClient smtpClient = new SmtpClient("smtpout.secureserver.net", 587); // Use GoDaddy SMTP server and port
                smtpClient.Credentials = new System.Net.NetworkCredential("deepak.patidar@averybit.in", "Deep1221@"); // Replace with your GoDaddy email and password
                smtpClient.EnableSsl = true; // Enable SSL/TLS

                smtpClient.Send(mailMessage);

                // Email sent successfully
                TempData["SuccessMessage"] = "Email sent successfully";

                // Save the data in the LeaveRequest table
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"]?.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string insertQuery = "INSERT INTO LeaveRequest (fk_user_id, start_date, end_date, leave_status, Description) " +
                                         "VALUES (@UserId, @StartDate, @EndDate, @LeaveStatus, @Description)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Retrieve the fk_user_id for the logged-in user
                        int staticUserId = loggedInUser?.user_id ?? 0; // Replace 'user_id' with the actual property name

                        command.Parameters.AddWithValue("@UserId", staticUserId);
                        command.Parameters.AddWithValue("@StartDate", model.FromDate);
                        command.Parameters.AddWithValue("@EndDate", model.ToDate);
                        command.Parameters.AddWithValue("@LeaveStatus", "Pending");
                        command.Parameters.AddWithValue("@Description", model.Subject);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                // Handle email sending error
                ModelState.AddModelError("", "Failed to send email: " + ex.Message);
            }

            // Redirect to Account/Index in case of error
            return RedirectToAction("Index", "Account");
        }







    }
}
