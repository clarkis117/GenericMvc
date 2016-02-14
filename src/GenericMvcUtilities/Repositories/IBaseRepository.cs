using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Entity;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.Repositories
{
	public interface IBaseRepository<T> where T : class
	{
		Task<bool> Exists(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<IEnumerable<T>> GetAll();

		Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<T> GetCompleteItem(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<IEnumerable<T>> GetMulti(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

		Task<bool> Insert(T entity);

		Task<bool> Inserts(ICollection<T> entities);

		Task<bool> Update(T entity);

		Task<bool> Delete(T entity);

		Task<int> Save();
	}
}
