using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.UserManager;
using GenericMvcUtilities.ViewModels.Generic;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	[Authorize(Roles = RoleHelper.SystemOwner + "," + RoleHelper.UserAdmin + "," + RoleHelper.ContentAdmin)]
	public class BaseController<TKey, T> : Controller, IBaseController<TKey, T>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly BaseRepository<T> Repository;

		protected readonly ILogger<T> Logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseController{T}" /> class.
		/// </summary>
		/// <param name="repository">The repo.</param>
		public BaseController(BaseRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository != null && logger != null)
				{
					//Set repo to repo field
					this.Repository = repository;

					this.Logger = logger;
				}
				else
				{
					throw new ArgumentNullException("Repository or Logger argument is null");
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

		[NonAction]
		protected string FormatExceptionMessage(string message)
		{
			return (this.GetType().ToString() + ": " + message + ": " + typeof(T).ToString());
		}

		// GET: /<controller>/
		[HttpGet]
		public virtual async Task<IActionResult> Index()
		{
			try
			{
				var indexViewModel = new IndexViewModel(ActionContext)
				{
					Data = await Repository.GetAll()
				};

				//return view
				return this.ViewFromModel(indexViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public virtual async Task<IActionResult> Details(TKey id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var detailsViewModel = new DetailsViewModel(ActionContext)
						{
							Data = item
						};

						return this.ViewFromModel(detailsViewModel);
					}
					else
					{
						//return HttpNotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed View Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public virtual async Task<IActionResult> Edit(TKey id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var editViewModel = new EditViewModel(ActionContext)
						{
							Data = item
						};

						return this.ViewFromModel(editViewModel);
					}
					else
					{
						//httpnotfound
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Edit View / Get By Id Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Edit(T item)
		{
			try
			{
				if (ModelState.IsValid && item != null)
				{
					//If Item Exists Update it
					if (await Repository.Exists(Repository.MatchByIdExpression(item.Id)))
					{
						if (await Repository.Update(item))
						{
							return RedirectToAction(nameof(this.Index));
						}
						else
						{
							//Send 500 Response if update fails
							throw new Exception("Update Item Failed");
						}
					}
					else
					{
						//return HttpNotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					//send bad request response with model state errors
					return HttpBadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[HttpGet, Route("[controller]/[action]/")]
		public virtual IActionResult Create()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				string Message = "Get Create View Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[HttpPost, Route("[controller]/[action]/")]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Create(T item)
		{
			try
			{
				if (ModelState.IsValid && item != null)
				{
					//If Item Exists Update it
					if (await Repository.Exists(Repository.MatchByIdExpression(item.Id)))
					{
						if (await Repository.Insert(item))
						{
							return RedirectToAction(nameof(this.Index));
						}
						else
						{
							//Send 500 Response if update fails
							throw new Exception("Creating Item Failed");
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
					//send bad request response with model state errors
					return HttpBadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string Message = "Created New item Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		// Delete View GET: /Delete/5
		[HttpGet, ActionName("Delete"), Route("[controller]/[action]/")]
		public async Task<IActionResult> Delete(TKey id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var deleteViewModel = new DeleteViewModel(ActionContext)
						{
							Data = item
						};

						return this.ViewFromModel(deleteViewModel);
					}
					else
					{
						//return HttpNotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed View Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[HttpPost("{id}"), ActionName("Delete"), Route("[controller]/[action]/")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(TKey id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var result = await Repository.Delete(item);

						if (result)
						{
							return RedirectToAction(nameof(this.Index));
						}
						else
						{
							return HttpBadRequest();
						}
					}
					else
					{
						//return HttpNotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Delete by Id Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}
	}
}