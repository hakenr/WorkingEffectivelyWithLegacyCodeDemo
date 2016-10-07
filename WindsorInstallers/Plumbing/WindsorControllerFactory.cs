using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Havit.Diagnostics.Contracts;

namespace Havit.MigrosChester.WindsorInstallers.Plumbing
{
	public class WindsorControllerFactory : DefaultControllerFactory
	{
		private readonly IKernel kernel;

		public WindsorControllerFactory(IKernel kernel)
		{
			Contract.Requires<ArgumentNullException>(kernel != null);

			this.kernel = kernel;
		}

		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
		{
			if (controllerType == null)
			{
				throw new HttpException(404, "Not found");
			}

			return kernel.Resolve(controllerType) as IController;
		}

		public override void ReleaseController(IController controller)
		{
			kernel.ReleaseComponent(controller);
		}
	}
}