using GenericMvcUtilities.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Test.Lib
{
	public interface IDatabaseTestFixture<TEntity> : IDisposable where TEntity : class, new()
	{
		BaseEntityFrameworkRepository<TEntity> Repository { get; }
	}
}
