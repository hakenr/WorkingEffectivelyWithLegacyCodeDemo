using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.SqlServer;
using System.Diagnostics;

[assembly: OwinStartupAttribute(typeof(WebApp.Startup))]
namespace WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            if (!Debugger.IsAttached) {
                app.UseHangfire(config => {
                    config.UseSqlServerStorage("SQLServerContext");
                    config.UseServer();
                });
            }
        }
    }
}
