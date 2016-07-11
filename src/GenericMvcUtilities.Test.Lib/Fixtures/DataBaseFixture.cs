﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Test.Lib.Models;
using GenericMvcUtilities.Test.Lib.Contexts;
using GenericMvcUtilities.Repositories;

namespace GenericMvcUtilities.Test.Lib.Fixtures
{
	public class InMemoryDataBaseFixture<TEntity, TContext> :
		IDatabaseTestFixture<TEntity>, IDisposable where TEntity : class, new()
		where TContext : InMemoryDbContext
	{

		public TContext DbContext { get; set; }

		BaseEntityFrameworkRepository<TEntity> Repo { get; set; }


		public BaseEntityFrameworkRepository<TEntity> Repository
		{
			get
			{
				return Repo;
			}
		}

		public InMemoryDataBaseFixture()
		{
			DbContext = InMemoryDbContext.New<TContext>();

			Repo = new BaseEntityFrameworkRepository<TEntity>(DbContext);
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
