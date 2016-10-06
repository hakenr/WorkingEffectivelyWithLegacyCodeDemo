using System;
using System.Threading;
using System.Net;
using System.Net.Mail;

using WebApp.Models;

namespace SyncService
{
    class EmailHelper
    {
        private static void TrySendEmail(String to, String subject, String body)
        {

            try
            {
                // save
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

                // prepare
                SmtpClient client = new SmtpClient();
                NetworkCredential credentials = (NetworkCredential)(client.Credentials);
                String from = credentials.UserName;
                MailMessage message = new MailMessage(from, mail.To, mail.Subject, mail.Body);

                // send
                client.Send(message);
                mail.IsSent = true;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.LogError("could not send email: " + e.Message);
            }
        }

        public static void SendEmail(String to, String subject, String body)
        {

            (new Thread(() => TrySendEmail(to, subject, body))).Start();
        }
    }
}
