
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GenericMvc.Test.App.Repositories;
using GenericMvc.Repositories;
using GenericMvc.Test.App.Models;
using Microsoft.EntityFrameworkCore;
using GenericMvc.StartupUtils;
using GenericMvc.Controllers;

namespace GenericMvc.Test.App
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			//services.AddEntityFrameworkInMemoryDatabase();
			services.AddDbContext<PersonDbContext>(opt => opt.UseInMemoryDatabase());
			services.AddTransient<IEntityRepository<Person>, PersonRepository>();

			//Add embedded file handlers
			EmbededFilesHelper.ConfigureEmbededFileProviders(services, EmbededFilesHelper.GetGenericMvcUtils());

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			var context = app.ApplicationServices.GetService<PersonDbContext>();

			context.People.AddRange(new Person { Name = "Blue" }, new Person { Name = "Nick" });

			context.SaveChanges();

			BasicModelsController.Initialize(typeof(Person));

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
