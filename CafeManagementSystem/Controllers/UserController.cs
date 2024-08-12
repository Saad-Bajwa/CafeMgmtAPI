using CafeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeMgmtSystemEntities db = new CafeMgmtSystemEntities();
        Response response = new Response();

        #region User
        [Route("signup")]
        [HttpPost]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Successfully Registered" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "An error occurred while processing your request." });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }

        [HttpGet, Route("getAllUsers")]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Users.Where(x => x.role != "admin").ToList());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Internal Server Error");
                throw ex;
            }
        }
        [HttpGet, Route("getAllUser")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllUser()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                var result = db.Users.Select(
                        u => new { u.id, u.name, u.contactNumber, u.email, u.status, u.role }
                        ).Where(x => x.role == "user").ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpPost, Route("updateUserStatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateUserStatus(User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User userObj = db.Users.Find(user.id);
                if (userObj == null)
                {
                    response.Message = "User id not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "User Status Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("getUserById/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetUserById(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                User userObj = db.Users.Where(x => x.id == id).FirstOrDefault();
                if (userObj == null)
                {
                    response.Message = "User Not Found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                return Request.CreateResponse(HttpStatusCode.OK, userObj);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Internal DB Error");
                throw ex;
            }
        }

        [HttpPost, Route("updateUser")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateUser([FromBody] User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Only admin can authorize this");
                }
                User userObj = db.Users.Find(user.id);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Not Found");
                }
                userObj.name = user.name;
                userObj.email = user.email;
                userObj.password = user.password;
                userObj.contactNumber = user.contactNumber;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Record Updated Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Internal DB Error");
                throw ex;
            }
        }
        [HttpPost, Route("deleteUser/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage DeleteUser(int id)
        {
            try
            {
                User user = db.Users.Find(id);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "User Not Found");
                }
                db.Users.Remove(user);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Internal DB Error");
                throw ex;
            }
        }
        #endregion


        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email && u.password == user.password).FirstOrDefault();
                if (userObj != null)
                {
                    if (userObj.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { token = TokenManager.GenerateToken(userObj.email, userObj.role) });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Wait for Admin approval" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Incorrect Username or Password" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        

        [HttpGet,Route("checkToken")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage checkToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        

        

        [HttpPost, Route("changePassword")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage ChangePassword(CafeManagementSystem.Models.ChangePassword changePassword) 
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                User userObj = db.Users.Where(x=>x.email == tokenClaim.Email && x.password == changePassword.OldPassword).FirstOrDefault();
                if(userObj == null)
                {
                    response.Message = "Incorrect Old Password";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.password = changePassword.NewPassword;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Password Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        private string createEmailBody(string email, string password)
        {
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/Template/ForgetPassword.html")))
                {
                    body = reader.ReadToEnd();
                };
                body = body.Replace("{email}", email);
                body = body.Replace("{password}", password);
                body = body.Replace("{frontendUrl}", "http://localhost:4200/");
                return body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        [HttpPost,Route("forgetPassword")]
        public async Task<HttpResponseMessage> ForgetPassword([FromBody] User user)
        {
            User userObj = db.Users.Where(u => u.email == user.email).FirstOrDefault();
            response.Message = "Password sent successfully to your email";
            if (userObj == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.email));
            message.Subject = "Password by Cafe Management System";
            message.Body = createEmailBody(userObj.email, userObj.password);
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
                await Task.FromResult(0);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        
    }
}
