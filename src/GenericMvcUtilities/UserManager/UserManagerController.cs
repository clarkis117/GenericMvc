using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Controllers;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace GenericMvcUtilities.UserManager
{
	[Authorize(Roles = RoleHelper.UserAdmin)]
	public class UserManagerController<T>: Controller where T : class
	{
		protected readonly BaseRepository<T> Repository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<T> Logger;

		public ControllerViewData ControllerViewModel { get; set; }

		public UserManagerController(BaseRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository != null)
				{
					//Set repo to repo field
					this.Repository = repository;

					this.Logger = logger;

					//Get Controler Name
					var controllerName = this.GetControllerName(this.GetType());

					//Create Controller View Model here
					this.ControllerViewModel = new ViewModels.ControllerViewData(controllerName);
				}
				else
				{
					throw new ArgumentNullException(nameof(repository));
				}
			}
			catch (Exception ex)
			{
				string Message = this.FormatExceptionMessage("Creation of Controller Failed");

				throw new Exception(Message, ex);
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
		private string GetControllerName(Type controllerType)
		{
			string controllerName = controllerType.Name;

			if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
			{
				controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
			}

			return controllerName;
		}

		// GET: /<controller>/
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			const string instructions = "All Users in the Database";

			ICollection<ViewModels.ActionViewData> actionViewModels = new List<ViewModels.ActionViewData>();

			try
			{

				var actionViewModel = new ActionViewData(
					this.ControllerViewModel,
					this.ActionContext.RouteData.Values["action"].ToString(),
					instructions,
					await Repository.GetAll());

				actionViewModels.Add(actionViewModel);

				//return view
				return View(this.ControllerViewModel.SharedViewPath, actionViewModels);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id:int}")]
		public async Task<IActionResult> Details(int? id)
		{
			string instructions = "All " + this.ControllerViewModel.ControllerName + "s in the Database";

			try
			{
				if (id != null)
				{
					var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						return View(item);
					}
					else
					{
						return HttpNotFound();
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
		[HttpGet("{id:int}")]
		public async Task<IActionResult> Edit(int? id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						return View(item);
					}
					else
					{
						Type type = typeof(T);
						T result = (T)Activator.CreateInstance(type);

						return View(result);
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
		public async Task<IActionResult> Edit(T item)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await Repository.Update(item);

					return RedirectToAction("Index");
				}

				return View(item);
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int? id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var result = await Repository.Delete(item);

						if (result != false)
						{
							return RedirectToAction("Index");
						}
						else
						{
							return HttpBadRequest();
						}
					}
					else
					{
						return HttpNotFound();
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

		public IActionResult PendingUserIndex()
		{
			//Serve index view with all pending users loaded 
			return null;
		}

		public IActionResult DetailedPendingUser(int? id)
		{
			return null;
		}

		public IActionResult AproveUser(int? id)
		{
			//create user account
			//send email stating request approved
			return null;
		}

		public IActionResult DenyUser(int? id)
		{
			//delete user account request
			//send email stating request denied
			return null;
		}
	}
}
