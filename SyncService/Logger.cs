using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SyncService
{
    class Logger
    {
        public static void Log(String message)
        {
            Log(EventLogEntryType.Information, message);
        }

        public static void LogError(String message)
        {
            Log(EventLogEntryType.Error, message);
        }

        public static void LogWarning(String message)
        {
            Log(EventLogEntryType.Warning, message);
        }

        public static void Log(EventLogEntryType level, String message)
        {
            String levelStr = "INFO";
            switch (level)
            {
                case EventLogEntryType.Warning:
                    levelStr = "WARN";
                    break;
                case EventLogEntryType.Error:
                    levelStr = "ERRR";
                    break;
                default:
                    break;
            }

            // CONSOLE
            Console.WriteLine("[" + levelStr + "] " + message);

            // EVENT LOG
            EventLog.WriteEntry(Service1.serviceName, message, level);
        }
    }
}
