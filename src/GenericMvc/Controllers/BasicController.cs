using GenericMvc.Models;
using GenericMvc.Repositories;
using GenericMvc.ViewModels.Basic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GenericMvc.Controllers
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

		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public virtual async Task<IActionResult> Details(TKey id, Status? message)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var detailsViewModel = new DetailsViewModel(this)
						{
							Id = item.Id,
							Data = item,
							Message = GetMessageFromEnum(message)
						};

						return this.ViewFromModel(detailsViewModel);
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.ItemNotFound});
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.RequestOrQueryIsInvalid});
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Detailed View Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public virtual async Task<IActionResult> Edit(TKey id, Status? message)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var editViewModel = new EditViewModel(this)
						{
							Id = item.Id,
							Data = item,
							Message = GetMessageFromEnum(message)
						};

						return this.ViewFromModel(editViewModel);
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.ItemNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.RequestOrQueryIsInvalid });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Edit View / Get By Id Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		public static IActionResult GetEditViewModel(Controller controller, T item, Status message)
		{
			var editViewModel = new EditViewModel(controller)
			{
				Id = item.Id,
				Data = item,
				Message = GetMessageFromEnum(message)
			};

			return ViewModelHelper.ViewFromModel(controller, editViewModel);
		}

		//todo use get post redirect to move user to created or updated item
		[Route("[controller]/[action]/"), HttpPost, ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Edit(T item)
		{
			try
			{
				if (item != null && ModelState.IsValid)
				{
					//If Item Exists Update it
					if (await Repository.Any(Repository.MatchByIdExpression(item.Id)))
					{
						var updatedItem = await Repository.Update(item);

						if (updatedItem != null)
						{
							//make redirect and retrieve item
							return RedirectToAction(nameof(this.Edit), new { id = updatedItem.Id, message = Status.ItemHasBeenEdited});
						}
						else
						{
							//Send 500 Response if update fails
							//throw new Exception("Update Item Failed");
							return GetEditViewModel(this, item, Status.ErrorProcessingRequest);
						}
					}
					else
					{
						//return NotFound();
						return RedirectToAction(nameof(this.Index), new { message = Status.ItemNotFound});
					}
				}
				else
				{
					if (item != null)
					{
						//send the user back the form with the bad data
						return GetEditViewModel(this, item, Status.ItemIsNotValidAndChangesHaveNotBeenSaved);
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.RequestOrQueryIsInvalid });
					}
				}
			}
			catch (Exception ex)
			{
				string message = "Posting Edit Failed";

				this.Logger.LogError(FormatLogMessage(message, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);

				return RedirectToAction(nameof(this.Index), new { message = Status.ItemCouldNotBeEdited });
			}
		}

		[Route("[controller]/[action]/"), HttpGet]
		public virtual IActionResult Create(Status? message)
		{
			try
			{
				var createViewModel = new CreateViewModel(this)
				{
					Data = Activator.CreateInstance<T>(),
					Message = GetMessageFromEnum(message)
				};

				return this.ViewFromModel(createViewModel);
			}
			catch (Exception ex)
			{
				string errorMessage = "Get Create View Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		[NonAction]
		protected static IActionResult GetCreateViewModel(Controller controller, T item, Status message)
		{
			var createViewModel = new CreateViewModel(controller)
			{
				Data = item,
				Message = GetMessageFromEnum(message)
			};

			return ViewModelHelper.ViewFromModel(controller, createViewModel);
		}

		[Route("[controller]/[action]/"), HttpPost, ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Create(T item)
		{
			try
			{
				if (item != null && ModelState.IsValid)
				{
					var createdItem = await Repository.Create(item);

					if (createdItem != null)
					{
						return RedirectToAction(nameof(this.Edit), new {id = createdItem.Id, message = Status.ItemHasBeenCreated});
					}
					else
					{
						//Send 500 Response if update fails
						return GetCreateViewModel(this, item, Status.ItemCouldNotBeCreated);
					}
				}
				else
				{
					if (item != null)
					{
						return GetCreateViewModel(this, item, Status.ItemIsNotValidAndChangesHaveNotBeenSaved);
					}
					else
					{
						return RedirectToAction(nameof(this.Create), new { message = Status.RequestOrQueryIsInvalid });
					}
				}
			}
			catch (Exception ex)
			{
				string message = "Created New item Failed";

				this.Logger.LogError(FormatLogMessage(message, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ItemCouldNotBeCreated });
			}
		}

		// Delete View GET: /Delete/5
		[Route("[controller]/[action]/"), HttpGet("{id}")]
		public async Task<IActionResult> Delete(TKey id, Status? message)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var deleteViewModel = new DeleteViewModel(this)
						{
							Id = item.Id,
							Data = item,
							Message = GetMessageFromEnum(message)
						};

						return this.ViewFromModel(deleteViewModel);
					}
					else
					{
						//return NotFound();
						return RedirectToAction(nameof(this.Index), new { message = Status.ItemNotFound });
					}
				}
				else
				{
					//else can't find model
					return RedirectToAction(nameof(this.Index), new { message = Status.RequestOrQueryIsInvalid });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Detailed View Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		[Route("[controller]/[action]/"), HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(TKey id)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var result = await Repository.Delete(item);

						if (result)
						{
							return RedirectToAction(nameof(this.Index), new { message = Status.ItemHasBeenDeleted});
						}
						else
						{
							throw new Exception("Item could not be deleted");
						}
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.ItemNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.RequestOrQueryIsInvalid });
				}
			}
			catch (Exception ex)
			{
				string message = "Delete by Id Failed";

				this.Logger.LogError(FormatLogMessage(message, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);

				return RedirectToAction(nameof(this.Index), new { message = Status.ItemCouldNotBeDeleted });
			}
		}
	}
}