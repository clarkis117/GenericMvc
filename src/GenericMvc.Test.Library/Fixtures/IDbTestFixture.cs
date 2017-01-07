using GenericMvc.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.Lib
{
	public interface IDatabaseTestFixture<TEntity, TRepository> : IDisposable
		where TEntity : class
		where TRepository : IRepository<TEntity>
	{
		TRepository Repository { get; }
	}
}
