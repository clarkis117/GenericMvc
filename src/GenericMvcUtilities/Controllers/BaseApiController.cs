using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using GenericMvcUtilities.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	[Authorize]
	[Route("api/")]
	public class BaseApiController<T> : Controller, IBaseApiController<T> where T : class
	{
		protected readonly BaseRepository<T> Repository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger Logger;

		public BaseApiController(BaseRepository<T> Repo)
		{
			try
			{
				if (Repo != null)
				{
					//Set repo to repo field
					this.Repository = Repo;
					
					var factory = new LoggerFactory();

					//Setup Logging for the Controller
					factory.AddConsole();

					this.Logger = factory.CreateLogger(this.GetType().Name);
				}
				else
				{
					throw new ArgumentNullException("Repository argument is null");
				}
			}
			catch (Exception ex)
			{
				string Message = this.FormatExceptionMessage("Creation of Controller Failed");

				this.Logger.LogCritical(Message, ex);

				throw new Exception(Message, ex);
			}
		}

		protected string FormatLogMessage(string message, Microsoft.AspNet.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		protected string FormatExceptionMessage(string message)
		{
			return (this.GetType().Name + ": " + message + ": " + typeof(T).ToString());
		}

		[AllowAnonymous]
		[Route("[controller]")]
		[HttpGet]
		public virtual async Task<IEnumerable<T>> GetAll()
		{
			try
			{
				return await Repository.GetAll();
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
		[HttpGet("{id:int}")]
		public virtual async Task<T> Get(int? id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						return item;
					}
					else
					{
						//Send Http response for not Found
						HttpContext.Response.StatusCode = 404;

						return null;
					}
				}
				else
				{
					//Send Http Bad Request
					HttpContext.Response.StatusCode = 400;

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
		/// If an Item already exists in the database, it will be removed from the collection.
		/// </summary>
		/// <param name="Items">The items.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">Items is null or empty</exception>
		/// <exception cref="System.Exception"></exception>
		protected virtual Task<ICollection<T>> DifferentalExistance(ICollection<T> Items)
		{
			try
			{
				return Task<ICollection<T>>.Run(() =>
				{
					if (Items != null && Items.Count > 0)
					{
						ConcurrentBag<T> differental = new ConcurrentBag<T>();

						Parallel.ForEach(Items, item =>
						{
							var DoesItExist = this.Repository.Exists(x => x.Equals(item));

							if (!DoesItExist.Result)
							{
								differental.Add(item);
							}
						});

						return differental as ICollection<T>;
					}
					else
					{
						throw new ArgumentNullException("Items is null or empty");
					}
				});
			}
			catch (Exception ex)
			{
				string Message = "Generating differental based on Database Failed";

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
						if (!(await Repository.Exists(x => x.Equals(item))))
						{
							//Attempt to Insert Item
							if ((await Repository.Insert(item)) != false)
							{
								//Send 201 Response
								return CreatedAtAction("create", item);
							}
							else
							{
								//Send 500 Response
								throw new Exception("Insert Failed");
							}
						}
						else
						{
							//Send Success response
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
		[HttpPut("{id:int}")]
		public virtual async Task<IActionResult> Update(int? id, [FromBody] T item)
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
				}

				//Send 400 Response
				return HttpBadRequest();
			}
			catch (Exception ex)
			{
				string Message = "Updating HTTP Put Body Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpDelete("{id:int}")]
		public virtual async Task<IActionResult> Delete(int? id)
		{
			try
			{
				if (id != null)
				{
					//Get Item, this causes EF to begin tracking it
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

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
				string Message = "Deleting Item Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}
	}
}