using GenericMvc.Models;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Api
{
	public class Api<T, TKey> : ReadOnlyApi<T, TKey>, IApi<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		public Api(IRepository<T> repository, ILogger<T> logger) : base(repository, logger)
		{
		}

		[NonAction]
		protected virtual Task<bool> IsValid(T Model, ModelStateDictionary ModelState, bool updating = false)
		{
			return Task.FromResult(ModelState.IsValid);
		}

		[NonAction]
		protected virtual Task<bool> IsValid(IEnumerable<T> Models, ModelStateDictionary ModelState, bool updating = false)
		{
			return Task.FromResult(ModelState.IsValid);
		}

		/// <summary>
		/// Creates the specified item from POSTED JSON Representation.
		/// T must implement equals.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>IActionResult</returns>
		/// <exception cref="System.Exception">
		/// Insert Failed
		/// or
		/// </exception>
		[Route("api/[controller]/[action]/"), HttpPost]
		public virtual async Task<IActionResult> Create([FromBody] T item)
		{
			try
			{
				if (item != null && await IsValid(item, ModelState))
				{
					//Attempt to Insert Item
					var createdItem = await Repository.Create(item);

					if (createdItem != null)
					{
						//Send 201 Response
						return CreatedAtAction("create", createdItem);
					}
					else
					{
						//Send 500 Response, and throw so the failure is logged
						throw new Exception("Insert Failed for unknown reasons");
					}
				}
				//Send 400 Response
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				string message = $"Create:{typeOfT.Name} Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}

		/// <summary>
		/// Creates the specified items from POSTED JSON Representation.
		/// T must implement equals
		/// </summary>
		/// <param name="items">The items.</param>
		/// <returns>
		/// IActionResult
		/// </returns>
		/// <exception cref="System.Exception">
		/// Inserting items failed
		/// or
		/// </exception>
		[Route("api/[controller]/[action]/"), HttpPost]
		public virtual async Task<IActionResult> Creates([FromBody] IEnumerable<T> items)
		{
			try
			{
				if (items != null && await IsValid(items, ModelState))
				{
					var createdRange = await Repository.CreateRange(items);

					if (createdRange != null && createdRange.Count() >= 1)
					{
						//Send 201 Response, created response
						return CreatedAtAction("Creates", createdRange);
					}
					else
					{
						//error 500 response
						throw new Exception("Creating multiple items failed for unknown reasons");
					}
				}

				//Send 400 Response
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				string message = $"Create Multiple:{typeOfT.Name} Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}

		//todo: add behavior for returning updated section
		[Route("api/[controller]/[action]/"), HttpPost]
		public virtual async Task<IActionResult> Update(TKey id, [FromBody] T item)
		{
			try
			{
				if (id != null && item != null && await IsValid(item, ModelState, updating: true))
				{
					//Check for item existence, since some things like "consts" use id arg for query
					var exists = await Repository.Any(x => x.Id.Equals(id));

					//If Item Exists Update it
					if (exists)
					{
						var updatedItem = await Repository.Update(item);

						if (updatedItem != null)
						{
							//Send 201 Response if success full
							return Json(updatedItem);
						}
						else
						{
							//Send 500 Response if update fails
							throw new Exception("Update Item Failed");
						}
					}
					else
					{
						//Send 404 Response if Item not Found
						return NotFound(id);
					}
				}

				//send bad request response with model state errors
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				string message = $"Update:{typeOfT.Name} Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}

		//fixed: fix design oversight to have whole object deleted
		[Route("api/[controller]/[action]/"), HttpDelete]
		public virtual async Task<IActionResult> Delete(TKey id)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					//Get Item, this causes EF to begin tracking it
					var item = await Repository.Get(x => x.Id.Equals(id));

					if (item != null)
					{
						//This causes EF to Remove the Item from the Database
						var result = await Repository.Delete(item);

						if (result != false)
						{
							//If success return 201 response
							return new NoContentResult();
						}
						else
						{
							throw new Exception("Deleting Item Failed");
						}
					}
					else
					{
						//Send 404 if object is not in Database
						return NotFound(id);
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string message = $"Delete:{typeOfT.Name} by Id Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}
	}
}