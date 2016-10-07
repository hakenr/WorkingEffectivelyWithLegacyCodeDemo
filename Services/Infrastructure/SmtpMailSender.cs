using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Havit.MigrosChester.Services.Infrastructure
{
	public class SmtpMailSender : IMailSender
	{
		public void SendMailMessage(MailMessage mailMessage)
		{
			using (SmtpClient client = new SmtpClient())
			{
				client.Send(mailMessage);
			}
		}
	}
}