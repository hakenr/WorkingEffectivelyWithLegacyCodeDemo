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
		public void Enqueue(String to, String subject, String template, object parameters)
		{
			Mail mail = new Mail
			{
				To = to,
				Subject = subject,
				Template = template,
				ParametersJSON = SerializeParameters(parameters),
				IsSent = false
			};

			InsertMailToDb(mail);

			var mailId = mail.MailID;
			EnqueueSendMail(mailId);
		}

		protected internal virtual string SerializeParameters(object parameters)
		{
			return JsonConvert.SerializeObject(parameters);
		}

		protected internal virtual void EnqueueSendMail(int mailId)
		{
			if (!Debugger.IsAttached)
			{
				BackgroundJob.Enqueue(() => SendMail(mailId));
			}
		}

		protected internal virtual void InsertMailToDb(Mail mail)
		{
			SQLServerContext dbContext = new SQLServerContext();
			dbContext.Mails.Add(mail);
			dbContext.SaveChanges(); // save first
		}

		public void Enqueue(String to, String subject, String body)
		{

			Mail mail = new Mail
			{						 
				To = to,
				Subject = subject,
				Body = body,
				IsSent = false
			};

			InsertMailToDb(mail);

			var mailId = mail.MailID;
			EnqueueSendMail(mailId);
		}

		public void SendMail(int mailID)
		{
			// get entity
			var dbContext = GetDbContext();
			var mail = LoadMail(mailID, dbContext);
			if (mail == null)
			{
				DoNothing();
				return;
			}

			if (mail.IsSent)
			{
				DoNothing();
				return;
			}

			// get client
			var client = CreateSmtpClient();
			var fromEmailAddress = GetFromEmailAddress(client);

			MailMessage message;
			if (mail.Template == null)
			{

				message = new MailMessage(fromEmailAddress, mail.To, mail.Subject, mail.Body);

			}
			else
			{

				// fill template parameters
				message = GenerateTemplatedMailMessage(mail);
				message.From = new MailAddress(fromEmailAddress);
				message.To.Add(mail.To);
				message.Subject = mail.Subject;
			}

			// send
			SendMail(client, message);
			mail.IsSent = true;
			SaveMailChanges(dbContext, mail);
		}

		// TODO: Extract to standalone testable service
		protected internal virtual MailMessage GenerateTemplatedMailMessage(Mail mail)
		{
			DynamicViewBag bag = new DynamicViewBag();
			Dictionary<String, String> parameters = JsonConvert.DeserializeObject<Dictionary<String, String>>(mail.ParametersJSON);
			foreach (KeyValuePair<String, String> entry in parameters)
			{
				bag.AddValue(entry.Key, entry.Value);
			}

			// create mail
			RazorMailMessageFactory razorMailMessageFactory = new RazorMailMessageFactory();
			var message = razorMailMessageFactory.Create(mail.Template, new { }, bag);
			return message;
		}

		protected internal virtual void SaveMailChanges(SQLServerContext dbContext, Mail mail)
		{
			dbContext.SaveChanges();
		}

		protected internal virtual void SendMail(SmtpClient client, MailMessage message)
		{
			client.Send(message);
		}

		protected internal virtual string GetFromEmailAddress(SmtpClient client)
		{
			NetworkCredential credentials = (NetworkCredential)(client.Credentials);
			return credentials.UserName;
		}

		protected internal virtual SmtpClient CreateSmtpClient()
		{
			SmtpClient client = new SmtpClient();
			return client;
		}

		protected internal virtual void DoNothing()
		{
			Debug.Write("Indication for test. Does nothing ;-)");
		}

		protected internal virtual Mail LoadMail(int mailID, SQLServerContext dbContext)
		{
			Mail mail = dbContext.Mails.Find(mailID);
			return mail;
		}

		protected internal virtual SQLServerContext GetDbContext()
		{
			SQLServerContext dbContext = new SQLServerContext();
			return dbContext;
		}
	}
}