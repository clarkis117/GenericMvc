using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Controllers;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels;
using GenericMvcUtilities.ViewModels.Generic;
using GenericMvcUtilities.ViewModels.UserManager;
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
	[Authorize(Roles = RoleHelper.SystemOwner + "," + RoleHelper.UserAdmin)]
	public class UserManagerController<TUser, TPendingUser, TKey, TRole> : Controller
		where TUser : IdentityUser<TKey>, IUserConstraints, new()
		where TPendingUser : PendingUser<TKey>
		where TKey : IEquatable<TKey>
		where TRole : IdentityRole<TKey>
	{
		protected readonly UserManager<TUser> UserManager;

		protected readonly RoleManager<TRole> RoleManager;

		protected readonly BaseRepository<TUser> UserRepository;

		protected readonly BaseRepository<TPendingUser> PendingUserRepository;

		//Maybe One Day using Logger<T> instead
		protected readonly ILogger<UserManagerController<TUser, TPendingUser, TKey, TRole>> Logger;

		public ControllerViewModel ControllerViewModel { get; }

		
		public UserManagerController( UserManager<TUser> userManager,
			RoleManager<TRole> roleManager,
			BaseRepository<TUser> userRepository,
			BaseRepository<TPendingUser> pendingUserRepository,
			ILogger<UserManagerController<TUser, TPendingUser, TKey, TRole>> logger)
		{
			try
			{
				//Check parameters for null values
				if (userManager != null
					&& userRepository != null
					&& roleManager != null
					&& pendingUserRepository != null
					&& logger != null)
				{
					//Set fields
					this.UserManager = userManager;

					//this.UserManager.g

					this.RoleManager = roleManager;

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

		//todo merge back to basic controller
		[NonAction]
		private string GetDepluaralizedControllerName(Type controllerType)
		{
			string controllerName = controllerType.Name;

			if (controllerName.EndsWith("sController", StringComparison.OrdinalIgnoreCase))
			{
				controllerName = controllerName.Substring(0, controllerName.Length - "sController".Length);
			}

			return controllerName;
		}

		// GET: /<controller>/
		[HttpGet]
		public async Task<IActionResult> UserIndex()
		{
			try
			{
				TableViewModel tableViewModel = new TableViewModel()
				{
					ControllerName = GetControllerName(this.GetType()),
					Title = "Application Users",
					Action = "Index",
					Description = "All Users in the Database",
					CreateButton = false,
					NestedView = "UserIndex"
				};

				var viewModelList = new List<IUserIndexView>();

				var users = await UserRepository.GetAll();

				if (users != null)
				{
					foreach (var user in await UserRepository.GetAll())
					{
						viewModelList.Add(new UserIndexViewModel<TKey, TUser>(user));
					}
				}

				tableViewModel.Data = viewModelList;

				return View("~/Views/Shared/TableContainer.cshtml", tableViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		public enum ManageMessageId
		{
			UserCannotBeLocked,
			UserAccountHasBeenLocked,
			ErrorOccuredProcessingRequest,
			UserAccountCreationSucceeded,
			UserAccountCreationFailed
		}

		//to-do: implement status message
		//todo: show user details page with actions on it
		[Route("[controller]/[action]/")]
		[HttpGet("{id}")]
		public async Task<IActionResult> UserDetails(TKey id, ManageMessageId? message = null)
		{

			try
			{
				if (id != null)
				{
					//var item = await UserRepository.GetCompleteItem(UserRepository.IsMatchedExpression("Id", id));
					var user = await UserManager.FindByIdAsync(id.ToString());

					if (user != null)
					{
						return View("~/Views/UserManager/UserDetails.cshtml", new UserDetailsViewModel<TKey,TUser, TRole>(user, this.RoleManager, this.UserManager));
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
				string Message = "Detailed User View Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: multiple role handling 
		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserRole(TKey id, string changeRole)
		{
			try
			{
				if (ModelState.IsValid)
				{

					//UserManager.res



					//return to the user details
					return RedirectToAction(nameof(this.UserDetails));
				}
				else
				{
					throw new Exception();
				}
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(TKey id)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var user = await this.UserManager.FindByIdAsync(id.ToString());

					if (user != null)
					{
						//await this.UserManager.GeneratePasswordResetTokenAsync()
						//await this.UserManager.pas
						
					}
					else
					{
						return HttpNotFound();
					}





					//return to the user details
					return RedirectToAction(nameof(this.UserDetails));
				}
				else
				{
					throw new Exception();
				}
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo if you want to permanately suspend user, remove password, invalidate access tokens 
		//todo: send email notifying user their account has been locked
		/* note date time offset is my localtime zone so est -5	*/
		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LockUser(TKey id, DateTimeOffset? lockOutTime)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var user = await this.UserManager.FindByIdAsync(id.ToString());

					if (user != null)
					{
						var canlockuser = this.UserManager.GetLockoutEnabledAsync(user);
						var canlock = this.UserManager.SupportsUserLockout;

						if (canlock && await canlockuser) //lock user
						{
							IdentityResult result;

							//todo fix this, 
							if (lockOutTime != null)
							{
								result = await this.UserManager.SetLockoutEndDateAsync(user, lockOutTime);
							}
							else
							{
								await this.UserManager.SetLockoutEndDateAsync(user, null);
								result = await this.UserManager.SetLockoutEnabledAsync(user, true);
							}

							if (result.Succeeded)
							{
								return RedirectToAction(nameof(this.UserDetails),
									new {id = id, message = ManageMessageId.UserAccountHasBeenLocked });
							}
							else
							{
								return RedirectToAction(nameof(this.UserDetails),
									new { id = id, message = ManageMessageId.ErrorOccuredProcessingRequest });
							}
						}
						else
						{
							return RedirectToAction(nameof(this.UserDetails), new {id = id , message = ManageMessageId.UserCannotBeLocked});
						}
					}
					else
					{
						return HttpNotFound();
					}
				}
				else
				{
					return HttpBadRequest(this.ModelState);
				}
			}
			catch (Exception ex)
			{
				string message = "Locking User Account Failed";

				this.Logger.LogError(this.FormatLogMessage(message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(message), ex);
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
					//var item = await UserRepository.Get(UserRepository.IsMatchedExpression("Id", id));
					var user = await UserManager.FindByIdAsync(id.ToString());

					if (user != null)
					{
						//var result = await UserRepository.Delete(item);
						var result = await UserManager.DeleteAsync(user);

						if (result.Succeeded)
						{
							return RedirectToAction(nameof(UserIndex));
						}
						else
						{
							//todo: probably internal error
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
			try
			{
				TableViewModel tableViewModel = new TableViewModel()
				{
					ControllerName = GetControllerName(this.GetType()),
					Title = "Pending Users",
					Action = "Index",
					Description = "All Pending Users in the Database",
					CreateButton = false,
					NestedView = "PendingUserIndex"
				};

				var viewModelList = new List<IUserIndexView>();

				var pendingUsers = await PendingUserRepository.GetAll();

				if(pendingUsers != null)
				{
					foreach (var pendingUser in pendingUsers)
					{
						viewModelList.Add(new PendingUserViewModel<TKey, TPendingUser>(pendingUser));
					}
				}

				tableViewModel.Data = viewModelList;

				return View("~/Views/Shared/TableContainer.cshtml", tableViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> DetailedPendingUser(TKey id)
		{
			//Serve the detailed view of the user
			return null;
		}

		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePendingUserRole(TKey id, string requestedRole)
		{
			//Serve the detailed view of the user
			return null;
		}

		//todo: email Notification
		//todo: handle user role creation
		[Route("[controller]/[action]/")]
		[HttpPost("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ApproveUser(TKey id)
		{
			//create user account
			//send email stating request approved
			try
			{
				if (id != null)
				{
					var pendingUser = await PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", id));

					if (pendingUser != null)
					{
						//todo: phone number and password
						TUser user = new TUser()
						{
							FirstName = pendingUser.FirstName,
							LastName = pendingUser.LastName,
							Email = pendingUser.Email,
							UserName = pendingUser.Email,
							DateRegistered = pendingUser.DateRegistered
						};

						var createdResult = await UserManager.CreateAsync(user);

						if (createdResult.Succeeded)
						{
							var result = PendingUserRepository.Delete(pendingUser);

							if (await result != false)
							{
								//redirect with status message that user has been added
								return RedirectToAction(nameof(this.PendingUserIndex),
									new { ManageMessageId = ManageMessageId.UserAccountCreationSucceeded });
							}
							else
							{
								string concatErrors = "";

								foreach (var error in createdResult.Errors)
								{
									concatErrors += error.Code + ": " + error.Description + ", ";
								}

								throw new Exception(concatErrors);
							}
						}
						else
						{
							//redirect back to user page with status message
							return RedirectToAction(nameof(this.PendingUserIndex),
								new { ManageMessageId = ManageMessageId.UserAccountCreationFailed });
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
				string Message = "Approving Pending User Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: email Notification
		//todo: reason message for notification
		//todo: status message for success or failure
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
					var user = await PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", id));

					if (user != null)
					{
						var result = await PendingUserRepository.Delete(user);

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
				string Message = "Deleting Pending User by Id Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}
	}
}
