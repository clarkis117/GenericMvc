using GenericMvcUtilities.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Fixtures
{
	public interface IDbTestFixture<TEntity> : IDisposable where TEntity : class, new()
	{
		DbContext DbContext { get; }

		BaseEntityFrameworkRepository<TEntity> Repository { get; }
	}
}
