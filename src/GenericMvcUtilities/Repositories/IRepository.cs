using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using GenericMvcUtilities.Models;
using System.Linq.Expressions;

namespace GenericMvcUtilities.Repositories
{
	//todo resolve error handling in insert and update, to use exceptions
	public interface IRepository<T> where T : class
	{
		Task<bool> Any(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<IEnumerable<T>> GetAll();

		Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<T> GetCompleteItem(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<IEnumerable<T>> GetMultiple(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<bool> Insert(T entity);

		Task<bool> Inserts(ICollection<T> entities);

		Task<bool> Update(T entity);

		Task<bool> Delete(T entity);
	}


	public interface IRepository2<TEntity> : IEnumerable<TEntity> where TEntity : class
	{
		Task<bool> Any(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<IEnumerable<TEntity>> GetAll();

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, bool WithNestedData = false);

		Task<ICollection<TEntity>> GetMany(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

		Task<ICollection<TEntity>> GetMany(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, bool WithNestedData = false);

		Task<TEntity> Create(TEntity entity);

		Task<ICollection<TEntity>> CreateRange(ICollection<TEntity> entities);

		Task<TEntity> Update(TEntity entity);

		Task<ICollection<TEntity>> UpdateRange(ICollection<TEntity> entities);

		Task<bool> Delete(TEntity entity);

		Task<bool> DeleteRange(ICollection<TEntity> entities);
	}


	//public interface IBaseRepository

	public interface IEntityFrameworkRepository<T> : IRepository<T>  where T : class 
	{
		Expression<Func<T, bool>> IsMatchedExpression(string propertyName, object propertyValue);

		Expression<Func<T, bool>> MatchByIdExpression(object id);

		DbContext DataContext { get; set; }

		DbSet<T> ContextSet { get; set; }

		Task<int> Save();
	}
}
