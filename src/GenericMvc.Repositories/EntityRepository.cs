using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	//todo fix number of changes saved can equal zero, this is a valid value
	/// <summary>
	/// Base Repository for accessing the Entity Framework Context
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BaseEntityRepository<T> : IEntityRepository<T>, IDisposable
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

		private static IEnumerable<IEntityType> modelEntityTypes;

		private static bool hasGenericTypeBeenChecked = false;

		private static readonly Type typeofT = typeof(T);

		public Type TypeOfEntity { get { return typeofT; } }

		private static readonly ParameterExpression expressionOfT = Expression.Parameter(typeofT);

		public ParameterExpression EntityExpression { get { return expressionOfT; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseEntityRepository{T}"/> class.
		/// </summary>
		/// <param name="dbContext">The data context.</param>
		/// <exception cref="System.ArgumentNullException">Null Data Context:  + DataContext.ToString()</exception>
		/// <exception cref="System.Exception">BaseRepository Constructor Failed:  + typeof(T).ToString()</exception>
		public BaseEntityRepository(DbContext dbContext)
		{
			try
			{
				if (dbContext == null)
					throw new ArgumentNullException(nameof(dbContext));

				if (modelEntityTypes == null)
					modelEntityTypes = dbContext.Model.GetEntityTypes();

				//check for entity based on one in graph controller
				if (hasGenericTypeBeenChecked || IsTypePresentInDataContext(typeofT, EntityTypes))
				{
					//set data base context
					this.DataContext = dbContext;

					//set the working set
					this.ContextSet = this.DataContext.Set<T>();

					if (!hasGenericTypeBeenChecked)
						hasGenericTypeBeenChecked = true;
				}
				else
				{
					throw new ArgumentException(this.GetType().ToString() + ": Not Member of Current DbContext.");
				}
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		/// <summary>
		/// Checks for the existence using the specified predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public Task<bool> Any(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			var token = new CancellationToken();

			token.ThrowIfCancellationRequested();

			return this.ContextSet.AsNoTracking().AnyAsync(predicate, token);
		}

		/// <summary>
		/// Checks for the existence of the entity using the specified synchronously.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public bool AnySync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return this.ContextSet.AsNoTracking().Any(predicate);
		}

		public Task<long> Count()
		{
			var token = new CancellationToken();

			token.ThrowIfCancellationRequested();

			return ContextSet.AsNoTracking().LongCountAsync(token);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ContextSet.AsEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ContextSet.AsEnumerable().GetEnumerator();
		}

		/// <summary>
		/// Gets all the entities from the table.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get All Failed:  + typeof(T).ToString()</exception>
		public virtual async Task<IEnumerable<T>> GetAll()
		{
			CancellationToken token = new CancellationToken();

			token.ThrowIfCancellationRequested();

			return await ContextSet.AsNoTracking().ToListAsync(token);
		}

		protected virtual IQueryable<T> GetIncludeQuery()
		{
			return this.ContextSet;
		}

		/// <summary>
		/// Gets the specified entity using the predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Failed:  + typeof(T).ToString()</exception>
		public virtual Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			System.Threading.CancellationToken token = new System.Threading.CancellationToken();

			//Throw if query is Canceled
			token.ThrowIfCancellationRequested();

			return ContextSet.AsNoTracking().FirstOrDefaultAsync<T>(predicate, token);
		}

		/// <summary>
		/// Gets the complete entity, this is meant to provide an interface for retrieving entities with multiple complex relationships/
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Failed:  + typeof(T).ToString()</exception>
		public virtual Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
			{
				System.Threading.CancellationToken token = new System.Threading.CancellationToken();

				//Throw if query is Canceled
				token.ThrowIfCancellationRequested();

				return GetIncludeQuery().AsNoTracking().FirstOrDefaultAsync(predicate, token);
			}
			else
			{
				return Get(predicate);
			}
		}

		//todo: also give this a boolean argument for using get or getcomplete
		/// <summary>
		/// Gets multiple entities from the data context using the predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Multi Failed:  + typeof(T).ToString()</exception>
		public async virtual Task<IList<T>> GetMany(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			CancellationToken token = new CancellationToken();

			token.ThrowIfCancellationRequested();

			return (await ContextSet.AsNoTracking().Where(predicate).ToListAsync(token)) as IList<T>;
		}

		public virtual Task<IList<T>> GetMany(Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
			{
				return GetIncludeQuery().AsNoTracking().Where(predicate).ToListAsync() as Task<IList<T>>;
			}
			else
			{
				return GetMany(predicate);
			}
		}

		/// <summary>
		/// Inserts the specified entity into the data context.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Insert Failed:  + typeof(T).ToString()</exception>
		public virtual async Task<T> Create(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			var addedEntity = ContextSet.Add(entity);

			if (await this.DataContext.SaveChangesAsync() >= 0)
			{
				//return true;
				return addedEntity.Entity;
			}
			else
			{
				//return false;
				throw new Exception("Adding item to Database Failed");
			}
		}

		/// <summary>
		/// Inserts the specified entities into the data context.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Inserts Failed:  + typeof(T).ToString()</exception>
		public virtual async Task<IEnumerable<T>> CreateRange(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			ContextSet.AddRange(entities);

			if (await this.DataContext.SaveChangesAsync() >= 0)
			{
				return entities;
			}
			else
			{
				//return false;
				throw new Exception("Adding items to Database failed");
			}
		}

		/// <summary>
		/// Updates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">Null Entity Argument</exception>
		/// <exception cref="System.Exception">Update Failed: + typeof(T).ToString()</exception>
		public virtual async Task<T> Update(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			var updatedEntity = ContextSet.Update(entity);

			if (await this.DataContext.SaveChangesAsync() >= 0)
			{
				return updatedEntity.Entity;
			}
			else
			{
				//return false;
				throw new Exception("Updating item Failed");
			}
		}

		public virtual async Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			ContextSet.UpdateRange(entities);

			if (await this.DataContext.SaveChangesAsync() >= 0)
			{
				return entities;
			}
			else
			{
				//return false;
				throw new Exception("Updating item Failed");
			}
		}

		/// <summary>
		/// Deletes the specified entity, Must be overridden to delete entries with more than one relationship.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Delete Failed:  + typeof(T).ToString()</exception>
		public virtual async Task<bool> Delete(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			ContextSet.Remove(entity);

			if (await DataContext.SaveChangesAsync() >= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public virtual async Task<bool> DeleteRange(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			ContextSet.RemoveRange(entities);

			if (await DataContext.SaveChangesAsync() >= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		//Second make sure the type is present in the data-context
		//One possibility
		//third delete the object
		//Repository.DataContext.
		//forth save changes
		public async Task<bool> DeleteChild(object child)
		{
			if (child == null)
				throw new ArgumentNullException(nameof(child));

			try
			{
				if (IsTypePresentInModel(typeofT, Model))
				{
					DataContext.Remove(child);

					if (await DataContext.SaveChangesAsync() >= 0)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					throw new Exception($"Type of {nameof(child)}:{child.GetType().Name} is not present in model {typeofT.Name}");
				}
			}
			catch (Exception e)
			{
				throw new Exception("Failed to delete child object", e);
			}
		}

		/// <summary>
		/// Saves any changes to the data context.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Save Failed: +ex.Message</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<int> Save()
		{
			return this.DataContext.SaveChangesAsync();
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
				if (!this._disposed)
				{
					if (disposing)
					{
						this.DataContext.Dispose();
					}
				}

				//this.ContextSet = null;

				this._disposed = true;
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
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}