using GenericMvcUtilitiesTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilitiesTests.Models;

namespace GenericMvcUtilitiesTests.Fixtures
{
	public class InMemoryDataBaseFixture : IDbTestFixture<Blog>, IDisposable
	{
		private static DbContextOptions<InMemDbContext> CreateNewContextOptions()
		{
			// Create a fresh service provider, and therefore a fresh 
			// InMemory database instance.
			var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();

			// Create a new options instance telling the context to use an
			// InMemory database and the new service provider.
			var builder = new DbContextOptionsBuilder<InMemDbContext>();
			builder.UseInMemoryDatabase()
				   .UseInternalServiceProvider(serviceProvider);

			return builder.Options;
		}

		public InMemDbContext DbContext { get; set; }

		public BlogRepo BlogRepo { get; set; }

		DbContext IDbTestFixture<Blog>.DbContext
		{
			get
			{
				return DbContext;
			}
		}

		public BaseEntityFrameworkRepository<Blog> Repository
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public InMemoryDataBaseFixture()
		{
			DbContext = new InMemDbContext(CreateNewContextOptions());

			BlogRepo = new BlogRepo(DbContext);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					DbContext.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~DataBaseFixture() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

	}
}
