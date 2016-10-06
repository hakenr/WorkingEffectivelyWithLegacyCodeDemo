using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

namespace SyncService
{
    [RunInstaller(true)]
    public sealed class MyServiceProcessInstaller : ServiceProcessInstaller
    {
        public MyServiceProcessInstaller()
        {
            this.Account = ServiceAccount.LocalSystem;
        }
    }

    [RunInstaller(true)]
    public sealed class MyServiceInstaller : ServiceInstaller
    {
        public MyServiceInstaller()
        {
            String name = Service1.serviceName;

            this.Description = name;
            this.DisplayName = name;
            this.ServiceName = name;
            this.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }
    }

    public class Installer {

        public static void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Service1).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
