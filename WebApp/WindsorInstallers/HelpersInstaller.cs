using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using WebApp.Helpers;

namespace WebApp.WindsorInstallers
{
	public class HelpersInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IEmailHelper>().ImplementedBy<EmailHelper>().LifestyleTransient());
		}
	}
}