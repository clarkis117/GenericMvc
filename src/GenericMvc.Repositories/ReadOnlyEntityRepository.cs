using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	//todo fix number of changes saved can equal zero, this is a valid value
	/// <summary>
	/// Base Repository for accessing the Entity Framework Context
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlyEntityRepository<T> : IReadOnlyRepository<T>, IDisposable
		where T : class
	{
		/// <summary>
		/// Gets or sets the data context.
		/// </summary>
		/// <value>
		/// The data base context.
		/// </value>
		public DbContext DataContext { get; }

		/// <summary>
		/// The context set, this is the Set used to access the Table this Repository corresponds to
		/// </summary>
		public DbSet<T> ContextSet { get; }

		protected static IEnumerable<IEntityType> modelEntityTypes;

		protected static bool hasGenericTypeBeenChecked = false;

		protected static readonly Type typeofT = typeof(T);

		public Type TypeOfEntity => typeofT;

		private static readonly ParameterExpression expressionOfT = Expression.Parameter(typeofT);

		public ParameterExpression EntityExpression => expressionOfT;

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
		/// </summary>
		/// <param name="dbContext">The data context.</param>
		/// <exception cref="System.ArgumentNullException">Null Data Context:  + DataContext.ToString()</exception>
		/// <exception cref="System.Exception">BaseRepository Constructor Failed:  + typeof(T).ToString()</exception>
		public ReadOnlyEntityRepository(DbContext dbContext)
		{
			try
			{
				DataContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

				if (modelEntityTypes == null)
					modelEntityTypes = dbContext.Model.GetEntityTypes();

				if (hasGenericTypeBeenChecked || IsTypePresentInDataContext(typeofT, EntityTypes))
				{
					if (!hasGenericTypeBeenChecked)
						hasGenericTypeBeenChecked = true;
				}
				else
				{
					throw new ArgumentException(this.GetType().ToString() + ": Not Member of Current DbContext.");
				}

				ContextSet = DataContext.Set<T>();
			}
			catch (Exception ex)
			{
				throw new Exception("BaseRepository Constructor Failed: " + typeofT.Name, ex);
			}
		}

		public IEnumerable<Type> EntityTypes
		{
			get
			{
				foreach (var type in modelEntityTypes)
				{
					yield return type.ClrType;
				}
			}
		}

		public IEntityType Model
		{
			get
			{
				IEntityType typeofModel = null;

				foreach (var type in modelEntityTypes)
				{
					if (type.ClrType == typeofT)
						return typeofModel = type;
				}

				return typeofModel;
			}
		}

		protected static bool IsTypePresentInDataContext(Type typeParam, IEnumerable<Type> entityTypes)
		{
			if (typeParam != null)
			{
				bool IsTypePresent = false;

				foreach (var type in entityTypes)
				{
					if (type == typeParam)
					{
						return (IsTypePresent = true);
					}
				}

				return IsTypePresent;
			}
			else
			{
				throw new ArgumentNullException(nameof(typeParam));
			}
		}

		protected static bool IsTypePresentInModel(Type typeParam, IEntityType entityType)
		{
			bool isPresent = false;

			var navprops = entityType.GetNavigations();

			foreach (var prop in navprops)
			{
				if (prop.GetTargetType().ClrType == typeParam)
					return isPresent = true;
			}

			return isPresent;
		}

		protected static CancellationToken NewCancelTokenThrows
		{
			get
			{
				var token = new CancellationToken();

				token.ThrowIfCancellationRequested();

				return token;
			}
		}

		/// <summary>
		/// Checks for the existence using the specified predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public Task<bool> Any(Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return ContextSet.AsNoTracking().AnyAsync(predicate, NewCancelTokenThrows);
		}

		/// <summary>
		/// Checks for the existence of the entity using the specified synchronously.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public bool AnySync(Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return ContextSet.AsNoTracking().Any(predicate);
		}

		public Task<long> Count() => ContextSet.AsNoTracking().LongCountAsync(NewCancelTokenThrows);

		public IEnumerator<T> GetEnumerator() => ContextSet.AsEnumerable().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ContextSet.AsEnumerable().GetEnumerator();

		/// <summary>
		/// Gets all the entities from the table.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get All Failed:  + typeof(T).ToString()</exception>
		public virtual async Task<IEnumerable<T>> GetAll() => (await ContextSet.AsNoTracking().ToListAsync(NewCancelTokenThrows));

		protected virtual IQueryable<T> GetIncludeQuery() => ContextSet;

		/// <summary>
		/// Gets the specified entity using the predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Failed:  + typeof(T).ToString()</exception>
		public virtual Task<T> Get(Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return ContextSet.AsNoTracking().FirstOrDefaultAsync<T>(predicate, NewCancelTokenThrows);
		}

		/// <summary>
		/// Gets the complete entity, this is meant to provide an interface for retrieving entities with multiple complex relationships/
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Failed:  + typeof(T).ToString()</exception>
		public virtual Task<T> Get(Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
				return GetIncludeQuery().AsNoTracking().FirstOrDefaultAsync(predicate, NewCancelTokenThrows);

			return Get(predicate);
		}

		//todo: also give this a boolean argument for using get or getcomplete
		/// <summary>
		/// Gets multiple entities from the data context using the predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Multi Failed:  + typeof(T).ToString()</exception>
		public async virtual Task<IList<T>> GetMany(Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return (await ContextSet.AsNoTracking().Where(predicate).ToListAsync(NewCancelTokenThrows));
		}

		public virtual Task<IList<T>> GetMany(Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
				return GetIncludeQuery().AsNoTracking().Where(predicate).ToListAsync() as Task<IList<T>>;

			return GetMany(predicate);
		}

		private bool _disposed = false;

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		/// <exception cref="System.Exception">Dispose Failed:  + typeof(T).ToString()</exception>
		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && !_disposed)
				{
					DataContext.Dispose();

					_disposed = true;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Dispose Failed: " + typeofT.Name, ex);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
	}
}