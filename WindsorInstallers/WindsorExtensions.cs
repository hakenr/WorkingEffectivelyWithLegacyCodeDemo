using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Havit.MigrosChester.WindsorInstallers.Plumbing;

namespace Havit.MigrosChester.WindsorInstallers
{
    public static class WindsorExtensions
    {
	    public static void ConfigureForWebApplication(this IWindsorContainer container)
	    {
		    ConfigureForAll(container);

			// place for custom registrations for WebApplication
		    container.Register(
			    Component.For<IControllerFactory>()
				    .ImplementedBy<WindsorControllerFactory>());
	    }

		public static void ConfigureForWindowsService(this IWindsorContainer container)
		{
			ConfigureForAll(container);

			// place for custom registrations for WindowsService
		}

		private static void ConfigureForAll(IWindsorContainer container)
		{
			container.Install(FromAssembly.This());
		}
    }
}
