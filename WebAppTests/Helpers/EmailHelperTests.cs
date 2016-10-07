using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Havit.MigrosChester.Services.Infrastructure;
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

			var emailHelperMock = new Mock<EmailHelper>(new Mock<IMailSender>().Object) { CallBase = true };

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

			var emailHelperMock = new Mock<EmailHelper>(new Mock<IMailSender>().Object) { CallBase = true };

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

			var emailHelperMock = new Mock<EmailHelper>(new Mock<IMailSender>().Object) { CallBase = true };

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

			var emailHelperMock = new Mock<EmailHelper>(new Mock<IMailSender>().Object) { CallBase = true };

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
			var fixture = new EmailHelperFixture().SetupWithMailToLoad(null);
			var emailHelperMock = fixture.CreateSutMock();

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
			var fixture = new EmailHelperFixture().SetupWithMailToLoad(mailToLoad);
			var emailHelperMock = fixture.CreateSutMock();

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
			var fixture = new EmailHelperFixture().SetupWithMailToLoad(mailToLoad);
			var emailHelperMock = fixture.CreateSutMock();

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.IsNotNull(fixture.MailMessageSent);
			Assert.AreEqual(fixture.MailMessageSent.Body, mailToLoad.Body);
			Assert.AreEqual(fixture.MailMessageSent.Subject, mailToLoad.Subject);
			Assert.AreEqual(fixture.MailMessageSent.From, FromEmailAddress);
			Assert.AreEqual(fixture.MailMessageSent.To.ToString(), mailToLoad.To);
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
			var fixture = new EmailHelperFixture().SetupWithMailToLoad(mailToLoad);
			var emailHelperMock = fixture.CreateSutMock();

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, fixture.MailProvidedToGenerateTemplatedMailMessage);
			Assert.AreSame(fixture.MailMessageGeneratedFromTemplate, fixture.MailMessageSent);
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
			var fixture = new EmailHelperFixture().SetupWithMailToLoad(mailToLoad);
			var emailHelperMock = fixture.CreateSutMock();

			// act
			emailHelperMock.Object.SendMail(MailId);

			// assert
			Assert.AreSame(mailToLoad, fixture.MailSaved);
			Assert.IsTrue(fixture.MailSaved.IsSent);
		}

		private class EmailHelperFixture
		{
			private Mail mailToLoad;

			internal MailMessage MailMessageGeneratedFromTemplate { get; } = new MailMessage();

			internal Mail MailProvidedToGenerateTemplatedMailMessage { get; private set; }
			internal MailMessage MailMessageSent { get; private set; }
			internal Mail MailSaved { get; private set; }
			public EmailHelperFixture SetupWithMailToLoad(Mail mail)
			{
				this.mailToLoad = mail;

				return this;
			}

			internal Mock<EmailHelper> CreateSutMock()
			{
				var mailSenderMock = new Mock<IMailSender>();
				mailSenderMock.Setup(h => h.SendMailMessage(It.IsAny<MailMessage>()))
					.Callback<MailMessage>(mailMessage => { this.MailMessageSent = mailMessage; });

				var emailHelperMock = new Mock<EmailHelper>(mailSenderMock.Object) { CallBase = true };

				emailHelperMock.Setup(h => h.LoadMail(MailId, It.IsAny<SQLServerContext>())).Returns(mailToLoad);
				emailHelperMock.Setup(h => h.GetDbContext()).Returns<SQLServerContext>(null); // default behavioral for non-CallBase mocking
				emailHelperMock.Setup(h => h.CreateSmtpClient()).Returns<SmtpClient>(null);
				emailHelperMock.Setup(h => h.GetFromEmailAddress(It.IsAny<SmtpClient>())).Returns(FromEmailAddress);
				emailHelperMock.Setup(h => h.GenerateTemplatedMailMessage(It.IsAny<Mail>())).Returns<MailMessage>(null);

				emailHelperMock.Setup(h => h.SaveMailChanges(It.IsAny<SQLServerContext>(), It.IsAny<Mail>()))
					.Callback<SQLServerContext, Mail>((dbContext, mail) => { this.MailSaved = mail; });

				emailHelperMock.Setup(h => h.GenerateTemplatedMailMessage(It.IsAny<Mail>()))
					.Callback<Mail>((mail) => { MailProvidedToGenerateTemplatedMailMessage = mail; })
					.Returns<Mail>((mail) => MailMessageGeneratedFromTemplate);


				return emailHelperMock;
			}
		}
	}
}