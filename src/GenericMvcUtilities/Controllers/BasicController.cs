using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels.Basic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	//this should be handled in derived classes: [Authorize(Roles = RoleHelper.SystemOwner + "," + RoleHelper.UserAdmin + "," + RoleHelper.ContentAdmin)]
	public abstract class BasicController<TKey, T> : BaseController<TKey, T>, IBaseController<TKey, T>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseController{T}" /> class.
		/// </summary>
		/// <param name="repository">The repo.</param>
		public BasicController(IEntityRepository<T> repository, ILogger<T> logger) : base(repository, logger)
		{
			try
			{
				if (repository != null && logger != null)
				{
					//Set repo to repo field
					//this.Repository = repository;

					//this.Logger = logger;
				}
				else
				{
					throw new ArgumentNullException("Repository or Logger argument is null");
				}
			}
			catch (Exception ex)
			{
				string message = FormatExceptionMessage(this, "Creation of Controller Failed");

				this.Logger.LogCritical(message, ex);

				throw new Exception(message, ex);
			}
		}

		/*
		// GET: /<controller>/
		[Route("[controller]/[action]/")]
		[HttpGet]
		public virtual async Task<IActionResult> Index()
		{
			try
			{
				var indexViewModel = new IndexViewModel(this)
				{
					Data = await Repository.GetAll()
				};

				//return view
				return this.ViewFromModel(indexViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this,Message), ex);
			}
		}
		*/

		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public virtual async Task<IActionResult> Details(TKey id, Message? message)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var detailsViewModel = new DetailsViewModel(this)
						{
							Id = item.Id,
							Data = item
						};

						return this.ViewFromModel(detailsViewModel);
					}
					else
					{
						//return NotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed View Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public virtual async Task<IActionResult> Edit(TKey id, Message? message)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var editViewModel = new EditViewModel(this)
						{
							Id = item.Id,
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
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Edit View / Get By Id Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("[controller]/[action]/"), HttpPost, ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Edit(T item)
		{
			try
			{
				if (ModelState.IsValid && item != null)
				{
					//If Item Exists Update it
					if (await Repository.Any(Repository.MatchByIdExpression(item.Id)))
					{
						var updatedItem = await Repository.Update(item);

						if (updatedItem != null)
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
						//return NotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					//send bad request response with model state errors
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("[controller]/[action]/"), HttpGet]
		public virtual IActionResult Create(Message? message)
		{
			try
			{
				var createViewModel = new CreateViewModel(this)
				{
					Data = Activator.CreateInstance<T>()
				};

				return this.ViewFromModel(createViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get Create View Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("[controller]/[action]/"), HttpPost, ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Create(T item)
		{
			try
			{
				if (ModelState.IsValid && item != null)
				{
					var createdItem = await Repository.Create(item);

					if (createdItem != null)
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
					//send bad request response with model state errors
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				string Message = "Created New item Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		// Delete View GET: /Delete/5
		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public async Task<IActionResult> Delete(TKey id, Message? message)
		{
			try
			{
				//case item couldn't be deleted
				//case error

				if (id != null && ModelState.IsValid)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var deleteViewModel = new DeleteViewModel(this)
						{
							Id = item.Id,
							Data = item
						};

						return this.ViewFromModel(deleteViewModel);
					}
					else
					{
						//return NotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					//else can't find model, and can't do shit
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed View Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[Route("[controller]/[action]/"), HttpPost("{id}"), ValidateAntiForgeryToken]
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
							return BadRequest();
						}
					}
					else
					{
						//return NotFound();
						return RedirectToAction(nameof(this.Index));
					}
				}
				else
				{
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Delete by Id Failed";

				this.Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}
	}
}