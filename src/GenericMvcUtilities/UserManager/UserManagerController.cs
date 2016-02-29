using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Controllers;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// see this blog post about the redirect design used in this controller: http://www.aspnetmvcninja.com/controllers/why-you-should-use-post-redirect-get
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	/// <typeparam name="TPendingUser"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <seealso cref="Microsoft.AspNet.Mvc.Controller" />
	[Authorize(Roles = RoleHelper.UserAdmin)]
	public class UserManagerController<TUser, TPendingUser, TKey> : Controller
		where TUser : IdentityUser<TKey>
		where TPendingUser : PendingUser<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly UserManager<TUser> UserManager;

		protected readonly BaseRepository<TUser> UserRepository;

		protected readonly BaseRepository<TPendingUser> PendingUserRepository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<UserManagerController<TUser, TPendingUser, TKey>> Logger;

		public ControllerViewModel ControllerViewModel { get; }

		
		public UserManagerController( UserManager<TUser> userManager, BaseRepository<TUser> userRepository, BaseRepository<TPendingUser> pendingUserRepository, ILogger<UserManagerController<TUser, TPendingUser, TKey>> logger)
		{
			try
			{
				//Check parameters for null values
				if (userManager != null
					&& userRepository != null
					&& pendingUserRepository != null
					&& logger != null)
				{
					//Set fields
					this.UserManager = userManager;

					this.UserRepository = userRepository;

					this.PendingUserRepository = pendingUserRepository;

					this.Logger = logger;

					//Get Controller Name
					var controllerName = this.GetControllerName(this.GetType());

					//Create Controller View Model here
					this.ControllerViewModel = new ViewModels.ControllerViewModel(controllerName);
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
			return (this.GetType().Name + ": " + message + ": " + typeof(TUser));
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
		public async Task<IActionResult> UserIndex()
		{
			const string instructions = "All Users in the Database";

			ICollection<ViewModels.ActionViewData> actionViewModels = new List<ViewModels.ActionViewData>();

			try
			{
				UserManager.lo
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

		//todo: show user details page with actions on it
		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public async Task<IActionResult> UserDetails(TKey id)
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


		//todo: do I need this?
		[Route("[controller]/[action]/")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserRole(TUser item)
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

		//todo: Change to removing this user
		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveUser(TKey id)
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
		public async Task<IActionResult> DetailedPendingUser(TKey id)
		{
			//Serve the detailed view of the user
			return null;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePendingUserRole(TKey id)
		{
			//Serve the detailed view of the user
			return null;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AproveUser(TKey id)
		{
			//create user account
			//send email stating request approved
			return null;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DenyUser(TKey id)
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
