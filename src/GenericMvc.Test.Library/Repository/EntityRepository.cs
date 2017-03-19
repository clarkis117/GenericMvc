using GenericMvc.Models;
using GenericMvc.Repositories;
using GenericMvc.Test.Lib.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GenericMvc.Test.Lib.Repository
{
	public abstract class EntityRepository<TEntity, TKey, TRepository, TContext, TFixture> : IRepositoryTests<TEntity, TKey, TRepository, TContext, TFixture>
		where TRepository : EntityRepository<TEntity>
		where TEntity : class, IModel<TKey>, new()
		where TContext : DbContext
		where TFixture : DataBaseFixture<TContext>, new()
		where TKey : IEquatable<TKey>
	{

		[Fact]
		public void GetDataContext()
		{
			var context = Repo.DataContext;

			Assert.NotNull(context);
		}

		[Fact]
		public void ContextSet()
		{
			var contextSet = Repo.ContextSet;

			Assert.NotNull(contextSet);
		}

		[Fact]
		public void GetEntityTypes()
		{
			var types = Repo.DataContext.Model.GetEntityTypes();

			Assert.NotNull(types);

			Assert.NotEmpty(types);
		}
	}
}
