using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommandLine;
using CommandLine.Text;

using Newtonsoft.Json;

namespace SharedLibs.Model
{
    public class Options
    {
        
        public static String settingsFolder = @"C:\.chestermigros";

        public static String getSettingsFile() {
            return settingsFolder + @"\settings.json";
        }

        [Option("Install", DefaultValue = false)]
        public bool Install { get; set; }

        [Option("Uninstall", DefaultValue = false)]
        public bool Uninstall { get; set; }

        [Option("Run", DefaultValue = false)]
        public bool Run { get; set; }

        [Option("CreateSettings", DefaultValue = false)]
        public bool CreateSettings { get; set; }

        [Option("MigrosUsername", DefaultValue = @"")]
        public String MigrosUsername { get; set; }

        [Option("MigrosPassword", DefaultValue = @"")]
        public String MigrosPassword { get; set; }

        [Option("ChesterUsername", DefaultValue = @"")]
        public String ChesterUsername { get; set; }

        [Option("ChesterPassword", DefaultValue = @"")]
        public String ChesterPassword { get; set; }

        [Option("DelayBetweenChecksInSeconds", DefaultValue = 300)]
        public int DelayBetweenChecksInSeconds { get; set; }

        [Option("ChesterContactPersonID", DefaultValue = 17628)]
        public int ChesterContactPersonID { get; set; }

        [Option("ChesterCallTypeID", DefaultValue = 17)]
        public int ChesterCallTypeID { get; set; }

        [Option("ChesterSecurityEndpoint", DefaultValue = @"")]
        public String ChesterSecurityEndpoint { get; set; }

        [Option("ChesterCustomerEndpoint", DefaultValue = @"")]
        public String ChesterCustomerEndpoint { get; set; }

        [Option("ChesterCommonEndpoint", DefaultValue = @"")]
        public String ChesterCommonEndpoint { get; set; }

        [Option("MigrosServiceEndpoint", DefaultValue = @"")]
        public String MigrosServiceEndpoint { get; set; }

        [Option("DefaultSerialNumber", DefaultValue = @"")]
        public String DefaultSerialNumber { get; set; }

        [Option("UpdateNotificationEmail", DefaultValue = @"")]
        public String UpdateNotificationEmail { get; set; }

        [Option("ErrorNotificationEmail", DefaultValue = @"")]
        public String ErrorNotificationEmail { get; set; }

        [Option("NotificationNumberCheckDelayInMinutes", DefaultValue = 1440)]
        public int NotificationNumberCheckDelayInMinutes { get; set; }

        [HelpOption("help", HelpText = "Shows this help text")]
        public string GetUsage()
        {
            HelpText helpText = new HelpText();
            helpText.AddDashesToOption = true;
            helpText.AdditionalNewLineAfterOption = true;
            helpText.AddOptions(this);
            return helpText;
        }

        public String toJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
