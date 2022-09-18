using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace MDT.Models
{
    /// <summary>
    /// This class handles setting up and sending email messages.
    /// </summary>
    public class EmailMessage
    {
        private MailMessage email;


        /// <summary>
        /// Creates a new EmailMessage object. 
        /// </summary>
        public EmailMessage()
        {
            email = new MailMessage()
            {
                BodyEncoding = Encoding.UTF8
            };
            email.From = new MailAddress("web@mydrawingtracker.com", "My Drawing Tracker");

        }

        /// <summary>
        /// Overrides the From field of the email to the specified address instead of the value of EmailFrom in the application settings
        /// </summary>
        /// <param name="senderEmail">email address of sender</param>
        /// <param name="senderName">name of sender, defaults to ACTION</param>
        public void SetFrom(string senderEmail, string senderName = null)
        {
            if (senderEmail != null)
            {
                email.From = new MailAddress(senderEmail, senderName ?? "My Drawing Tracker");
            }
        }

        /// <summary>
        /// Adds recipients in the To field of the email 
        /// </summary>
        /// <param name="recipients">a semi-colon delimited list of recipients</param>
        public void AddTo(string recipients)
        {
            if (recipients != null)
            {
                AddTo(recipients.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        /// <summary>
        /// Adds recipients in the To field of the email 
        /// </summary>
        /// <param name="recipients">List of recipient emails</param>
        public void AddTo(List<string> recipients)
        {
            foreach (string r in recipients)
            {
                string[] s = r.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (email.To.Where(to => to.Address.Equals(s[0].Trim())).Count() == 0)
                {
                    email.To.Add(new MailAddress(s[0].Trim(), s.Length > 1 ? s[1].Trim() : ""));
                }
            }
        }

        /// <summary>
        /// Adds recipients in the CC field of the email 
        /// </summary>
        /// <param name="recipients">a semi-colon delimited list of recipients, optionally each address can include a tab character followed by a display name</param>
        public void AddCC(string recipients)
        {
            if (recipients != null)
            {
                AddCC(recipients.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        /// <summary>
        /// Adds recipients in the CC field of the email 
        /// </summary>
        /// <param name="recipients">List of recipient emails, optionally each address can include a tab character followed by a display name</param>
        public void AddCC(List<string> recipients)
        {
            foreach (string r in recipients)
            {
                string[] s = r.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (email.CC.Where(to => to.Address.Equals(s[0].Trim())).Count() == 0)
                {
                    email.CC.Add(new MailAddress(s[0].Trim(), s.Length > 1 ? s[1].Trim() : ""));
                }
            }
        }

        /// <summary>
        /// Adds recipients in the BCC field of the email 
        /// </summary>
        /// <param name="recipients">a semi-colon delimited list of recipients, optionally each address can include a tab character followed by a display name</param>
        public void AddBCC(string recipients)
        {
            if (recipients != null)
            {
                AddBCC(recipients.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        /// <summary>
        /// Adds recipients in the BCC field of the email 
        /// </summary>
        /// <param name="recipients">List of recipient emails, optionally each address can include a tab character followed by a display name</param>
        public void AddBCC(List<string> recipients)
        {
            foreach (string r in recipients)
            {
                string[] s = r.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (email.Bcc.Where(to => to.Address.Equals(s[0].Trim())).Count() == 0)
                {
                    email.Bcc.Add(new MailAddress(s[0].Trim(), s.Length > 1 ? s[1].Trim() : ""));
                }
            }
        }

        /// <summary>
        /// Adds recipients in the Reply To field of the email 
        /// </summary>
        /// <param name="recipients">a semi-colon delimited list of recipients, optionally each address can include a tab character followed by a display name</param>
        public void AddReplyTo(string recipients)
        {
            if (recipients != null)
            {
                AddReplyTo(recipients.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        /// <summary>
        /// Adds recipients in the Reply To field of the email 
        /// </summary>
        /// <param name="recipients">List of recipient emails, optionally each address can include a tab character followed by a display name</param>
        public void AddReplyTo(List<string> recipients)
        {
            foreach (string r in recipients)
            {
                string[] s = r.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (email.ReplyToList.Where(to => to.Address.Equals(s[0].Trim())).Count() == 0)
                {
                    email.ReplyToList.Add(new MailAddress(s[0].Trim(), s.Length > 1 ? s[1].Trim() : ""));
                }
            }
        }
        /// <summary>
        /// Sets the subject of the email to the provided string
        /// </summary>
        /// <param name="subject">The subject of the email</param>
        public void SetSubject(string subject)
        {
            email.Subject = subject ?? "";
        }

        /// <summary>
        /// Sets the body of a plain text email to the provided string
        /// </summary>
        /// <param name="body">The plain text body of the email</param>
        public void SetTextOnlyBody(string body)
        {
            email.IsBodyHtml = false;
            email.Body = body;
        }

        /// <summary>
        /// Creates the body of the email from the provided template, making replacements from the variables list.
        /// Does nothing if the specified template file does not exist in the EmailTemplates folder.
        /// </summary>
        /// <param name="templatefilename">Email template file to use for the email</param>
        /// <param name="variables">List of variable replacements to apply to the template.</param>
        public void SetTemplateBody(string templatefilename, Dictionary<string, string> variables)
        {
            string RawBody;
            string templatepath = Path.Combine(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "EmailTemplates"), templatefilename);
            if (File.Exists(templatepath))
            {
                email.IsBodyHtml = true;
                using (StreamReader sr = new StreamReader(templatepath))
                {
                    RawBody = sr.ReadToEnd();
                }

                foreach (KeyValuePair<string, string> v in variables)
                {
                    if (RawBody.Contains(v.Key))
                    {
                        RawBody = RawBody.Replace(v.Key, v.Value);
                    }
                }

                RawBody = RawBody.Replace("[[RootUrl]]", "https://mydrawingtracker.com");
                RawBody = RawBody.Replace("[[Year]]", $"{DateTime.Now.Year}");
                RawBody = RawBody.Replace("[[Date]]", $"{DateTime.Now:MM/dd/yyyy}");
                email.Body = RawBody;
            }
            else
            {
                email.IsBodyHtml = false;
            }
        }

        /// <summary>
        /// Get the body of the email message
        /// </summary>
        /// <returns>The email body</returns>
        public string GetMessageBody()
        {
            return email.Body;
        }

        /// <summary>
        /// Sends the email after set up. Requires the To field, Subject, and Body of the email to be set, else returns false.
        /// If the EmailsOn setting is False, message will be saved in the UnsentEmails folder.
        /// </summary>
        /// <returns>success/failure of sending</returns>
        public bool SendMessage(bool includeDev = true)
        {
            if (email.To.Count > 0 && email.Subject.Length > 0 && email.Body.Length > 0)
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += ValidateCertificate;

                    using (SmtpClient smtpClient = new SmtpClient("142.11.247.164", 60447))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential("web@mydrawingtracker.com", "29IQH3xyyDGMubY0q1jl6A"); 
                        smtpClient.Send(email);
                    }
                    return true;

                }
                catch (Exception e)
                {
                    email.Body += $"\n\nEXCEPTION:{e.Message} \n {e.StackTrace}";
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    Exception ie = e.InnerException;
                    while (ie != null)
                    {
                        email.Body += $"\n\nINNER EXCEPTION:{ie.Message} \n {ie.StackTrace}";
                        System.Diagnostics.Debug.WriteLine(ie.Message);
                        ie = e.InnerException;
                    }
                }
            }

            SaveMessage();
            return false;
        }

        private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }


        /// <summary>
        /// Saves the email after set up. Emails are saved in the UnsentEmails folder
        /// </summary>
        /// <returns>success/failure of saving</returns>
        public bool SaveMessage()
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    string unsentfolder = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "UnsentEmails");
                    if (!Directory.Exists(unsentfolder))
                    {
                        Directory.CreateDirectory(unsentfolder);
                    }

                    if (email.To.Count == 0)
                    {
                        email.To.Add("web@mydrawingtracker.com");
                    }
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = unsentfolder;
                    smtpClient.Send(email);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
