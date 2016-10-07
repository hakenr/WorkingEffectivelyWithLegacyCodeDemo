using System.Net.Mail;

namespace Havit.MigrosChester.Services.Infrastructure
{
	public interface IMailSender
	{
		void SendMailMessage(MailMessage mailMessage);
	}
}