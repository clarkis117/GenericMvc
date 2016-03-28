using GenericMvcUtilities.Repositories;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using GenericMvcUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data;
using System.Net;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	[Route("api/")]
	public class BaseApiController<T, TKey> : Controller, IBaseApiController<T, TKey>
		where T : class, IModel<TKey> 
		where TKey : IEquatable<TKey>
	{
		protected readonly BaseRepository<T> Repository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<T> Logger;

		public BaseApiController(BaseRepository<T> repository, ILogger<T> logger)
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
		protected string FormatLogMessage(string message, Microsoft.AspNet.Http.HttpRequest request)
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
						var doesItExist = await this.Repository.ContextSet.AnyAsync(x => x.Id.Equals(item.Id));

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
				return await Repository.ContextSet.ToListAsync();
			}
			catch (Exception ex)
			{
				string Message = "Get All Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[AllowAnonymous]
		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public virtual async Task<T> Get(TKey id)
		{
			try
			{
				if (id != null)
				{
					if (await Repository.Exists(Repository.MatchByIdExpression(id)))
					{
						var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

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
						if (!(await Repository.Exists(Repository.MatchByIdExpression(item.Id))))
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
							return new HttpStatusCodeResult((int)HttpStatusCode.Conflict);
						}
					}
					else
					{
						//Send 400 Response
						return HttpBadRequest();
					}
				}
				else
				{
					//Send 400 Response
					return HttpBadRequest();
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
						return HttpBadRequest();
					}
				}
				else
				{
					//Send 400 Response
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Creates from HTTP Post Body Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpPut("{id}")]
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
						var exists = await Repository.Exists(Repository.MatchByIdExpression(id));

						//If Item Exists Update it
						if (exists == true)
						{
							if ((await Repository.Update((item)) != false))
							{
								//Send 201 Response if success full
								return new NoContentResult();
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
							return HttpNotFound();
						}
					}
					else
					{
						//send bad request response with model state errors
						return HttpBadRequest(ModelState);
					}
				}
				else
				{
					//Send 400 Response
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Update - HTTP Put Request Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: finish
		//todo: add unit test
		[Route("[controller]/[action]/")]
		[HttpDelete]
		public async Task<IActionResult> DeleteChild([FromBody] object child)
		{
			try
			{
				if (child != null)
				{
					if (ModelState.IsValid)
					{
						//first make sure type isn't the root of the object graph, in this case type T

						//todo: maybe cache entity types in a field?
						//todo: check type T in controller constructor as well
						//todo: check type T in repository constructor as well
						//Second make sure the type is present in the data-context 
						//One possiblity
						var a = Repository.DataContext.Model.GetEntityTypes().First().ClrType;


						//third delete the object
						//Repository.DataContext.
						
						//forth save changes
						
					}
				}

				return HttpBadRequest(ModelState);
			}
			catch (Exception ex)
			{

				throw;
			}
		}
		

		//todo: fix design oversight to have whole object deleted
		[Route("[controller]/[action]/")]
		[HttpDelete("{id}")]
		public virtual async Task<IActionResult> Delete(TKey id)
		{
			try
			{
				if (id != null)
				{
					//Get Item, this causes EF to begin tracking it
					var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

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
						return HttpNotFound();
					}
				}
				else
				{
					//Send 400 Response
					return HttpBadRequest();
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