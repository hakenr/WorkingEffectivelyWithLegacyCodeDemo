using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;

using Newtonsoft.Json;
using SharedLibs.Model;
using WebApp.Helpers;

namespace SyncService
{
    class Program
    {
        public static int Main(string[] args)
        {
            Options options = new Options();
            bool success = CommandLine.Parser.Default.ParseArguments(args, options);
            if (!success)
            {
                if (args.Length != 0)
                {
                    return 1;
                }
            }

            try
            {
                if (options.CreateSettings) {
                    createSettingsStub();
                    Console.WriteLine("Settings stub created! check " + Options.getSettingsFile());
                }
                else if (options.Install)
                {
                    Installer.Install(false, args);
                }
                else if (options.Uninstall)
                {
                    Installer.Install(true, args);
                } 
                else if (options.Run || Debugger.IsAttached)
                {
                    Service1 service = new Service1();

                    Logger.Log("Starting...");
                    service.OnStartPublic(args);

                    if (service.isRunning)
                    {
                        Logger.Log("Service running (v1); press any key to stop");
                        Console.ReadKey(true);

                        service.OnStopPublic();
                        Logger.Log("Stopped");
                    }
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[] 
                    { 
                        new Service1() 
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }

        private static void createSettingsStub()
        {
            Options sampleOptions = new Options();

            // migros
            sampleOptions.MigrosUsername = "";
            sampleOptions.MigrosPassword = "";
            sampleOptions.MigrosServiceEndpoint = "";
            
            // chester
            sampleOptions.ChesterUsername = "";
            sampleOptions.ChesterPassword = "";
            sampleOptions.ChesterCallTypeID = 17;
            sampleOptions.ChesterContactPersonID = 17628;
            sampleOptions.ChesterSecurityEndpoint = "";
            sampleOptions.ChesterCustomerEndpoint = "";
            sampleOptions.ChesterCommonEndpoint = "";
            sampleOptions.NotificationNumberCheckDelayInMinutes = 60 * 24;
            
            // misc
            sampleOptions.DelayBetweenChecksInSeconds = 300;
            sampleOptions.DefaultSerialNumber = "";

            String optionsString = JsonConvert.SerializeObject(sampleOptions, Formatting.Indented);

            Directory.CreateDirectory(Options.settingsFolder);
            String settingsFilePath = Options.getSettingsFile();

            TextWriter writer = new StreamWriter(settingsFilePath);
            writer.WriteLine(optionsString);
            writer.Close();
        }
    }
}
