using GenericMvcUtilities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

//ToDo: Add Eager Loading to this and IBaseRepository
//Todo: reconcile this with changes in RC2
//Todo: add an additional generic type constraint of TKey
namespace GenericMvcUtilities.Repositories
{
	public class InjectEntityRepo<T, TContext> : BaseEntityRepository<T>
		where T : class
		where TContext : DbContext
	{
		public InjectEntityRepo(TContext context) : base(context)
		{

		}
	}

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
		public DbContext DataContext { get; set; }

		/// <summary>
		/// The context set, this is the Set used to access the Table this Repository corresponds to
		/// </summary>
		public DbSet<T> ContextSet { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseEntityRepository{T}"/> class.
		/// </summary>
		/// <param name="dataContext">The data context.</param>
		/// <exception cref="System.ArgumentNullException">Null Data Context:  + DataContext.ToString()</exception>
		/// <exception cref="System.Exception">BaseRepository Constructor Failed:  + typeof(T).ToString()</exception>
		public BaseEntityRepository(DbContext dataContext)
		{
			try
			{
				if (dataContext != null)
				{
					//todo add check for entity based on one in graph controller

					//dataContext.Model.GetEntityTypes().
					/*
					if(IsTypePresentInDataContext(ApplicationDbContext.TypeList))
					{
						dataContext = DataContext;

						this.ContextSet = dataContext.Set<T>();
					}
					else
					{
						throw new ArgumentException(this.GetType().ToString() + ": Not Member of Current DbContext.");
					}
					*/

					//set data base context
					this.DataContext = dataContext;

					//set the working set
					this.ContextSet = this.DataContext.Set<T>();
				}
				else
				{
					throw new ArgumentNullException(nameof(dataContext));
				}
			}
			catch (Exception ex)
			{
				throw new Exception("BaseRepository Constructor Failed: " + typeof(T).Name, ex);
			}
		}

		/*
		private bool IsTypePresentInDataContext(List<Type> TypeList)
		{
			try
			{
				bool IsTypePresent = false;

				Type param = typeof(T);

				if (TypeList != null)
				{
					foreach (var type in TypeList)
					{
						if (type == param)
						{
							return (IsTypePresent = true);
						}
					}
				}
				else
				{
					throw new ArgumentNullException(TypeList.GetType().ToString());
				}

				return IsTypePresent;
			}
			catch(Exception ex)
			{
				throw new Exception("IsTypePresentInDataContext Failed," + typeof(T).ToString() + " not present in DBContext", ex);
			}
		}
		*/

		/// <summary>
		/// Determines whether [is matched expression] [the specified property name].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <returns></returns>
		public Expression<Func<T, bool>> IsMatchedExpression(string propertyName, object propertyValue)
		{
			var parameterExpression = Expression.Parameter(typeof(T));
			var propertyOrField = Expression.PropertyOrField(parameterExpression, propertyName);
			var binaryExpression = Expression.Equal(propertyOrField, Expression.Constant(propertyValue));
			return Expression.Lambda<Func<T, bool>>(binaryExpression, parameterExpression);
		}

		/// <summary>
		/// Matches the by identifier expression.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public Expression<Func<T, bool>> MatchByIdExpression(object id)
		{
			var parameterExpression = Expression.Parameter(typeof(T));
			var propertyOrField = Expression.PropertyOrField(parameterExpression, "Id");
			var binaryExpression = Expression.Equal(propertyOrField, Expression.Constant(id));
			return Expression.Lambda<Func<T, bool>>(binaryExpression, parameterExpression);
		}

		/// <summary>
		/// Matches the by identifier expression.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public Expression<Func<IModel<TKey>, bool>> MatchByIdExpression<TKey>(object id) where TKey : IEquatable<TKey>
		{
			var parameterExpression = Expression.Parameter(typeof(IModel<TKey>));
			var propertyOrField = Expression.PropertyOrField(parameterExpression, "Id");
			var binaryExpression = Expression.Equal(propertyOrField, Expression.Constant(id));
			return Expression.Lambda<Func<IModel<TKey>, bool>>(binaryExpression, parameterExpression);
		}

		/// <summary>
		/// Checks for the existence using the specified predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public virtual Task<bool> Any(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				try
				{
					var token = new CancellationToken();

					token.ThrowIfCancellationRequested();

					return this.ContextSet.AnyAsync(predicate, token);
				}
				catch (Exception ex)
				{
					throw new Exception("Exists Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		/// <summary>
		/// Checks for the existence of the entity using the specified synchronously.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		public virtual bool AnySync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			try
			{
				return this.ContextSet.Any(predicate);
			}
			catch (Exception ex)
			{
				throw new Exception("Exists Sync Failed: " + typeof(T).Name, ex);
			}
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
			try
			{
				CancellationToken token = new CancellationToken();

				token.ThrowIfCancellationRequested();

				return await ContextSet.ToListAsync(token);
			}
			catch (Exception ex)
			{
				throw new Exception("Get All Failed: " + typeof(T).Name, ex);
			}
		}

		/*
		protected virtual Task<T> GetCompleteItem(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				throw new NotImplementedException();
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}
		*/

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
			if (predicate != null)
			{
				try
				{
					System.Threading.CancellationToken token = new System.Threading.CancellationToken();

					//Throw if query is Canceled
					token.ThrowIfCancellationRequested();

					return ContextSet.FirstOrDefaultAsync<T>(predicate, token);
				}
				catch (Exception ex)
				{
					throw new Exception("Get Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		/// <summary>
		/// Gets the complete entity, this is meant to provide an interface for retrieving entities with multiple complex relationships/
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Get Failed:  + typeof(T).ToString()</exception>
		public virtual Task<T> Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate != null)
			{
				if (WithNestedData)
				{
					try
					{
						System.Threading.CancellationToken token = new System.Threading.CancellationToken();

						//Throw if query is Canceled
						token.ThrowIfCancellationRequested();

						return GetIncludeQuery().FirstOrDefaultAsync(predicate, token);
					}
					catch (Exception e)
					{
						throw new Exception("Get with Nested Data Failed", e);
					}
				}
				else
				{
					return Get(predicate);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
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
			if (predicate != null)
			{
				try
				{
					CancellationToken token = new CancellationToken();

					token.ThrowIfCancellationRequested();

					var list = await ContextSet.Where(predicate).ToListAsync(token);
						
					return list;
				}
				catch (Exception ex)
				{
					throw new Exception("Get Multi Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		public Task<IList<T>> GetMany(Expression<Func<T, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate != null)
			{
				if (WithNestedData)
				{
					try
					{
						return GetIncludeQuery().Where(predicate).ToListAsync() as Task<IList<T>>;
					}
					catch (Exception e)
					{
						throw new Exception("Get Many with Nested Data Failed", e);
					}
				}
				else
				{
					return GetMany(predicate);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
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
			if (entity != null)
			{
				try
				{
					var addedEntity = ContextSet.Add(entity);

					if (await this.DataContext.SaveChangesAsync() > 0)
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
				catch (Exception ex)
				{
					throw new Exception("Creating Item Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
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
			if (entities != null)
			{
				try
				{
					ContextSet.AddRange(entities);

					if (await this.DataContext.SaveChangesAsync() > 0)
					{
						return entities;
					}
					else
					{
						//return false;
						throw new Exception("Adding items to Database failed");
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Inserts Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
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
			if (entity != null)
			{
				try
				{
					//todo is this needed
					/*
					if (this.DataContext.Entry(entity).State == EntityState.Detached)
					{
						ContextSet.Attach(entity);
					}
					*/

					var updatedEntity = ContextSet.Update(entity);

					//dataContext.Entry(entity).State = EntityState.Modified;

					if (await this.DataContext.SaveChangesAsync() > 0)
					{
						return updatedEntity.Entity;
					}
					else
					{
						//return false;
						throw new Exception("Updating item Failed");
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Update Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		public virtual async Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities)
		{
			if (entities != null)
			{
				try
				{
					ContextSet.UpdateRange(entities);

					if (await this.DataContext.SaveChangesAsync() > 0)
					{
						return entities;
					}
					else
					{
						//return false;
						throw new Exception("Updating item Failed");
					}
				}
				catch (Exception e)
				{
					throw new Exception("Updating Range of Items Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
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
			if (entity != null)
			{
				try
				{
					//todo is this needed
					/*
					if (this.DataContext.Entry(entity).State == EntityState.Detached)
					{
						this.ContextSet.Attach(entity);
					}
					*/

					ContextSet.Remove(entity);

					if (await DataContext.SaveChangesAsync() > 0)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Delete Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		public virtual async Task<bool> DeleteRange(IEnumerable<T> entities)
		{
			if (entities != null)
			{
				try
				{
					//todo is this needed
					/*
					if (this.DataContext.Entry(entity).State == EntityState.Detached)
					{
						this.ContextSet.Attach(entity);
					}
					*/

					ContextSet.RemoveRange(entities);

					if (await DataContext.SaveChangesAsync() > 0)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Delete Failed: " + typeof(T).Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
			}
		}

		/// <summary>
		/// Saves any changes to the data context.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Save Failed: +ex.Message</exception>
		public Task<int> Save()
		{
			try
			{
				return this.DataContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new Exception("Save Failed: " + ex.Message, ex);
			}
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

				this.ContextSet = null;

				this._disposed = true;
			}
			catch (Exception ex)
			{
				throw new Exception("Dispose Failed: " + typeof(T).Name, ex);
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

		private IEnumerable<IEntityType> _entityTypes;

		public IEnumerable<Type> EntityTypes
		{
			get
			{
				if (_entityTypes == null)
				{
					_entityTypes = DataContext.Model.GetEntityTypes();
				}

				foreach (var type in _entityTypes)
				{
					yield return type.ClrType;
				}
			}
		}

		//todo: maybe cache entity types in a field?
		//todo: check type T in controller constructor as well
		//todo: check type T in repository constructor as well
		//Second make sure the type is present in the data-context
		//One possibility
		//third delete the object
		//Repository.DataContext.
		//forth save changes
		public async Task<bool> DeleteChild(object child)
		{
			if (child != null)
			{
				try
				{
					//EntityTypes.Any(x => x.ClrType == child.GetType())
					if (DataContext.Entry(child).State == EntityState.Detached)
					{
						DataContext.Attach(child);
					}

					DataContext.Remove(child);

					if (await DataContext.SaveChangesAsync() > 0)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception e)
				{
					throw new Exception("Failed to delete child object", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(child));
			}
		}
	}
}