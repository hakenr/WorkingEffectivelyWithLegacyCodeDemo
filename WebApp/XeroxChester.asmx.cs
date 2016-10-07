using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using SharedLibs.Chester;
using SharedLibs.Model;

using WebApp.Models;
using WebApp.Helpers;

using Newtonsoft.Json;

using Hangfire;
using Havit.MigrosChester.Services.Infrastructure;

namespace WebApp
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class XeroxChester : System.Web.Services.WebService
	{
		private static readonly object Locker = new object();

		public static String WebServiceName = "Chester Migros Integration Web Service";

		public Options Options = null;

		private String cookies;

		// dependencies
		private readonly EmailHelper emailHelper;

		public XeroxChester()
		{
			emailHelper = new EmailHelper(new SmtpMailSender());

			// check config
			if (!File.Exists(Options.getSettingsFile()))
			{
				throw new Exception("settings file missing");
			}

			// parse config
			StreamReader reader = new StreamReader(Options.getSettingsFile());
			String contents = reader.ReadToEnd();
			Options = JsonConvert.DeserializeObject<Options>(contents);

			Security security = new Security(Options.ChesterSecurityEndpoint);
			bool result = security.Login(Options.ChesterUsername, Options.ChesterPassword);
			if (!result)
			{
				throw new Exception("Could not login with username: " + Options.ChesterUsername + " and password: ******");
			}
			cookies = security.Cookies;
		}

		public static String Normalize(String input)
		{

			return (input == null) ? "" : input.Trim();
		}

		[WebMethod]
		public string Create(string user, string password, string Migros_Cagri_No, string Cihaz_Seri_No, string Magaza_Kodu, string Cagri_Ilgilisi, string Cagri_Durum_Kodu, string Magaza_Telefonu, string Cagri_Aciklamasi, System.DateTime Cagri_Acilis_Tarih_Saati)
		{
			SQLServerContext dbContext = new SQLServerContext();

			// NORMALIZE
			user = Normalize(user);
			password = Normalize(password);
			Migros_Cagri_No = Normalize(Migros_Cagri_No);
			Cihaz_Seri_No = Normalize(Cihaz_Seri_No);
			Magaza_Kodu = Normalize(Magaza_Kodu);
			Cagri_Ilgilisi = Normalize(Cagri_Ilgilisi);
			Cagri_Durum_Kodu = Normalize(Cagri_Durum_Kodu);
			Magaza_Telefonu = Normalize(Magaza_Telefonu);
			Cagri_Aciklamasi = Normalize(Cagri_Aciklamasi);

			// STORE INITIAL PARAMS
			ServiceCall dbCall = new ServiceCall
			{
				// params
				MigrosCagriNo = Migros_Cagri_No,
				CihazSeriNo = Cihaz_Seri_No,
				MagazaKodu = Magaza_Kodu,
				CagriIlgilisi = Cagri_Ilgilisi,
				CagriDurumKodu = Cagri_Durum_Kodu,
				MagazaTelefonu = Magaza_Telefonu,
				CagriAciklamasi = Cagri_Aciklamasi,
				CagriAcilisTarihSaati = Cagri_Acilis_Tarih_Saati,
				// other
				ChesterServiceCallID = 0, // not known yet
				IsSuccessful = false,
				ParentServiceCallID = 0,
				HasChild = false
			};
			dbContext.ServiceCalls.Add(dbCall);
			dbContext.SaveChanges();

			int newChesterServiceCallID = -1;
			try
			{
				// REQUIRED FIELDS
				if (Migros_Cagri_No == "")
				{
					throw new Exception("Migros_Cagri_No gerekli");
				}
				if (Cihaz_Seri_No == "")
				{
					throw new Exception("Cihaz_Seri_No gerekli");
				}

				// create service
				CustomerService customerService = new CustomerService(Options.ChesterCustomerEndpoint);
				customerService.Cookies = cookies;

				// equipment id
				EquipmentWso[] equipments = customerService.GetEquipments(null, Cihaz_Seri_No);
				bool hasEquipments = equipments.Length > 0;
				bool usedDefaultSerial = false;
				String oldSerial = "";

				// use default serial number if necessary and present
				if (!hasEquipments && Options.DefaultSerialNumber != null && Options.DefaultSerialNumber != "")
				{
					usedDefaultSerial = true;
					oldSerial = Cihaz_Seri_No;
					Cihaz_Seri_No = Options.DefaultSerialNumber;
					equipments = customerService.GetEquipments(null, Cihaz_Seri_No);
					hasEquipments = equipments.Length > 0;
				}

				if (!hasEquipments)
				{
					if (usedDefaultSerial)
					{
						throw new Exception(
							"Ekipmanın kontratı yok yada kontrata eklenmesi atlanmış, varsayılan seri no ile de (" + Options.DefaultSerialNumber + ") ekipman bulunamadı.");
					}
					else
					{
						throw new Exception(
							"Ekipmanın kontratı yok yada kontrata eklenmesi atlanmış.");
					}
				}

				EquipmentWso theEquipment = equipments[0];

				// PREPARE PARAMS FOR CREATESERVICECALL
				int equipmentID = theEquipment.EquipmentID;
				String equipmentSerialNumber = Cihaz_Seri_No;
				String title = CreateTitle(Magaza_Telefonu, Cagri_Aciklamasi);
				String contactPersonName = ExtractContactPersonName(Cagri_Ilgilisi);
				String description = CreateDescription(Cagri_Aciklamasi, contactPersonName, Magaza_Telefonu, Magaza_Kodu);
				int callTypeID = Options.ChesterCallTypeID;
				String customerReference = Migros_Cagri_No;
				int addressID = theEquipment.AddressID;
				int contactPersonID = Options.ChesterContactPersonID;

				// UPDATE DB RECORD WITH ADDITIONAL PARAMS BEFORE CALL
				dbCall.EquipmentID = equipmentID;
				dbCall.AddressID = addressID;
				dbCall.ContactPersonID = contactPersonID;
				dbCall.ContactPersonName = contactPersonName;
				dbContext.SaveChanges();

				CreatedServiceCallWso createdServiceCall = null;

				try
				{
					// CREATE SERVICE CALL
					createdServiceCall = customerService.CreateServiceCall(
						equipmentID,
						equipmentSerialNumber,
						title,
						description,
						callTypeID,
						customerReference,
						addressID,
						contactPersonID
					);
				}
				catch (Exception e2)
				{
					throw new Exception("Error response from Chester.CustomerService.CreateServiceCall: " + e2.Message);
				}

				newChesterServiceCallID = createdServiceCall.ServiceCallID;

				// UPDATE STATUS AND ID
				dbCall.ChesterServiceCallID = newChesterServiceCallID;
				dbCall.IsSuccessful = true;
				dbContext.SaveChanges();
			}
			catch (Exception e)
			{
				// LOG EXCEPTION MESSAGES
				dbCall.Message = e.Message;
				dbContext.SaveChanges();

				emailHelper.Enqueue(Options.ErrorNotificationEmail, WebServiceName + " CREATE", e.Message);

				throw;
			}

			if (!Debugger.IsAttached)
			{
				BackgroundJob.Schedule(
					() => CheckNotificationNumber(dbCall.ServiceCallID),
					TimeSpan.FromMinutes(Options.NotificationNumberCheckDelayInMinutes)
				);
			}

			return newChesterServiceCallID + "";
		}

		[WebMethod]
		public string Update(string user, string password, string Migros_Cagri_No, string Cagri_Aciklamasi, string Cagri_Durumu)
		{
			int newChesterServiceCallID = -1;
			ServiceCall dbCall = null;
			ServiceCall newDbCall = null;
			SQLServerContext dbContext = new SQLServerContext();

			// NORMALIZE
			user = Normalize(user);
			password = Normalize(password);
			Migros_Cagri_No = Normalize(Migros_Cagri_No);
			Cagri_Aciklamasi = Normalize(Cagri_Aciklamasi);
			Cagri_Durumu = Normalize(Cagri_Durumu);

			if (Cagri_Durumu == "")
			{
				Cagri_Durumu = "UPDATE";
			}
			else
			{
				Cagri_Durumu = Cagri_Durumu.ToUpper();
			}

			try
			{
				// GET EXISTING SERVICE CALL FROM DB (THE NEWEST ONE)
				var query = from c in dbContext.ServiceCalls
							where c.MigrosCagriNo == Migros_Cagri_No && c.HasChild == false && c.IsSuccessful == true
							orderby c.ServiceCallID descending
							select c;
				dbCall = query.FirstOrDefault();
				if (dbCall == null)
				{
					throw new Exception("Migros_Cagri_No (" + Migros_Cagri_No + ") ile bir kayıt bulunamadı.");
				}
				int chesterServiceCallID = dbCall.ChesterServiceCallID;

				CustomerService customerService = new CustomerService(Options.ChesterCustomerEndpoint);
				customerService.Cookies = cookies;

				// PREPARE PARAMS FOR CREATESERVICECALL
				int equipmentID = dbCall.EquipmentID;
				String equipmentSerialNumber = dbCall.CihazSeriNo;
				String title = CreateTitle(Cagri_Durumu, dbCall.MagazaTelefonu, Cagri_Aciklamasi);
				String contactPersonName = dbCall.ContactPersonName;
				String description = CreateDescription(Cagri_Aciklamasi, contactPersonName, dbCall.MagazaTelefonu, dbCall.MagazaKodu);
				int callTypeID = Options.ChesterCallTypeID;
				String customerReference = Migros_Cagri_No;
				int addressID = dbCall.AddressID;
				int contactPersonID = Options.ChesterContactPersonID;

				// FILL NEW RECORD WITH UPDATED DATA
				newDbCall = new ServiceCall
				{
					// params
					MigrosCagriNo = Migros_Cagri_No,
					CihazSeriNo = dbCall.CihazSeriNo,
					MagazaKodu = dbCall.MagazaKodu,
					CagriIlgilisi = dbCall.CagriIlgilisi,
					CagriDurumKodu = Cagri_Durumu,
					MagazaTelefonu = dbCall.MagazaTelefonu,
					CagriAciklamasi = Cagri_Aciklamasi,
					CagriAcilisTarihSaati = DateTime.Now,
					// additional
					EquipmentID = dbCall.EquipmentID,
					AddressID = dbCall.AddressID,
					ContactPersonID = dbCall.ContactPersonID,
					ContactPersonName = dbCall.ContactPersonName,
					// other
					ChesterServiceCallID = 0, // not known yet
					IsSuccessful = false,
					ParentServiceCallID = dbCall.ServiceCallID,
					HasChild = false,
					IsCompleted = false,
					IsCancelled = false,
					IsMissing = false
				};
				dbContext.ServiceCalls.Add(newDbCall);
				dbContext.SaveChanges();

				CreatedServiceCallWso newChesterServiceCall = null;
				try
				{
					// CREATE SERVICE CALL
					newChesterServiceCall = customerService.CreateServiceCall(
						equipmentID,
						equipmentSerialNumber,
						title,
						description,
						callTypeID,
						customerReference,
						addressID,
						contactPersonID
					);
				}
				catch (Exception e2)
				{
					throw new Exception("Error response from Chester.CustomerService.CreateServiceCall: " + e2.Message);
				}

				newChesterServiceCallID = newChesterServiceCall.ServiceCallID;

				using (var dbContextTransaction = dbContext.Database.BeginTransaction())
				{
					try
					{
						// UPDATE STATUS AND ID
						newDbCall.ChesterServiceCallID = newChesterServiceCallID;
						newDbCall.IsSuccessful = true;

						// has a new child now
						dbCall.HasChild = true;

						dbContext.SaveChanges();
						dbContextTransaction.Commit();
					}
					catch (Exception)
					{
						dbContextTransaction.Rollback();
						throw;
					}
				}

				if (!Debugger.IsAttached)
				{
					BackgroundJob.Enqueue(() => PrepareAndSendUpdateNotification(dbCall.ServiceCallID, newDbCall.ServiceCallID));
				}
			}
			catch (Exception e)
			{

				// LOG EXCEPTION MESSAGES
				if (newDbCall != null)
				{
					newDbCall.Message = e.Message;
					dbContext.SaveChanges();
				}

				emailHelper.Enqueue(Options.ErrorNotificationEmail, WebServiceName + " UPDATE", e.Message);

				throw;
			}

			return newChesterServiceCallID + "";
		}

		public void CheckNotificationNumber(int dbCallID)
		{

			SQLServerContext dbContext = new SQLServerContext();
			ServiceCall dbCall = dbContext.ServiceCalls.Find(dbCallID);
			if (dbCall == null)
			{
				return;
			}

			// check config
			if (!File.Exists(Options.getSettingsFile()))
			{
				throw new Exception("config json missing");
			}

			// parse config
			StreamReader reader = new StreamReader(Options.getSettingsFile());
			String contents = reader.ReadToEnd();
			Options options = JsonConvert.DeserializeObject<Options>(contents);

			// login
			SharedLibs.Chester.Security security = new SharedLibs.Chester.Security(options.ChesterSecurityEndpoint);
			try
			{
				if (!security.Login(options.ChesterUsername, options.ChesterPassword))
				{
					throw new Exception("Could not login with username: " + options.ChesterUsername + " and password: ******");
				}
			}
			catch (Exception e)
			{
				throw new Exception("could not login: " + e.Message);
			}

			CustomerService customerService = new CustomerService(options.ChesterCustomerEndpoint);
			customerService.Cookies = security.Cookies;

			ServiceCallWso[] fetchedChesterCalls = new ServiceCallWso[] { };
			ServiceCallWso chesterCall;

			try
			{
				fetchedChesterCalls = customerService.GetServiceCalls(dbCall.ChesterServiceCallID, "", "", null, "", null, null, null, null);
			}
			catch (Exception e)
			{
				throw new Exception("could not fetch service calls from chester: " + e.Message);
			}

			if (fetchedChesterCalls.Length < 1)
			{
				return;
			}
			chesterCall = fetchedChesterCalls[0];

			// check
			if (chesterCall.NotificationNumber != null && chesterCall.NotificationNumber != "")
			{
				return; // success
			}

			String subject = "Servis Bilgilendirme Numarası Atanmamış (" + dbCall.ChesterServiceCallID + ")";
			String body = String.Join("\n", new String[] 
			{
				GetDateTime(chesterCall.Reported),
				"tarihinde açılmış",
				dbCall.MigrosCagriNo + " müşteri referanslı",
				"servis çağrısına halen servis bilgilendirme numarası atanmamıştır."
			});

			Mail mail = dbContext.Mails.Where(m => m.Subject == subject).FirstOrDefault();
			if (mail == null)
			{
				mail = new Mail
				{
					To = options.UpdateNotificationEmail,
					Subject = subject,
					Body = body,
					IsSent = false
				};
				dbContext.Mails.Add(mail);
				dbContext.SaveChanges();
			}

			emailHelper.SendMail(mail.MailID);
		}

		public void PrepareAndSendUpdateNotification(int oldServiceCallDBID, int newServiceCallDBID)
		{

			SQLServerContext dbContext = new SQLServerContext();
			ServiceCall oldServiceCallFromDB = dbContext.ServiceCalls.Find(oldServiceCallDBID);
			ServiceCall newServiceCallFromDB = dbContext.ServiceCalls.Find(newServiceCallDBID);
			if (oldServiceCallFromDB == null || newServiceCallFromDB == null)
			{
				//throw new Exception("could not find service calls in DB ('" + oldServiceCallDBID + "' and '" + newServiceCallDBID + "')");
				return;
			}

			// check config
			if (!File.Exists(Options.getSettingsFile()))
			{
				throw new Exception("config json missing");
			}

			// parse config
			StreamReader reader = new StreamReader(Options.getSettingsFile());
			String contents = reader.ReadToEnd();
			Options options = JsonConvert.DeserializeObject<Options>(contents);

			// login
			SharedLibs.Chester.Security security = new SharedLibs.Chester.Security(options.ChesterSecurityEndpoint);
			try
			{
				if (!security.Login(options.ChesterUsername, options.ChesterPassword))
				{
					throw new Exception("Could not login with username: " + options.ChesterUsername + " and password: ******");
				}
			}
			catch (Exception e)
			{
				throw new Exception("could not login: " + e.Message);
			}

			SharedLibs.Chester.CustomerService customerService = new SharedLibs.Chester.CustomerService(options.ChesterCustomerEndpoint);
			customerService.Cookies = security.Cookies;

			ServiceCallWso[] fetchedOldServiceCallsFromChester = new ServiceCallWso[] { };
			ServiceCallWso[] fetchedNewServiceCallsFromChester = new ServiceCallWso[] { };
			ServiceCallWso theOldServiceCallFromChester;
			ServiceCallWso theNewServiceCallFromChester;

			try
			{

				fetchedOldServiceCallsFromChester = customerService.GetServiceCalls(oldServiceCallFromDB.ChesterServiceCallID, "", "", null, "", null, null, null, null);
				fetchedNewServiceCallsFromChester = customerService.GetServiceCalls(newServiceCallFromDB.ChesterServiceCallID, "", "", null, "", null, null, null, null);

			}
			catch (Exception e)
			{
				throw new Exception("could not fetch service calls from chester: " + e.Message);
			}

			if (fetchedOldServiceCallsFromChester.Length < 1 || fetchedNewServiceCallsFromChester.Length < 1)
			{
				return;
			}

			theOldServiceCallFromChester = fetchedOldServiceCallsFromChester[0];
			theNewServiceCallFromChester = fetchedNewServiceCallsFromChester[0];

			String oldNotificationNumber = theOldServiceCallFromChester.NotificationNumber;
			String newNotificationNumber = theNewServiceCallFromChester.NotificationNumber;

			if (oldNotificationNumber == null || oldNotificationNumber == "" || newNotificationNumber == null || newNotificationNumber == "")
			{
				throw new Exception("missing notification numbers: ('" + oldNotificationNumber + "', '" + newNotificationNumber + "')");
			}

			String subject = "Servis Çağrısı Güncellemesi (" + oldNotificationNumber + " -> " + newNotificationNumber + ")";
			String body = String.Join("\n", new String[] 
			{
				"Durum: " + newServiceCallFromDB.CagriDurumKodu,
				"Açıklama: " + newServiceCallFromDB.CagriAciklamasi,
				"Lütfen eski çağrıyı kapatınız, yeni çağrı üzerinden ilerleyiniz."
			});

			Mail mail = dbContext.Mails.Where(m => m.Subject == subject).FirstOrDefault();
			if (mail == null)
			{
				mail = new Mail
				{
					To = options.UpdateNotificationEmail,
					Subject = subject,
					Body = body,
					IsSent = false
				};
				dbContext.Mails.Add(mail);
				dbContext.SaveChanges();
			}

			emailHelper.SendMail(mail.MailID);
		}

		private static String ExtractContactPersonName(String cagriIlgilisi)
		{
			if (!cagriIlgilisi.Contains("("))
			{
				return cagriIlgilisi;
			}

			String[] contactPersonParts = cagriIlgilisi.Split(new char[] { ' ' });
			List<String> list = new List<String>(contactPersonParts);
			if (list.Count > 1)
			{
				int lastIndex = list.Count - 1;
				String contactPersonIDStr = list[lastIndex];
				list.RemoveAt(lastIndex);
			}
			return String.Join(" ", list.ToArray());
		}

		private static String CreateTitle(String magazaTelefonu, String cagriAciklamasi)
		{
			String title = magazaTelefonu + ", " + cagriAciklamasi;
			title = title.Substring(0, Math.Min(40, title.Length));
			return title;
		}

		private static String CreateTitle(String prefix, String magazaTelefonu, String cagriAciklamasi)
		{
			String title = prefix + ", " + magazaTelefonu + ", " + cagriAciklamasi;
			title = title.Substring(0, Math.Min(40, title.Length));
			return title;
		}

		private static String CreateDescription(String cagriAciklamasi, String contactPersonName, String magazaTelefonu, String magazaKodu)
		{

			String description = cagriAciklamasi + "\n" +
				"İlgili: " + contactPersonName + "\n" +
				"Tel: " + magazaTelefonu + "\n" +
				"M. Kodu: " + magazaKodu;
			return description;
		}

		public void Log(String message)
		{
			this.Log(EventLogEntryType.Information, message);
		}

		public void LogError(String message)
		{
			this.Log(EventLogEntryType.Error, message);
		}

		public void LogWarning(String message)
		{
			this.Log(EventLogEntryType.Warning, message);
		}

		public void Log(EventLogEntryType level, String message)
		{
			// EVENT LOG
			EventLog.WriteEntry(WebServiceName, message, level);
		}

		private static String GetDateTime(DateTime dateTime)
		{

			string format = "yyyy-MM-dd HH:mm:ss";
			return "[" + dateTime.ToString(format) + "]";
		}

		private static String GetDateTime()
		{
			DateTime now = DateTime.Now;
			return GetDateTime(now);
		}
	}
}
