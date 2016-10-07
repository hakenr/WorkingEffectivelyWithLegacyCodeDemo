using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Havit.MigrosChester.Services.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApp.Helpers;
using WebApp.Models;

namespace WebAppTests.Helpers
{
	[TestClass]
	public class EmailHelperPoorMansTests
	{
		private const int MailId = 1;
		private const string FromEmailAddress = "from@devmail.havit.cz";

		[TestMethod]
		public void EmailHelper_SendMail_MailNotFound_DoesNothing()
		{
			// arrange
			var emailHelper = new TestableEmailHelper(mailToLoad: null, mailSender: new FakeMailSender());

			// act
			emailHelper.SendMail(1);

			// assert
			Assert.IsTrue(emailHelper.DidNothingIndicated);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailAlreadySent_DoesNothing()
		{
			// arrange
			var emailHelper = new TestableEmailHelper(mailToLoad: new Mail() { IsSent = true }, mailSender: new FakeMailSender());

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.IsTrue(emailHelper.DidNothingIndicated);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailWithoutTemplate_SendsPlainMessage()
		{
			// arrange
			var mailToLoad = new Mail()
			{
				MailID = MailId,
				Body = "FAKE_BODY",
				Subject = "FAKE_SUBJECT",
				IsSent = false,
				To = "to@devmail.havit.cz"
			};
			var fakeMailSender = new FakeMailSender();
			var emailHelper = new TestableEmailHelper(mailToLoad, fakeMailSender);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.IsNotNull(fakeMailSender.MailMessageSent);
			Assert.AreEqual(fakeMailSender.MailMessageSent.Body, mailToLoad.Body);
			Assert.AreEqual(fakeMailSender.MailMessageSent.Subject, mailToLoad.Subject);
			Assert.AreEqual(fakeMailSender.MailMessageSent.From, FromEmailAddress);
			Assert.AreEqual(fakeMailSender.MailMessageSent.To.ToString(), mailToLoad.To);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailWithTemplate_SendsMessageGeneratedFromTemplate()
		{
			// arrange
			var mailToLoad = new Mail()
			{
				MailID = MailId,
				Body = "FAKE_BODY",
				Subject = "FAKE_SUBJECT",
				IsSent = false,
				To = "to@devmail.havit.cz",
				Template = "TEMPLATE",
				ParametersJSON = "PARAMETERS"
			};
			var fakeMailSender = new FakeMailSender();
			var emailHelper = new TestableEmailHelper(mailToLoad, fakeMailSender);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, emailHelper.MailProvidedToGenerateTemplatedMailMessage);
			Assert.AreSame(emailHelper.MailMessageGeneratedFromTemplate, fakeMailSender.MailMessageSent);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailToSend_MarksMailAsSentAndSavesChanges()
		{
			// arrange
			var mailToLoad = new Mail()
			{
				MailID = MailId,
				Body = "FAKE_BODY",
				Subject = "FAKE_SUBJECT",
				IsSent = false,
				To = "to@devmail.havit.cz"
			};
			var emailHelper = new TestableEmailHelper(mailToLoad, new FakeMailSender());

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, emailHelper.MailSaved);
			Assert.IsTrue(emailHelper.MailSaved.IsSent);
		}

		internal class TestableEmailHelper : EmailHelper
		{
			private readonly Mail mailToLoad;

			public bool DidNothingIndicated { get; private set; }
			public Mail MailProvidedToGenerateTemplatedMailMessage { get; private set; }
			public Mail MailSaved { get; private set; }
			public MailMessage MailMessageGeneratedFromTemplate { get; private set; }

			public TestableEmailHelper(Mail mailToLoad, IMailSender mailSender) : base(mailSender)
			{
				this.mailToLoad = mailToLoad;
			}

			protected internal override Mail LoadMail(int mailID, SQLServerContext dbContext)
			{
				if (mailID == MailId)
				{
					return mailToLoad;
				}
				return null;
			}

			protected internal override SQLServerContext GetDbContext()
			{
				return null;
			}

			protected internal override void DoNothing()
			{
				this.DidNothingIndicated = true;
			}

			protected internal override SmtpClient CreateSmtpClient()
			{
				return null;
			}

			protected internal override string GetFromEmailAddress(SmtpClient client)
			{
				return FromEmailAddress;
			}

			protected internal override void SaveMailChanges(SQLServerContext dbContext, Mail mail)
			{
				this.MailSaved = mail;
			}

			protected internal override MailMessage GenerateTemplatedMailMessage(Mail mail)
			{
				this.MailProvidedToGenerateTemplatedMailMessage = mail;
				this.MailMessageGeneratedFromTemplate = new MailMessage();
				return MailMessageGeneratedFromTemplate;
			}
		}

		internal class FakeMailSender : IMailSender
		{
			public MailMessage MailMessageSent { get; private set; }

			public void SendMailMessage(MailMessage mailMessage)
			{
				this.MailMessageSent = mailMessage;
			}
		}
	}

}