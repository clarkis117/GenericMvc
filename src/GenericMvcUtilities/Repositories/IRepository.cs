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
	public interface IRepository<T> where T : class
	{
		Task<bool> Exists(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<IEnumerable<T>> GetAll();

		Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<T> GetCompleteItem(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<ICollection<T>> GetMultiple(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<bool> Insert(T entity);

		Task<bool> Inserts(ICollection<T> entities);

		Task<bool> Update(T entity);

		Task<bool> Delete(T entity);
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
