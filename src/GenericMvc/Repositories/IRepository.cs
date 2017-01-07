using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using GenericMvc.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GenericMvc.Repositories
{
	//todo resolve error handling in insert and update, to use exceptions
	/*
	public interface IRepository<TEntity> where TEntity : class
	{
		Task<bool> Any(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<IEnumerable<TEntity>> GetAll();

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, bool WithNestedDate = false);

		Task<IEnumerable<TEntity>> GetMany(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<bool> Create(TEntity entity);

		Task<bool> Creates(ICollection<TEntity> entities);

		Task<bool> Update(TEntity entity);

		Task<bool> Delete(TEntity entity);
	}
	*/

	public interface IRepository<TEntity> : IEnumerable<TEntity>
	{
		Type TypeOfEntity { get; }

		ParameterExpression EntityExpression { get;  }

		Task<bool> Any(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<long> Count();

		//enumerable because entire store is not loaded
		Task<IEnumerable<TEntity>> GetAll();

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, bool WithNestedData = false);

		//list because loaded into memory
		Task<IList<TEntity>> GetMany(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		//list because loaded into memory
		Task<IList<TEntity>> GetMany(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, bool WithNestedData = false);

		Task<TEntity> Create(TEntity entity);

		Task<IEnumerable<TEntity>> CreateRange(IEnumerable<TEntity> entities);

		Task<TEntity> Update(TEntity entity);

		Task<IEnumerable<TEntity>> UpdateRange(IEnumerable<TEntity> entities);

		Task<bool> Delete(TEntity entity);

		Task<bool> DeleteRange(IEnumerable<TEntity> entities);
	}


	public interface IGraphRepository<T> : IRepository<T> where T : class
	{
		IEnumerable<Type> EntityTypes { get; }

		Task<bool> DeleteChild(object child);
	}

	//public interface IBaseRepository

	public interface IEntityRepository<T> : IGraphRepository<T> where T : class
	{
		//Expression<Func<T, bool>> IsMatchedExpression(string propertyName, object propertyValue);

		//Expression<Func<T, bool>> SearchExpression(string propertyName, object propertyValue);

		//Expression<Func<T, bool>> MatchByIdExpression(object id);

		DbContext DataContext { get; }

		DbSet<T> ContextSet { get; }

		Task<int> Save();
	}
}
