using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Controllers;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// see this blog post about the redirect design used in this controller: http://www.aspnetmvcninja.com/controllers/why-you-should-use-post-redirect-get
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="X"></typeparam>
	/// <seealso cref="Microsoft.AspNet.Mvc.Controller" />
	[Authorize(Roles = RoleHelper.UserAdmin)]
	public class UserManagerController<T, X> : Controller
		where T : IdentityUser
		where X : PendingUser
	{
		protected readonly BaseRepository<T> UserRepository;

		protected readonly BaseRepository<X> PendingUserRepository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<T> Logger;

		public ControllerViewData ControllerViewModel { get; }

		public UserManagerController( BaseRepository<T> userRepository, BaseRepository<X> pendingUserRepository, ILogger<T> logger)
		{
			try
			{
				if (userRepository != null)
				{
					//Set repo to repo field
					this.UserRepository = userRepository;

					this.PendingUserRepository = pendingUserRepository;

					this.Logger = logger;

					//Get Controler Name
					var controllerName = this.GetControllerName(this.GetType());

					//Create Controller View Model here
					this.ControllerViewModel = new ViewModels.ControllerViewData(controllerName);
				}
				else
				{
					throw new ArgumentNullException(nameof(userRepository));
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
					await UserRepository.GetAll());

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
		[HttpGet("{id:Guid}")]
		public async Task<IActionResult> Details(Guid? id)
		{
			var instructions = "All " + this.ControllerViewModel.ControllerName + "s in the Database";

			try
			{
				if (id != null)
				{
					var item = await UserRepository.GetCompleteItem(UserRepository.IsMatchedExpression("Id", id));

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
		[HttpGet("{id:Guid}")]
		public async Task<IActionResult> Edit(Guid? id)
		{
			try
			{
				if (id != null)
				{
					var item = await UserRepository.Get(UserRepository.IsMatchedExpression("Id", id));

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
					await UserRepository.Update(item);

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
		[HttpDelete("{id:Guid}")]
		public async Task<IActionResult> Delete(Guid? id)
		{
			try
			{
				if (id != null)
				{
					var item = await UserRepository.Get(UserRepository.IsMatchedExpression("Id", id));

					if (item != null)
					{
						var result = await UserRepository.Delete(item);

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

		[HttpGet]
		public async Task<IActionResult> PendingUserIndex()
		{
			//Serve index view with all pending users loaded 
			return null;
		}

		[HttpGet]
		public async Task<IActionResult> DetailedPendingUser(Guid? id)
		{
			//Serve the detailed view of the user
			return null;
		}

		[HttpPost]
		public async Task<IActionResult> DetailedPendingUser(Guid? id)
		{
			//Serve the detailed view of the user
			return null;
		}

		[HttpPost]
		public async Task<IActionResult> AproveUser(Guid? id)
		{
			//create user account
			//send email stating request approved
			return null;
		}

		[HttpPost]
		public async Task<IActionResult> DenyUser(Guid? id)
		{
			//delete user account request
			//send email stating request denied
			try
			{
				if (id != null)
				{
					var item = await UserRepository.Get(UserRepository.IsMatchedExpression("Id", id));

					if (item != null)
					{
						var result = await UserRepository.Delete(item);

						if (result != false)
						{
							return RedirectToAction(nameof(this.PendingUserIndex));
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
	}
}
