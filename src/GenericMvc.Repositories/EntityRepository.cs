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
	public class EntityRepository<T> : ReadOnlyEntityRepository<T>, IEntityRepository<T>, IDisposable
		where T : class
	{
		public EntityRepository(DbContext dbContext) : base(dbContext)
		{
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

			if (await DataContext.SaveChangesAsync() >= 0)
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

			if (await DataContext.SaveChangesAsync() >= 0)
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

			if (await DataContext.SaveChangesAsync() >= 0)
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

			if (await DataContext.SaveChangesAsync() >= 0)
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

			return await DataContext.SaveChangesAsync() >= 0;
		}

		public virtual async Task<bool> DeleteRange(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			ContextSet.RemoveRange(entities);

			return await DataContext.SaveChangesAsync() >= 0;
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

					return await DataContext.SaveChangesAsync() >= 0;
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
		public Task<int> Save()
		{
			return DataContext.SaveChangesAsync();
		}
	}
}