using GenericMvc.Models;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Controllers
{
	public class BaseApiController<T, TKey> : Controller, IBaseApiController<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly IRepository<T> Repository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<T> Logger;

		protected static readonly Type typeOfT = typeof(T);

		public BaseApiController(IRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository == null)
					throw new ArgumentNullException(nameof(repository));

				if (logger == null)
					throw new ArgumentNullException(nameof(logger));

				this.Repository = repository;

				this.Logger = logger;
			}
			catch (Exception ex)
			{
				string message = FormatExceptionMessage(this, "Creation of Controller Failed");

				this.Logger.LogCritical(message, ex);

				throw new Exception(message, ex);
			}
		}

		[NonAction]
		protected static string FormatLogMessage(string message, Microsoft.AspNetCore.Http.HttpRequest request)
		{
			return message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString();
		}

		[NonAction]
		protected static string FormatExceptionMessage(Controller controller, string message)
		{
			return controller.GetType().Name + ": " + message + ": " + typeOfT.Name;
		}

		[NonAction]
		public virtual Task<bool> IsValid(T Model, ModelStateDictionary ModelState, bool updating = false)
		{
			return Task.FromResult(ModelState.IsValid);
		}

		[NonAction]
		public virtual Task<bool> IsValid(T[] Models, ModelStateDictionary ModelState, bool updating = false)
		{
			return Task.FromResult(ModelState.IsValid);
		}

		/*
		[NonAction]
		protected virtual async Task<ICollection<T>> DifferentalExistance(ICollection<T> items)
		{
			try
			{
				if (items != null && items.Count > 0)
				{
					ICollection<T> differental = new List<T>();

					//todo: fix this, test fix
					foreach (var item in items)
					{
						var doesItExist = await this.Repository.Any(x => x.Id.Equals(item.Id));
						//var doesItExist = await this.Repository.ContextSet.AnyAsync(x => x.Id.Equals(item.Id));

						if (!doesItExist)
						{
							differental.Add(item);
						}
					}

					return differental;
				}
				else
				{
					throw new ArgumentNullException(nameof(items));
				}
			}
			catch (Exception ex)
			{
				string Message = "Generating differential based on Database Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}
		*/

		[Route("api/[controller]/[action]/"), HttpGet]
		public virtual async Task<IEnumerable<T>> GetAll()
		{
			try
			{
				return await Repository.GetAll();
			}
			catch (Exception ex)
			{
				string Message = "Get All Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("api/[controller]/[action]/"), HttpGet("{id}")]
		public virtual async Task<IActionResult> Get(TKey id)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					if (await Repository.Any(x => x.Id.Equals(id)))
					{
						var item = await Repository.Get(x => x.Id.Equals(id), WithNestedData: true);

						if (item != null)
						{
							return Json(item);
						}
					}

					return NotFound(id);
				}
				else
				{
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string Message = "Get by Id Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("api/[controller]/[action]/"), HttpGet]
		public virtual async Task<IActionResult> GetMany(string propertyName, string value)
		{
			if (propertyName == null || value == null || !ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				//todo work this out somehow

				var item = await Repository.GetMany(this.Repository.IsMatchedExpression<T>(propertyName, value), WithNestedData: false);

				if (item != null)
				{
					return Json(item);
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				string Message = "Get by Id Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
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
		//[AllowAnonymous] //Testing only
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
				string Message = "Create from HTTP Post Body Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
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
		public virtual async Task<IActionResult> Creates([FromBody] T[] items)
		{
			try
			{
				if (items != null && items.Length >= 1 && await IsValid(items, ModelState))
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
				string Message = "Creates from HTTP Post Body Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		//todo: add behavior for returning updated section
		[Route("api/[controller]/[action]/"), HttpPost("{id}")]
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
							return new JsonResult(updatedItem);
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
				string Message = "Update - HTTP Put Request Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request));

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		//fixed: fix design oversight to have whole object deleted
		[Route("api/[controller]/[action]/"), HttpDelete("{id}")]
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
				string message = "Deleting Item Failed";

				this.Logger.LogError(FormatLogMessage(message, this.Request));

				throw new Exception(FormatExceptionMessage(this, message), ex);
			}
		}
	}
}