using GenericMvcUtilities.Attributes;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels.Basic;
using GenericMvcUtilities.ViewModels.UserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GenericMvcUtilities.UserManager
{
	//todo: change so that current user is resolved to an actual user account and not just he claims principal 
	//todo: don't show the user's current role in, role change selection list
	//todo: enable password resets
	/// <summary>
	/// see this blog post about the redirect design used in this controller: http://www.aspnetmvcninja.com/controllers/why-you-should-use-post-redirect-get
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	/// <typeparam name="TPendingUser"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
	///[Authorize(Roles = RoleHelper.SystemOwner + "," + RoleHelper.UserAdmin)]
	[Route("UserManager/[controller]/[action]/"), AuthorizeUserAdmin]
	public class UserController<TUser, TRole, TKey> : Controller
		where TUser : IdentityUser<TKey>, IPrivilegedUserConstraints, new()
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly UserManager<TUser> UserManager;

		protected readonly RoleManager<TRole> RoleManager;

		protected readonly BaseEntityRepository<TUser> UserRepository;

		protected readonly ILogger<UserController<TUser, TRole, TKey>> Logger;

		public UserController(UserManager<TUser> userManager,
			RoleManager<TRole> roleManager,
			BaseEntityRepository<TUser> userRepository,
			ILogger<UserController<TUser, TRole, TKey>> logger)
		{
			try
			{
				if (userManager != null
					&& userRepository != null
					&& roleManager != null
					&& logger != null)
				{
					this.UserManager = userManager;

					this.RoleManager = roleManager;

					this.UserRepository = userRepository;

					this.Logger = logger;
				}
				else
				{
					throw new ArgumentNullException(nameof(userRepository));
				}
			}
			catch (Exception ex)
			{
				string Message = FormatExceptionMessage(this, "Creation of Controller Failed");

				throw new Exception(Message, ex);
			}
		}

		[NonAction]
		protected static string FormatLogMessage(string message, Microsoft.AspNetCore.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		//Todo: revamp this hardcore
		[NonAction]
		protected static string FormatExceptionMessage(Controller controller, string message)
		{
			return (controller.GetType().Name + ": " + message + ": " + typeof(TUser));
		}

		[NonAction]
		private async Task<bool> IsSystemOwnerAccount(TUser user)
		{
			var roles = await UserManager.GetRolesAsync(user);

			return roles.Any(x => x == RoleHelper.SystemOwner);
		}

		[NonAction]
		private async Task<bool> IsAnotherUserAdminAccount(TUser user)
		{
			var roles = await UserManager.GetRolesAsync(user);

			return roles.Any(x => x == RoleHelper.UserAdmin);
		}

		[NonAction]
		private static bool IsUsersOwnAccount(TUser user, ClaimsPrincipal currentUser)
		{
			if (currentUser.Identity.Name == user.UserName)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		[NonAction]
		private async Task<Status> CanThisAccountBeManaged(TUser user, ClaimsPrincipal currentUser)
		{
			//users are not allowed to manage their own accounts
			if (IsUsersOwnAccount(user, currentUser))
			{
				return Status.YouCannotManageYourOwnAccount;
			}
			else if (currentUser.IsInRole(RoleHelper.SystemOwner) && await IsAnotherUserAdminAccount(user))
			{
				//if current user is system owner, and user to be managed is a user admin, allow it
				return Status.NoError;
			}
			else if (await IsSystemOwnerAccount(user))
			{
				return Status.YouCannotManageSystemOwner;
			}
			else if (await IsAnotherUserAdminAccount(user))
			{
				return Status.YouCannotManagerAnotherUserAdmin;
			}
			else if (await DoesUserHaveManagableRole(user))
			{
				//if a user is not part of the privilege user system then they are not manageable
				return Status.UserDoesNotHaveAManagableRole;
			}
			else
			{
				return Status.NoError;
			}
		}

		public enum Status : byte
		{
			AccountLocked,
			PasswordReset,
			AccountDeleted,
			RoleChanged,
			NoError,
			UserNotFound,
			UserCannotBeLocked,
			ErrorProcessingRequest,
			UserDoesNotHaveAManagableRole,
			YouCannotManagerAnotherUserAdmin,
			YouCannotManageYourOwnAccount,
			YouCannotManageSystemOwner
		}

		[NonAction]
		public async Task<bool> DoesUserHaveManagableRole(TUser user)
		{
			var roles = await UserManager.GetRolesAsync(user);

			foreach (var role in RoleHelper.MutableRoles)
			{
				if (roles.Any(x => x == role))
				{
					return true;
				}
			}

			return false;
		}

		[NonAction]
		public async Task<bool> FindAndChangeRole(TUser user, string newRole)
		{
			var roles = await UserManager.GetRolesAsync(user);

			foreach (var role in RoleHelper.MutableRoles)
			{
				if (roles.Any(x => x == role))
				{
					var currentRole = roles.First(x => x == role);

					var removeCurrentRoleResult = await UserManager.RemoveFromRoleAsync(user, currentRole);

					if (removeCurrentRoleResult.Succeeded)
					{
						var addToNewRoleResult = await UserManager.AddToRoleAsync(user, newRole);

						if (addToNewRoleResult.Succeeded)
						{
							return true;
						}
						else
						{
							//if for some reason this failed try adding old role back
							var revertResult = await UserManager.AddToRoleAsync(user, currentRole);

							if (!revertResult.Succeeded)
								throw new Exception("Something is going wrong here really bad, with changing roles in UserManager");

							return false;
						}
					}
				}
			}

			return false;
		}

		//todo make these actions
		//public virtual Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token);

		//public virtual Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);

		//public virtual Task<IdentityResult> ChangePhoneNumber(TUser user, string phoneNumber, string token);

		// GET: /<controller>/
		[HttpGet]
		public async Task<IActionResult> Index(Status? message)
		{
			try
			{
				MessageViewModel messageViewModel = null;

				if (message != null)
				{
					switch (message.Value)
					{
						case Status.UserNotFound:
							messageViewModel = new MessageViewModel(MessageType.Danger, "System could not find the selected User");
							break;

						case Status.UserDoesNotHaveAManagableRole:
							messageViewModel = new MessageViewModel(MessageType.Warning, "This user is not a privileged user therefore, they cannot be managed");
							break;

						case Status.ErrorProcessingRequest:
							messageViewModel = new MessageViewModel(MessageType.Danger, "System the encountered an Error Processing your Request");
							break;

						case Status.YouCannotManageYourOwnAccount:
							messageViewModel = new MessageViewModel(MessageType.Danger, "You Cannot Manage your Own User Account");
							break;

						case Status.YouCannotManageSystemOwner:
							messageViewModel = new MessageViewModel(MessageType.Danger, "You cannot manage the System Owner Account");
							break;

						case Status.YouCannotManagerAnotherUserAdmin:
							messageViewModel = new MessageViewModel(MessageType.Danger, "Only the System Owner can manage another User Admin");
							break;

						default:
							messageViewModel = new MessageViewModel();
							break;
					}
				}

				IndexViewModel tableViewModel = new IndexViewModel(this)
				{
					ControllerName = "UserManager/User",
					Title = "Application Users",
					Action = "Index",
					Description = "All Users in the Database",
					ShowCreateButton = false,
					ShowSearchForm = false,
					UseNestedViewConventions = false,
					NestedView = "Index",
					Message = messageViewModel ?? new MessageViewModel()
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

				return this.ViewFromModel(tableViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Index Failed";

				Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Details(TKey id, Status? message = null)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var user = await UserManager.FindByIdAsync(id.ToString());

					//redirect is can't find user
					if (user == null)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}

					var isManagable = await CanThisAccountBeManaged(user, this.User);

					//redirect if user is not allowed to manage user
					if (isManagable != Status.NoError)
					{
						return RedirectToAction(nameof(this.Index), new { message = isManagable });
					}

					MessageViewModel messageViewModel = null;

					if (message != null)
					{
						switch (message.Value)
						{
							case Status.AccountLocked:
								messageViewModel = new MessageViewModel(MessageType.Success, "The User's Account has been successfully locked");
								break;

							case Status.UserCannotBeLocked:
								messageViewModel = new MessageViewModel(MessageType.Danger, "The System could not lock this user's account");
								break;

							case Status.PasswordReset:
								messageViewModel = new MessageViewModel(MessageType.Success, "The user's password has been successfully reset");
								break;

							case Status.RoleChanged:
								messageViewModel = new MessageViewModel(MessageType.Success, "The User's Role has been successfully changed");
								break;

							case Status.ErrorProcessingRequest:
								messageViewModel = new MessageViewModel(MessageType.Danger, "System the encountered an Error Processing your Request");
								break;

							default:
								messageViewModel = new MessageViewModel();
								break;
						}
					}


					var viewModel = new UserDetailsViewModel<TKey, TUser, TRole>(user, this.RoleManager, this.UserManager)
					{
						Message = messageViewModel ?? new MessageViewModel()
					};

					if (user != null)
					{
						ViewData["Roles"] = RoleHelper.SelectableRoleList();

						return View("~/Views/UserManager/User/Details.cshtml", viewModel);
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest});
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Detailed User View Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		//todo: multiple role handling
		[HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserRole(RoleChangeViewModel roleChange)
		{
			try
			{
				if (roleChange != null && roleChange.IsValid && ModelState.IsValid)
				{
					var user = await UserManager.FindByIdAsync(roleChange.ToString());

					//redirect is can't find user
					if (user == null)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}

					var isManagable = await CanThisAccountBeManaged(user, this.User);

					//redirect if user is not allowed to manage user
					if (isManagable != Status.NoError)
					{
						return RedirectToAction(nameof(this.Index), new { message = isManagable });
					}

					if(await this.FindAndChangeRole(user, roleChange.NewRole))
					{
						return RedirectToAction(nameof(this.Details), new { id = user.Id, message = Status.RoleChanged });
					}
					else
					{
						return RedirectToAction(nameof(this.Details), new { id = user.Id, message = Status.ErrorProcessingRequest });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Changing Role Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		[HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(TKey id)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var user = await this.UserManager.FindByIdAsync(id.ToString());

					//redirect is can't find user
					if (user == null)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}

					var isManagable = await CanThisAccountBeManaged(user, this.User);

					//redirect if user is not allowed to manage user
					if (isManagable != Status.NoError)
					{
						return RedirectToAction(nameof(this.Index), new { message = isManagable });
					}

					//do password reset here
					//need some kind of token sending system here
					//UserManager.ResetPasswordAsync()

					throw new NotImplementedException();

					//return to the user details
					//return RedirectToAction(nameof(this.Details));
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Issuing user password reset failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		//todo add offset entry control to modal
		//todo if you want to permanately suspend user, remove password, invalidate access tokens
		//todo: send email notifying user their account has been locked
		/* note date time offset is my localtime zone so est -5	*/

		[HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> LockUser(LockViewModel lockViewModel)
		{
			try
			{
				if (lockViewModel != null && lockViewModel.IsValid && ModelState.IsValid)
				{
					var user = await UserManager.FindByIdAsync(lockViewModel.Id);

					//redirect is can't find user
					if (user == null)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}

					var isManagable = await CanThisAccountBeManaged(user, this.User);

					//redirect if user is not allowed to manage user
					if (isManagable != Status.NoError)
					{
						return RedirectToAction(nameof(this.Index), new { message = isManagable });
					}

					//lockout logic
					if (this.UserManager.SupportsUserLockout && await this.UserManager.GetLockoutEnabledAsync(user))
					{
						var lockoutEnabledResult = await UserManager.SetLockoutEnabledAsync(user, true);

						if (lockoutEnabledResult.Succeeded)
						{
							var setEndDate = await UserManager.SetLockoutEndDateAsync(user, lockViewModel.ConvertToLockoutTime());

							if (setEndDate.Succeeded)
							{
								return RedirectToAction(nameof(this.Details), new { id = user.Id, message = Status.AccountLocked });
							}
						}

						//error processing request
						return RedirectToAction(nameof(this.Details), new { id = user.Id, message = Status.ErrorProcessingRequest });
					}

					return RedirectToAction(nameof(this.Details), new { id = user.Id, message = Status.UserCannotBeLocked });
				}
				else
				{
					//Id is required so if model state not valid redirect to index
					return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Locking User Account Failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}

		[HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveUser(TKey id)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var user = await UserManager.FindByIdAsync(id.ToString());

					//redirect is can't find user
					if (user == null)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.UserNotFound });
					}

					var isManagable = await CanThisAccountBeManaged(user, this.User);

					//redirect if user is not allowed to manage user
					if (isManagable != Status.NoError)
					{
						return RedirectToAction(nameof(this.Index), new { message = isManagable });
					}

					var result = await UserManager.DeleteAsync(user);

					if (result.Succeeded)
					{
						return RedirectToAction(nameof(this.Index), new { message = Status.AccountDeleted });
					}
					else
					{
						return RedirectToAction(nameof(this.Details), new { id = id, message = Status.ErrorProcessingRequest });
					}
				}
				else
				{
					//index because no id
					return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Removing user from system failed";

				this.Logger.LogError(FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(FormatExceptionMessage(this, Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Status.ErrorProcessingRequest });
			}
		}
	}
}