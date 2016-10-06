using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using RazorMailMessage;
using RazorEngine.Templating;
using WebApp.Models;
using Newtonsoft.Json;
using Hangfire;
using System.Diagnostics;

namespace WebApp.Helpers
{
    public class EmailHelper
    {
        public static void Enqueue(String to, String subject, String template, object parameters) {

            SQLServerContext dbContext = new SQLServerContext();
            Mail mail = new Mail
            {
                To = to,
                Subject = subject,
                Template = template,
                ParametersJSON = JsonConvert.SerializeObject(parameters),
                IsSent = false
            };
            dbContext.Mails.Add(mail);
            dbContext.SaveChanges(); // save first

            if (!Debugger.IsAttached) {
                BackgroundJob.Enqueue(() => SendMail(mail.MailID));
            }
        }

        public static void Enqueue(String to, String subject, String body) {

            SQLServerContext dbContext = new SQLServerContext();
            Mail mail = new Mail
            {
                To = to,
                Subject = subject,
                Body = body,
                IsSent = false
            };
            dbContext.Mails.Add(mail);
            dbContext.SaveChanges(); // save first

            if (!Debugger.IsAttached) {
                BackgroundJob.Enqueue(() => SendMail(mail.MailID));
            }
        }

        public static void SendMail(int mailID)
        {
            // get entity
            SQLServerContext dbContext = new SQLServerContext();
            Mail mail = dbContext.Mails.Find(mailID);
            if (mail == null) {
                return;
            }

            if (mail.IsSent) {
                return;
            }

            // get client
            SmtpClient client = new SmtpClient();
            NetworkCredential credentials = (NetworkCredential)(client.Credentials);
            String from = credentials.UserName;
            MailMessage message;

            if (mail.Template == null) {

                message = new MailMessage(from, mail.To, mail.Subject, mail.Body);

            } else {
            
                // fill template parameters
                DynamicViewBag bag = new DynamicViewBag();
                Dictionary<String, String> parameters = JsonConvert.DeserializeObject<Dictionary<String, String>>(mail.ParametersJSON);
                foreach (KeyValuePair<String, String> entry in parameters)
                {
                    bag.AddValue(entry.Key, entry.Value);
                }

                // create mail
                RazorMailMessageFactory razorMailMessageFactory = new RazorMailMessageFactory();
                message = razorMailMessageFactory.Create(mail.Template, new { }, bag);
                message.From = new MailAddress(from);
                message.To.Add(mail.To);
                message.Subject = mail.Subject;
            }

            // send
            client.Send(message);
            mail.IsSent = true;
            dbContext.SaveChanges();
        }
    }
}