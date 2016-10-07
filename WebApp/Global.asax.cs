using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Havit.CastleWindsor.WebForms;
using Havit.MigrosChester.WindsorInstallers;
using WebApp.Models;
using Configuration = WebApp.Migrations.Configuration;

namespace WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
	    private IWindsorContainer container;
		protected void Application_Start()
        {
			container = new WindsorContainer();
	        container.ConfigureForWebApplication();
	        container.Install(FromAssembly.This());

			SetupControllerFactory(container);
			DependencyInjectionWebFormsHelper.SetResolver(container);

			AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SQLServerContext, Configuration>());
			var dbContext = new SQLServerContext();
			dbContext.Database.Initialize(true);
        }

	    protected void Application_End()
	    {
		    container?.Dispose();
	    }

		private static void SetupControllerFactory(IWindsorContainer container)
		{
			var controllerFactory = container.Resolve<IControllerFactory>();

			ControllerBuilder.Current.SetControllerFactory(controllerFactory);
		}
	}
}
