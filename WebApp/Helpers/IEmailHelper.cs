using System;

namespace WebApp.Helpers
{
	public interface IEmailHelper
	{
		void Enqueue(String to, String subject, String template, object parameters);

		void Enqueue(String to, String subject, String body);

		void SendMail(int mailID);
	}
}