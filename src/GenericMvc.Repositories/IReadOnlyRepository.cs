using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	public interface IReadOnlyRepository<TEntity> : IEnumerable<TEntity>
	{
		Type TypeOfEntity { get; }

		ParameterExpression EntityExpression { get; }

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
	}
}
