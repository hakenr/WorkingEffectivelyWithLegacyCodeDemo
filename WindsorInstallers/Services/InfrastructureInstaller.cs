using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Havit.MigrosChester.Services.Infrastructure;

namespace Havit.MigrosChester.WindsorInstallers.Services
{
	public class InfrastructureInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component
					.For<IMailSender>()
					.ImplementedBy<SmtpMailSender>()
					.LifestyleSingleton());
		}
	}
}