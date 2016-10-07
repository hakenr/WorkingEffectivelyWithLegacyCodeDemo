using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
		public void EmailHelper_Enqueue_TemplatedMail_InsertsMailToDb()
		{
			// arrange
			const string to = "to@devmail.havit.cz";
			const string subject = "FAKE SUBJECT";
			const string template = "FAKE TEMPLATE";
			object parameters = new object();
			const string serializedParameters = "FAKE_PARAMS";

			var emailHelperMock = new Mock<EmailHelper>() { CallBase = true };

			Mail mailInserted = null;
			emailHelperMock.Setup(h => h.InsertMailToDb(It.IsAny<Mail>())).Callback<Mail>(mail => { mailInserted = mail; });
			emailHelperMock.Setup(h => h.SerializeParameters(parameters)).Returns(serializedParameters);
			emailHelperMock.Setup(h => h.EnqueueSendMail(It.IsAny<int>())).Verifiable();

			// act
			emailHelperMock.Object.Enqueue(to, subject, template, parameters);

			// assert
			Assert.IsNotNull(mailInserted);
			Assert.AreEqual(to, mailInserted.To);
			Assert.AreEqual(subject, mailInserted.Subject);
			Assert.AreEqual(template, mailInserted.Template);
			Assert.AreEqual(serializedParameters, mailInserted.ParametersJSON);
			Assert.IsFalse(mailInserted.IsSent);
		}

		[TestMethod]
		public void EmailHelper_Enqueue_TemplatedMail_EnqueuesSendMail()
		{
			// arrange
			const string to = "to@devmail.havit.cz";
			const string subject = "FAKE SUBJECT";
			const string template = "FAKE TEMPLATE";
			object parameters = new object();

			var emailHelperMock = new Mock<EmailHelper>() { CallBase = true };

			emailHelperMock.Setup(h => h.InsertMailToDb(It.IsAny<Mail>())).Callback<Mail>(mail => { mail.MailID = MailId; });
			emailHelperMock.Setup(h => h.SerializeParameters(parameters)).Returns<string>(null);
			emailHelperMock.Setup(h => h.EnqueueSendMail(MailId)).Verifiable();

			// act
			emailHelperMock.Object.Enqueue(to, subject, template, parameters);

			// assert
			emailHelperMock.Verify(h => h.EnqueueSendMail(MailId), Times.Once);
		}

		[TestMethod]
		public void EmailHelper_Enqueue_PlainMail_InsertsMailToDb()
		{
			// arrange
			const string to = "to@devmail.havit.cz";
			const string subject = "FAKE SUBJECT";
			const string body = "FAKE BODY";

			var emailHelperMock = new Mock<EmailHelper>() { CallBase = true };

			Mail mailInserted = null;
			emailHelperMock.Setup(h => h.InsertMailToDb(It.IsAny<Mail>())).Callback<Mail>(mail => { mailInserted = mail; });
			emailHelperMock.Setup(h => h.EnqueueSendMail(It.IsAny<int>())).Verifiable();

			// act
			emailHelperMock.Object.Enqueue(to, subject, body);

			// assert
			Assert.IsNotNull(mailInserted);
			Assert.AreEqual(to, mailInserted.To);
			Assert.AreEqual(subject, mailInserted.Subject);
			Assert.AreEqual(body, mailInserted.Body);
			Assert.IsFalse(mailInserted.IsSent);
		}

		[TestMethod]
		public void EmailHelper_Enqueue_PlainMail_EnqueuesSendMail()
		{
			// arrange
			const string to = "to@devmail.havit.cz";
			const string subject = "FAKE SUBJECT";
			const string body = "FAKE BODY";

			var emailHelperMock = new Mock<EmailHelper>() { CallBase = true };

			emailHelperMock.Setup(h => h.InsertMailToDb(It.IsAny<Mail>())).Callback<Mail>(mail => { mail.MailID = MailId; });
			emailHelperMock.Setup(h => h.EnqueueSendMail(MailId)).Verifiable();

			// act
			emailHelperMock.Object.Enqueue(to, subject, body);

			// assert
			emailHelperMock.Verify(h => h.EnqueueSendMail(MailId), Times.Once);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailNotFound_DoesNothing()
		{
			// arrange
			Mail mailToLoad = null;
			var emailHelperMock = BuildEmailHelperMock_SendMail(mailToLoad);

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			emailHelperMock.Verify(h => h.DoNothing(), Times.Once);
		}

		[TestMethod]
		public void EmailHelper_SendMail_MailAlreadySent_DoesNothing()
		{
			// arrange
			Mail mailToLoad = new Mail() { MailID = MailId, IsSent = true };
			var emailHelperMock = BuildEmailHelperMock_SendMail(mailToLoad);

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			emailHelperMock.Verify(h => h.DoNothing(), Times.Once);
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
			var emailHelperMock = BuildEmailHelperMock_SendMail(mailToLoad);

			MailMessage mailSent = null;
			emailHelperMock.Setup(h => h.SendMail(It.IsAny<SmtpClient>(), It.IsAny<MailMessage>()))
				.Callback<SmtpClient, MailMessage>((smtpClient, mailMessage) => { mailSent = mailMessage; });

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.IsNotNull(mailSent);
			Assert.AreEqual(mailSent.Body, mailToLoad.Body);
			Assert.AreEqual(mailSent.Subject, mailToLoad.Subject);
			Assert.AreEqual(mailSent.From, FromEmailAddress);
			Assert.AreEqual(mailSent.To.ToString(), mailToLoad.To);
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
			var emailHelperMock = BuildEmailHelperMock_SendMail(mailToLoad);

			MailMessage mailSent = null;
			emailHelperMock.Setup(h => h.SendMail(It.IsAny<SmtpClient>(), It.IsAny<MailMessage>()))
				.Callback<SmtpClient, MailMessage>((smtpClient, mailMessage) => { mailSent = mailMessage; });

			Mail mailProvidedToGenerateTemplatedMailMessage = null;
			MailMessage mailMessageGeneratedFromTemplate = new MailMessage();
			emailHelperMock.Setup(h => h.GenerateTemplatedMailMessage(It.IsAny<Mail>()))
				.Callback<Mail>((mail) => { mailProvidedToGenerateTemplatedMailMessage = mail; })
				.Returns<Mail>((mail) => mailMessageGeneratedFromTemplate);

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, mailProvidedToGenerateTemplatedMailMessage);
			Assert.AreSame(mailMessageGeneratedFromTemplate, mailSent);
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
			var emailHelperMock = BuildEmailHelperMock_SendMail(mailToLoad);

			Mail mailSaved = null;
			emailHelperMock.Setup(h => h.SaveMailChanges(It.IsAny<SQLServerContext>(), It.IsAny<Mail>()))
				.Callback<SQLServerContext, Mail>((dbContext, mail) => { mailSaved = mail; });

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, mailSaved);
			Assert.IsTrue(mailSaved.IsSent);
		}

		protected virtual Mock<EmailHelper> BuildEmailHelperMock_SendMail(Mail mailToLoad)
		{
			var emailHelperMock = new Mock<EmailHelper>() { CallBase = true };

			emailHelperMock.Setup(h => h.LoadMail(MailId, It.IsAny<SQLServerContext>())).Returns(mailToLoad);
			emailHelperMock.Setup(h => h.GetDbContext()).Returns<SQLServerContext>(null); // default behavioral for non-CallBase mocking
			emailHelperMock.Setup(h => h.CreateSmtpClient()).Returns<SmtpClient>(null);
			emailHelperMock.Setup(h => h.GetFromEmailAddress(It.IsAny<SmtpClient>())).Returns(FromEmailAddress);
			emailHelperMock.Setup(h => h.SendMail(It.IsAny<SmtpClient>(), It.IsAny<MailMessage>())).Verifiable();
			emailHelperMock.Setup(h => h.SaveMailChanges(It.IsAny<SQLServerContext>(), It.IsAny<Mail>())).Verifiable();
			emailHelperMock.Setup(h => h.GenerateTemplatedMailMessage(It.IsAny<Mail>())).Returns<MailMessage>(null);

			return emailHelperMock;
		}
	}
}