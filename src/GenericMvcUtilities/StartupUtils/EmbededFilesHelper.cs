using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericMvcUtilities.StartupUtils
{
	public static class EmbededFilesHelper
	{
		public static void ConfigureEmbededFileProviders(IServiceCollection services, params Assembly[] assemblies)
		{
			if (assemblies != null && services != null)
			{
				//Fucking Success
				List<Assembly> componentAssemblies = new List<Assembly>();

				foreach (var assembly in assemblies)
				{
					if (assembly != null)
					{
						componentAssemblies.Add(assembly);
					}
				}

				var embeddedFileProviders = componentAssemblies.Select(a => (IFileProvider)new EmbeddedFileProvider(a, a.GetName().Name)).ToList();

				services.Configure<RazorViewEngineOptions>(options =>
				{
					options.FileProviders.Add(new CompositeFileProvider(embeddedFileProviders));
				});
			}
			else
			{
				throw new ArgumentNullException(nameof(assemblies)+" or "+ nameof(services));
			}
		}

		public static Assembly GetGenericMvcUtils()
		{
			return typeof(GenericMvcUtilities.Models.IModel<int>).GetTypeInfo().Assembly;
		}
	}
}
