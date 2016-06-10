using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	[Route("api/")]
	public class BaseApiController<T, TKey> : Controller, IBaseApiController<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly IRepository<T> Repository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<T> Logger;

		public BaseApiController(BaseEntityFrameworkRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository != null)
				{
					//Set DI injected repository to repository field
					this.Repository = repository;

					if (logger != null)
					{
						this.Logger = logger;
					}
					else
					{
						throw new ArgumentNullException(nameof(logger));
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(repository));
				}
			}
			catch (Exception ex)
			{
				string message = this.FormatExceptionMessage("Creation of Controller Failed");

				this.Logger.LogCritical(message, ex);

				throw new Exception(message, ex);
			}
		}

		[NonAction]
		protected string FormatLogMessage(string message, Microsoft.AspNetCore.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		//Todo: revamp this hardcore
		[NonAction]
		protected string FormatExceptionMessage(string message)
		{
			return (this.GetType().Name + ": " + message + ": " + typeof(T));
		}

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
						var doesItExist = await this.Repository.Exists(x => x.Id.Equals(item.Id));
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

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[AllowAnonymous]
		[Route("[controller]/[action]/")]
		[HttpGet]
		public virtual async Task<IEnumerable<T>> GetAll()
		{
			try
			{
				//return await Repository.ContextSet.ToListAsync();
				return await Repository.GetAll();
			}
			catch (Exception ex)
			{
				string Message = "Get All Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo change back to mvc style
		[AllowAnonymous]
		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public virtual async Task<T> Get(TKey id)
		{
			try
			{
				if (id != null)
				{
					if (await Repository.Exists(x => x.Id.Equals(id)))
					{
						var item = await Repository.GetCompleteItem(x => x.Id.Equals(id));

						if (item != null)
						{
							return item;
						}
						else
						{
							//Send Http response for not Found
							HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

							return null;
						}
					}
					else
					{
						//Send Http response for not Found
						HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

						return null;
					}
				}
				else
				{
					//Send Http Bad Request
					HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

					return null;
				}
			}
			catch (Exception ex)
			{
				string Message = "Get by Id Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
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
		[Route("[controller]/[action]/")]
		[HttpPost]
		public virtual async Task<IActionResult> Create([FromBody] T item)
		{
			try
			{
				if (item != null)
				{
					if (ModelState.IsValid)
					{
						if (!(await Repository.Exists(x => x.Id.Equals(item.Id))))
						{
							//Attempt to Insert Item
							if ((await Repository.Insert(item)) != false)
							{
								//Send 201 Response
								return CreatedAtAction("create", item);
							}
							else
							{
								//Send 500 Response, and throw so the failure is logged
								throw new Exception("Insert Failed for unknown reasons");
							}
						}
						else
						{
							//Send conflict response
							return new StatusCodeResult((int)HttpStatusCode.Conflict);
						}
					}
					else
					{
						//Send 400 Response
						return BadRequest(ModelState);
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest("Request Value is Null");
				}
			}
			catch (Exception ex)
			{
				string Message = "Create from HTTP Post Body Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
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
		//[AllowAnonymous] //testing only
		[Route("[controller]/[action]/")]
		[HttpPost]
		public virtual async Task<IActionResult> Creates([FromBody] T[] items)
		{
			try
			{
				if (items != null)
				{
					if (ModelState.IsValid)
					{
						ICollection<T> differental = await this.DifferentalExistance(items);

						if (differental != null && differental.Count > 0)
						{
							await Repository.Inserts(differental);

							//Repository.Save().Wait();
							//Send 201 Response
							return CreatedAtAction("creates", differental);
						}
						else
						{
							//Send ~200 Response
							return new NoContentResult();
						}
					}
					else
					{
						//Send 400 Response
						return BadRequest(ModelState);
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest("null input value");
				}
			}
			catch (Exception ex)
			{
				string Message = "Creates from HTTP Post Body Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: add behavior for returning updated section
		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		public virtual async Task<IActionResult> Update(TKey id, [FromBody] T item)
		{
			try
			{
				if (id != null && item != null)
				{
					//Validate Model
					if (ModelState.IsValid)
					{
						//Check for item existence
						var exists = await Repository.Exists(x => x.Id.Equals(item.Id));

						//If Item Exists Update it
						if (exists == true)
						{
							if ((await Repository.Update((item)) != false))
							{
								//Send 201 Response if success full
								return new JsonResult(item);
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
							return NotFound();
						}
					}
					else
					{
						//send bad request response with model state errors
						return BadRequest(ModelState);
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest("Request Value is Null");
				}
			}
			catch (Exception ex)
			{
				string Message = "Update - HTTP Put Request Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}


		//fixed: fix design oversight to have whole object deleted
		[Route("[controller]/[action]/")]
		[HttpDelete("{id}")]
		public virtual async Task<IActionResult> Delete(TKey id)
		{
			try
			{
				if (id != null)
				{
					//Get Item, this causes EF to begin tracking it
					var item = await Repository.GetCompleteItem(x => x.Id.Equals(id));

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
						return NotFound();
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest("Object Identifier is Null");
				}
			}
			catch (Exception ex)
			{
				string message = "Deleting Item Failed";

				this.Logger.LogError(this.FormatLogMessage(message, this.Request));

				throw new Exception(this.FormatExceptionMessage(message), ex);
			}
		}
	}
}