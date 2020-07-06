using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using UserLoginSQLCommand.Models.Common;
using System.Net;
using System.Net.Mail;
using UserLoginSQLCommand.Models.DBContext;

namespace UserLoginSQLCommand.Controllers
{
    public class LoginController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        SqlConnection connection;
        SqlCommand command;
        SqlDataReader reader;
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View("Login");
        }

        //[HttpPost]
        public JsonResult Login(string EmailId, string Password)
        {
            AjaxResult ajaxResult = new AjaxResult();
            IList<string> errMesgs = new List<string>();
            Int32 count;
            var _passWord = UserLoginSQLCommand.Models.encryptPassword.textToEncrypt(Password);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM [dbo].[UserM] Where 1=1" +
                               "AND [Email] = '" + EmailId + "' AND [Password]='" + _passWord + "' AND [EmailVerification]=1";
                SqlCommand command = new SqlCommand(query, connection);
                count = (Int32)command.ExecuteScalar();
                //count = 
                connection.Close();
            }
           
            UserLoginSQLCommand.Models.UserLogin a = new Models.UserLogin();
            if (count > 0)
            {
                int timeout =  a.Rememberme? 60 : 5; // Timeout in minutes, 60 = 1 hour.  
                var ticket = new FormsAuthenticationTicket(EmailId, false, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = System.DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                //return View("Index", "_LayoutAfterLogin");
                //return RedirectToAction("Index", "Member");
                
                //return Redirect("Member/Index");
                //return PartialView("~/Views/Shared/_LayoutAfterLogin.cshtml");

                ajaxResult.Result = AjaxResult.VALUE_SUCCESS;
                ajaxResult.RedirectURL = "Member";
            }
            else {
                //ViewBag.Message = "Email yang kamu gunakan tidak ditemukan atau belum verfikasi email";
                ajaxResult.Result = ajaxResult.ValueError;
                ajaxResult.ErrMesgs = new string[] { string.Format("{0} : {1}", "Warning", "Email yang kamu gunakan tidak ditemukan atau belum verfikasi email") };
                if (errMesgs.Count > 0)
                {
                    ajaxResult.Result = AjaxResult.VALUE_ERROR;
                    ajaxResult.ErrMesgs = errMesgs.ToArray();
                }
            }
            return Json(ajaxResult);
        }

        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        public JsonResult ForgetPassword(UserM data)
        {
            AjaxResult ajaxResult = new AjaxResult();
            IList<string> errMesgs = new List<string>();
            Int32 count;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM [dbo].[UserM] Where 1=1" +
                               "AND [Email] like '%" + data.Email + "'";
                SqlCommand command = new SqlCommand(query, connection);
                count = (Int32)command.ExecuteScalar();
                //count = 
                connection.Close();
            }

            if (count == 0)
            {
                ajaxResult.Result = ajaxResult.ValueError;
                ajaxResult.ErrMesgs = new string[] { string.Format("{0} = {1}", "Warning", "File extension should be .xls or .xlsx") };
                if (errMesgs.Count > 0)
                {
                    ajaxResult.Result = AjaxResult.VALUE_ERROR;
                    ajaxResult.ErrMesgs = errMesgs.ToArray();
                }
                //"EMAILNOTEXIST"
            }
            else {
                // Genrate OTP   
                string OTP = GeneratePassword();

                data.ActivetionCode = Guid.NewGuid();
                data.OTP = OTP;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //CARA 1
                    //string query = "UPDATE [dbo].[UserM] SET [OTP] = '" + data.OTP + "' WHERE [Email]='" + data.Email + "'";
                    //
                    //SqlCommand command = new SqlCommand(query, connection);
                    //try
                    //{
                    //    command.ExecuteNonQuery();
                    //}
                    //catch (SqlException ex)
                    //{
                    //
                    //}

                    //CARA 2
                    using (SqlCommand cmd = connection.CreateCommand()) {
                        cmd.CommandText = "UPDATE [dbo].[UserM] SET [OTP] = @otp WHERE [Email]=@email";
                        cmd.Parameters.AddWithValue("@otp", data.OTP);
                        cmd.Parameters.AddWithValue("@email", data.Email);
                        cmd.ExecuteNonQuery();
                    } 
                    connection.Close();
                }
                ForgetPasswordEmailToUser(data.Email, data.ActivetionCode.ToString(), data.OTP);

                ajaxResult.Result = AjaxResult.VALUE_SUCCESS;
                //ajaxResult.RedirectURL = "lokasi...";
            }

            return Json(ajaxResult);
        }

        public string GeneratePassword()
        {
            string OTPLength = "4";
            string OTP = string.Empty;

            string Chars = string.Empty;
            Chars = "1,2,3,4,5,6,7,8,9,0";

            char[] seplitChar = { ',' };
            string[] arr = Chars.Split(seplitChar);
            string NewOTP = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < Convert.ToInt32(OTPLength); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                NewOTP += temp;
                OTP = NewOTP;
            }
            return OTP;
        }

        public void ForgetPasswordEmailToUser(string emailId, string activationCode, string OTP)
        {
            var GenarateUserVerificationLink = "/Login/ChangePasswordView/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenarateUserVerificationLink);

            var fromMail = new MailAddress("testemail@gmail.com", "Agung Kasep"); // set your email  
            var fromEmailpassword = "yourpassword email"; // Set your password   
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Password Reset-Demo";
            Message.Body = "<br/> Your registration completed succesfully." +
                           "<br/> please click on the below link for account verification" +
                           "<br/><br/><a href=" + link + ">" + link + "</a>" +
               "<br/>OTP for password change: " + OTP;
            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        public ActionResult ChangePasswordView()
        {
            return View("ChangePassword");
        }


        #region Change Password from Email Account.
        public JsonResult ChangePassword(UserM data)
        {
            AjaxResult ajaxResult = new AjaxResult();
            IList<string> errMesgs = new List<string>();
            Int32 count;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM [dbo].[UserM] Where 1=1" +
                               "AND [Email]='" + data.Email + "' AND [OTP] = '" + data.OTP + "'";
                SqlCommand command = new SqlCommand(query, connection);
                count = (Int32)command.ExecuteScalar();
                //count = 
                connection.Close();
            }

            if (count != 0)
            {
                  var passwordbeforeencryp = data.Password;
                data.Password = UserLoginSQLCommand.Models.encryptPassword.textToEncrypt(data.Password);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE [dbo].[UserM] SET [Password]=@Password WHERE [Email]=@Email AND [OTP] = @OTP";
                        cmd.Parameters.AddWithValue("@Email", data.Email);
                        cmd.Parameters.AddWithValue("@OTP", data.OTP);
                        cmd.Parameters.AddWithValue("@Password", data.Password);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                SendChangePasswordToUser(data.Email, passwordbeforeencryp);
                ajaxResult.Result = AjaxResult.VALUE_SUCCESS;
                ajaxResult.RedirectURL = "Login";
            }
            else
            {
                ajaxResult.Result = ajaxResult.ValueError;
                ajaxResult.ErrMesgs = new string[] { string.Format("{0} : {1}", "Warning", "Email Yang Digunakan Salah") };
                if (errMesgs.Count > 0)
                {
                    ajaxResult.Result = AjaxResult.VALUE_ERROR;
                    ajaxResult.ErrMesgs = errMesgs.ToArray();
                }
            }

            return Json(ajaxResult);
        }
        #endregion


        public void SendChangePasswordToUser(string emailId, string password)
        {
            
            var GenarateUserVerificationLink = "/Login/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenarateUserVerificationLink);

            var fromMail = new MailAddress("testemail@gmail.com", "Agung Kasep"); // set your email  
            var fromEmailpassword = "yourpassword email"; // Set your password   
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Change Password Completed-Demo";
            Message.Body = "<br/> Change Password completed succesfully." +
                           "<br/> please click on the below link for login" +
                           "<br/><br/><a href=" + link + ">" + link + "</a>" +
                           "<br/> Your New Password: " + password;
            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        public JsonResult Register(UserM data)
        {
            AjaxResult ajaxResult = new AjaxResult();
            IList<string> errMesgs = new List<string>();
            Int32 count;
            // email not verified on registration time  
            data.EmailVerification = false;
            //it generate unique code     
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM [dbo].[UserM] Where 1=1" +
                               "AND [Email] = '" + data.Email + "'";
                SqlCommand command = new SqlCommand(query, connection);
                count = (Int32)command.ExecuteScalar();
                //count = 
                connection.Close();
            }

            if (count == 0)
            {
                data.ActivetionCode = Guid.NewGuid();
                data.Password = UserLoginSQLCommand.Models.encryptPassword.textToEncrypt(data.Password);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [dbo].[UserM]([FirstName],[LastName],[Email],[Password],[EmailVerification],[ActivetionCode]) VALUES(@FirstName,@LastName,@Email,@Password,@EmailVerification,@ActivetionCode) ";
                        cmd.Parameters.AddWithValue("@FirstName", data.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", data.LastName);
                        cmd.Parameters.AddWithValue("@Email", data.Email);
                        cmd.Parameters.AddWithValue("@Password", data.Password);
                        cmd.Parameters.AddWithValue("@EmailVerification", 0);
                        cmd.Parameters.AddWithValue("@ActivetionCode", data.ActivetionCode);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                
                SendEmailVerificationToUser(data.Email, data.ActivetionCode.ToString());

                ajaxResult.Result = AjaxResult.VALUE_SUCCESS;
                //ajaxResult.RedirectURL = "lokasi...";
            }
            else
            {
                ajaxResult.Result = ajaxResult.ValueError;
                ajaxResult.ErrMesgs = new string[] { string.Format("{0} : {1}", "Warning", "Email yang digunakan sudah terdaftar") };
                if (errMesgs.Count > 0)
                {
                    ajaxResult.Result = AjaxResult.VALUE_ERROR;
                    ajaxResult.ErrMesgs = errMesgs.ToArray();
                }
            }

            return Json(ajaxResult);
        }

        public void SendEmailVerificationToUser(string emailId, string activationCode)
        {
            var GenarateUserVerificationLink = "/Login/UserVerification/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenarateUserVerificationLink);

            var fromMail = new MailAddress("testemail@gmail.com", "Agung Kasep"); // set your email  
            var fromEmailpassword = "yourpassword email"; // Set your password   
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Registration Completed-Demo";
            Message.Body = "<br/> Your registration completed succesfully." +
                           "<br/> please click on the below link for account verification" +
                           "<br/><br/><a href=" + link + ">" + link + "</a>";
            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        #region Verification from Email Account.
        public ActionResult UserVerification(string id)
        {
            Int32 count;
          
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM [dbo].[UserM] Where 1=1" +
                               "AND [ActivetionCode] = '" + id + "'";
                SqlCommand command = new SqlCommand(query, connection);
                count = (Int32)command.ExecuteScalar();
                //count = 
                connection.Close();
            }

            if (count == 0)
            {
                ViewBag.Message = "Invalid Request...Email not verify";
                //ViewBag.Status = false;
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE [dbo].[UserM] SET [EmailVerification]=1 WHERE [ActivetionCode] = @ActivetionCode";
                        cmd.Parameters.AddWithValue("@ActivetionCode", id);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                ViewBag.Message = "Email Verification completed";
            }

            return View("UserVerification");
        }
        #endregion
    }
}

// Copyright © 2020 Agung Anggara. All rights reserved.
// Purpose: To search all columns of all tables for a given search string
// Written by: Agung Anggara
// Site: https://www.agungpanduan.com
// Updated and tested by Tim Gaunt
// https://medium.com/@agungpanduan/asp-net-create-login-form-using-sql-command-7a59297edb3e
// Tested on: SQL Server 2012 & 2016, Visual Studio 2010
