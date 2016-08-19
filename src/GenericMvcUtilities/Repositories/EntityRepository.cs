using GenericMvcUtilities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
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
		public DbContext DataContext { get; set; }

		/// <summary>
		/// The context set, this is the Set used to access the Table this Repository corresponds to
		/// </summary>
		public DbSet<T> ContextSet { get; set; }

		private static readonly Type typeofT = typeof(T);

		private static IEnumerable<IEntityType> entityTypes;

		private static bool hasGenericTypeBeenChecked = false;

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
				if (dbContext != null)
				{
					if (entityTypes == null)
						entityTypes = dbContext.Model.GetEntityTypes();

					//check for entity based on one in graph controller
					if(hasGenericTypeBeenChecked || IsTypePresentInDataContext(typeofT))
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
				else
				{
					throw new ArgumentNullException(nameof(dbContext));
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
				foreach (var type in entityTypes)
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

				foreach (var type in entityTypes)
				{
					if (type.ClrType == type)
						return typeofModel = type;
				}

				return typeofModel;
			}
		}

		protected bool IsTypePresentInDataContext(Type typeParam)
		{
			if (typeParam != null)
			{
				bool IsTypePresent = false;

				foreach (var type in EntityTypes)
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

		protected bool IsTypePresentInModel(Type typeParam, IEntityType entityType)
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
		///
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		/// <returns></returns>
		public Expression<Func<T, bool>> SearchExpression(string propertyName, object propertyValue)
		{
			var typeofString = typeof(string);

			if (propertyValue.GetType() == typeofString)
			{
				//see this SO Answer: http://stackoverflow.com/questions/278684/how-do-i-create-an-expression-tree-to-represent-string-containsterm-in-c

				var parameterExp = Expression.Parameter(typeofT);
				var propertyExp = Expression.Property(parameterExp, propertyName);
				MethodInfo method = typeofString.GetMethod("Contains", new[] { typeofString });
				var someValue = Expression.Constant(propertyValue, typeofString);
				var containsMethodExp = Expression.Call(propertyExp, method, someValue);

				return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
			}
			else
			{
				return IsMatchedExpression(propertyName, propertyValue);
			}
		}

		/// <summary>
		/// Determines whether [is matched expression] [the specified property name].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <returns></returns>
		public Expression<Func<T, bool>> IsMatchedExpression(string propertyName, object propertyValue)
		{
			var parameterExpression = Expression.Parameter(typeofT);
			var propertyOrField = Expression.PropertyOrField(parameterExpression, propertyName);
			var typeConversion = Expression.Convert(propertyOrField, propertyValue.GetType());
			var binaryExpression = Expression.Equal(typeConversion, Expression.Constant(propertyValue));
			return Expression.Lambda<Func<T, bool>>(binaryExpression, parameterExpression);
		}

		/// <summary>
		/// Matches the by identifier expression.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public Expression<Func<T, bool>> MatchByIdExpression(object id)
		{
			var parameterExpression = Expression.Parameter(typeofT);
			var propertyOrField = Expression.PropertyOrField(parameterExpression, "Id");
			var binaryExpression = Expression.Equal(propertyOrField, Expression.Constant(id));
			return Expression.Lambda<Func<T, bool>>(binaryExpression, parameterExpression);
		}

		/*
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
		*/

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

					return this.ContextSet.AsNoTracking().AnyAsync(predicate, token);
				}
				catch (Exception ex)
				{
					throw new Exception("Exists Failed: " + typeofT.Name, ex);
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
				return this.ContextSet.AsNoTracking().Any(predicate);
			}
			catch (Exception ex)
			{
				throw new Exception("Exists Sync Failed: " + typeofT.Name, ex);
			}
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
			try
			{
				CancellationToken token = new CancellationToken();

				token.ThrowIfCancellationRequested();

				return await ContextSet.AsNoTracking().ToListAsync(token);
			}
			catch (Exception ex)
			{
				throw new Exception("Get All Failed: " + typeofT.Name, ex);
			}
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
			if (predicate != null)
			{
				try
				{
					System.Threading.CancellationToken token = new System.Threading.CancellationToken();

					//Throw if query is Canceled
					token.ThrowIfCancellationRequested();

					return ContextSet.AsNoTracking().FirstOrDefaultAsync<T>(predicate, token);
				}
				catch (Exception ex)
				{
					throw new Exception("Get Failed: " + typeofT.Name, ex);
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

						return GetIncludeQuery().AsNoTracking().FirstOrDefaultAsync(predicate, token);
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

					var list = await ContextSet.AsNoTracking().Where(predicate).ToListAsync(token);

					return list;
				}
				catch (Exception ex)
				{
					throw new Exception("Get Multi Failed: " + typeofT.Name, ex);
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
						return GetIncludeQuery().AsNoTracking().Where(predicate).ToListAsync() as Task<IList<T>>;
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
					throw new Exception("Creating Item Failed: " + typeofT.Name, ex);
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
					throw new Exception("Inserts Failed: " + typeofT.Name, ex);
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
					throw new Exception("Update Failed: " + typeofT.Name, ex);
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

					if (await DataContext.SaveChangesAsync() >= 0)
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
					throw new Exception("Delete Failed: " + typeofT.Name, ex);
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

					if (await DataContext.SaveChangesAsync() >= 0)
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
					throw new Exception("Delete Failed: " + typeofT.Name, ex);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
			}
		}

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
					if (IsTypePresentInModel(typeofT, Model))
					{
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
			else
			{
				throw new ArgumentNullException(nameof(child));
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