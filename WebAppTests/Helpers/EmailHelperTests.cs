﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApp.Helpers;
using WebApp.Models;

namespace WebAppTests.Helpers
{
	[TestClass]
	public class EmailHelperTests
	{
		private const int MailId = 1;
		private const string FromEmailAddress = "from@devmail.havit.cz";

		[TestMethod]
		public void EmailHelper_SendMail_MailNotFound_DoesNothing()
		{
			// arrange
			var emailHelper = new TestableEmailHelper(mailToLoad: null);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.IsTrue(emailHelper.DidNothingIndicated);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailAlreadySent_DoesNothing()
		{
			// arrange
			var emailHelper = new TestableEmailHelper(mailToLoad: new Mail() { MailID = MailId, IsSent =  true });

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
			var emailHelper = new TestableEmailHelper(mailToLoad);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.IsNotNull(emailHelper.MailMessageSent);
			Assert.AreEqual(emailHelper.MailMessageSent.Body, mailToLoad.Body);
			Assert.AreEqual(emailHelper.MailMessageSent.Subject, mailToLoad.Subject);
			Assert.AreEqual(emailHelper.MailMessageSent.From, FromEmailAddress);
			Assert.AreEqual(emailHelper.MailMessageSent.To.ToString(), mailToLoad.To);
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
			var emailHelper = new TestableEmailHelper(mailToLoad);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, emailHelper.MailProvidedToGenerateTemplatedMailMessage);
			Assert.AreSame(emailHelper.MailMessageGeneratedFromTemplate, emailHelper.MailMessageSent);
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
			var emailHelper = new TestableEmailHelper(mailToLoad);

			// act
			emailHelper.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, emailHelper.MailSaved);
			Assert.IsTrue(emailHelper.MailSaved.IsSent);
		}

		private class TestableEmailHelper : EmailHelper
		{
			private readonly Mail mailToLoad;

			public bool DidNothingIndicated { get; private set; }
			public MailMessage MailMessageSent { get; private set; }
			public Mail MailProvidedToGenerateTemplatedMailMessage { get; private set; }
			public Mail MailSaved { get; private set; }
			public MailMessage MailMessageGeneratedFromTemplate { get; private set; }

			public TestableEmailHelper(Mail mailToLoad)
			{
				this.mailToLoad = mailToLoad;
			}

			protected override Mail LoadMail(int mailID, SQLServerContext dbContext)
			{
				if (mailID == MailId)
				{
					return mailToLoad;
				}
				Debug.Fail("Invalid test setup");
				return null;
			}

			protected override SQLServerContext GetDbContext()
			{
				return null;
			}

			protected override void DoNothing()
			{
				this.DidNothingIndicated = true;
			}

			protected override void SendMail(SmtpClient client, MailMessage message)
			{
				this.MailMessageSent = message;
			}

			protected override SmtpClient CreateSmtpClient()
			{
				return null;
			}

			protected override string GetFromEmailAddress(SmtpClient client)
			{
				return FromEmailAddress;
			}

			protected override void SaveMailChanges(SQLServerContext dbContext, Mail mail)
			{
				this.MailSaved = mail;
			}

			protected override MailMessage GenerateTemplatedMailMessage(Mail mail)
			{
				this.MailProvidedToGenerateTemplatedMailMessage = mail;
				this.MailMessageGeneratedFromTemplate = new MailMessage();
				return MailMessageGeneratedFromTemplate;
			}
		}
	}
}