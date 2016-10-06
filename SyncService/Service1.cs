using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using SharedLibs.Chester;
using SharedLibs.Model;

using WebApp.Models;

namespace SyncService {
    public partial class Service1 : ServiceBase {

        public static String serviceName = "Chester Migros Sync Service";
        public static Options options = null;

        public bool isRunning = true;

        public Service1() {

            InitializeComponent();

            this.ServiceName = serviceName;
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;

            //Trust all certificates (for web requests)
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        public void OnStartPublic(string[] args) {
            this.OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            // check config, stop if missing
            if (!File.Exists(Options.getSettingsFile())) {
                Logger.LogError("config file does not exist: " + Options.getSettingsFile());
                (new Thread(() => this.StopService())).Start();
                return;
            }

            // parse config
            StreamReader reader = new StreamReader(Options.getSettingsFile());
            String contents = reader.ReadToEnd();
            options = JsonConvert.DeserializeObject<Options>(contents);
            Logger.Log("options: " + options.toJson());
            
            // start
            (new Thread(() => this.MainLoop())).Start();
            Logger.Log("service started...");
        }

        private void StopService() {

            Thread.Sleep(5000);
            this.Stop();
        }

        private void MainLoop() {

            while (this.isRunning) {

                try {
                    this.CheckServiceCalls();
                } catch (Exception e) {
                    String message = "unexpected error: " + e.Message;
                    Logger.LogError(message);
                    EmailHelper.SendEmail(options.ErrorNotificationEmail, serviceName, message);
                }

                // sleep in short intervals to check if service is stopped or not
                int maxSleepTimeInSeconds = 5;
                if (options.DelayBetweenChecksInSeconds > maxSleepTimeInSeconds) {

                    int tries = options.DelayBetweenChecksInSeconds / maxSleepTimeInSeconds;
                    for (int i = 0; i < tries; i++) {
                        if (!this.isRunning) {
                            break;
                        }
                        Thread.Sleep(maxSleepTimeInSeconds * 1000);
                    }
                } else {
                    Thread.Sleep(options.DelayBetweenChecksInSeconds * 1000);
                }
            }
            Logger.Log("thread finished");
        }

        private void CheckServiceCalls()
        {
            // db
            SQLServerContext dbContext = new SQLServerContext();

            // create services
            SharedLibs.Chester.CustomerService customerService = new SharedLibs.Chester.CustomerService(options.ChesterCustomerEndpoint);
            SharedLibs.Migros.Service migrosService = new SharedLibs.Migros.Service(options.MigrosServiceEndpoint);
            SharedLibs.Chester.Security securityService = new SharedLibs.Chester.Security(options.ChesterSecurityEndpoint);

            // login
            if (!securityService.Login(options.ChesterUsername, options.ChesterPassword))
            {
                Logger.LogError("Could not login with username: " + options.ChesterUsername + " and password: ******");
                return;
            }

            Logger.Log("login successful");
            customerService.Cookies = securityService.Cookies;

            int syncCount = 0;

            // get tracked service calls from DB
            List<ServiceCall> fetchedDbCalls = (
                from c in dbContext.ServiceCalls
                where
                    c.IsSuccessful == true && // has a corresponding service call in chester
                    c.HasChild == false &&    // it the latest version of a service call 
                    c.IsCompleted == false && // no need to sync completed ones
                    c.IsCancelled == false && // no need to sync cancelled ones
                    c.IsMissing == false      // has still a corresponding service call in chester
                select c).ToList<ServiceCall>();

            Logger.Log("got " + fetchedDbCalls.Count + " service calls to sync");

            ServiceCallWso[] fetchedChesterCalls;
            ServiceCallWso chesterCall;
            ChesterMigrosStatusMapping mapping;
            String description;
            String newStatus;

            // loop over and check one by one
            foreach (ServiceCall dbCall in fetchedDbCalls) { 
            
                // fetch corresponding service call from chester
                fetchedChesterCalls = new ServiceCallWso[] { };
                try {
                    fetchedChesterCalls = customerService.GetServiceCalls(dbCall.ChesterServiceCallID, "", "", null, "", null, null, null, null);
                    if (fetchedChesterCalls.Length < 1) {
                        // mark as missing
                        dbCall.IsMissing = true;
                        dbContext.SaveChanges();
                        throw new Exception("missing chester service call, id: " + dbCall.ChesterServiceCallID);
                    }
                    chesterCall = fetchedChesterCalls[0];
                }
                catch (Exception e)
                {
                    String message = "could not get service call: " + e.Message; 
                    Logger.LogError(message);
                    EmailHelper.SendEmail(options.ErrorNotificationEmail, serviceName, message);
                    continue;
                }

                // get status mapping from DB
                mapping = dbContext.ChesterMigrosStatusMappings.Where(m => m.ChesterStatusCode == chesterCall.StateID).First();
                if (mapping == null) {
                    String message = "unknown chester state id: " + chesterCall.StateID + " in service call: " + dbCall.ChesterServiceCallID;
                    Logger.LogWarning(message);
                    EmailHelper.SendEmail(options.ErrorNotificationEmail, serviceName, message);
                    continue;
                }
                newStatus = mapping.MigrosStatusName;
                description = mapping.MigrosStatusDescription;

                if (dbCall.CagriDurumKodu == description)
                {

                    Logger.Log("skipping up-to-date call " + dbCall.ServiceCallID + ", dbCall.CagriDurumKodu: " + dbCall.CagriDurumKodu + ", newStatus: " + description);
                    continue;
                }

                try {

                    // update migros service call
                    String response = migrosService.UpdateMgrsCall(
                        options.MigrosUsername, 
                        options.MigrosPassword, 
                        dbCall.MigrosCagriNo, 
                        newStatus, 
                        description
                    );

                    // check if success
                    Regex pattern = new Regex("success", RegexOptions.IgnoreCase);
                    Match match = pattern.Match(response);
                    if (!match.Success) {
                        throw new Exception("UpdateMgrsCall failed, request: ('" + dbCall.MigrosCagriNo + "', '" + newStatus + "', '" + description + "'), response: " + response);
                    }

                    // then, update database
                    dbCall.CagriDurumKodu = description;

                    if (mapping.ChesterStatusCode == 6) // completed
                    { 
                        dbCall.IsCompleted = true;
                    }
                    else if (mapping.ChesterStatusCode == 7) // cancelled
                    { 
                        dbCall.IsCancelled = true;
                    }

                    dbContext.SaveChanges();

                    // all is well
                    syncCount++;

                } catch (Exception e) {

                    Logger.LogError(e.Message);
                    EmailHelper.SendEmail(options.ErrorNotificationEmail, serviceName, e.Message);
                    continue;
                }
            }
            Logger.Log("synced " + syncCount + " records");
        }

        public void OnStopPublic() {
            this.OnStop();
        }

        protected override void OnStop()
        {
            this.isRunning = false;
            Logger.Log("service stopped...");
        }
    }
}
