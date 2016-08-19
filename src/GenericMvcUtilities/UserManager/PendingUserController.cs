using GenericMvcUtilities.Attributes;
using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels.Basic;
using GenericMvcUtilities.ViewModels.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// see this blog post about the redirect design used in this controller: http://www.aspnetmvcninja.com/controllers/why-you-should-use-post-redirect-get
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	/// <typeparam name="TPendingUser"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
	///[Authorize(Roles = RoleHelper.SystemOwner + "," + RoleHelper.UserAdmin)]
	[Route("UserManager/[controller]/[action]/"), AuthorizeUserAdmin]
	public class PendingUserController<TPendingUser, TKey> : Controller
		where TPendingUser : PendingUser<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly BaseEntityRepository<TPendingUser> PendingUserRepository;

		protected readonly ILogger<PendingUserController<TPendingUser, TKey>> Logger;

		public PendingUserController(
			BaseEntityRepository<TPendingUser> pendingUserRepository,
			ILogger<PendingUserController<TPendingUser, TKey>> logger)
		{
			try
			{
				//Check parameters for null values
				if (pendingUserRepository != null
					&& logger != null)
				{
					//Set fields
					this.PendingUserRepository = pendingUserRepository;

					this.Logger = logger;
				}
				else
				{
					throw new ArgumentNullException(nameof(pendingUserRepository));
				}
			}
			catch (Exception ex)
			{
				string Message = this.FormatExceptionMessage("Creation of Controller Failed");

				throw new Exception(Message, ex);
			}
		}

		[NonAction]
		protected string FormatLogMessage(string message, Microsoft.AspNetCore.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		//Todo: revamp this hardcore
		[NonAction]
		protected string FormatExceptionMessage(string message)
		{
			return (this.GetType().Name + ": " + message + ": ");
		}

		[NonAction]
		public bool HasUserAlreadyBeenApproved(TPendingUser user)
		{
			return user.HasUserBeenAdded;
		}

		//todo test if user has already been approved, then not allow changes
		//
		public enum Message
		{
			//approved denied actions
			UserApproved,

			UserDenied,

			//RoleChanged
			UserRoleChanged,

			UserRoleCouldnotBeChanged,

			//Restricted actions
			UserHasAlreadyBeenApproved,

			//errors
			UserNotFound,

			ErrorProcessingRequest
		}

		private List<IUserIndexView> convertToViewModelList(IEnumerable<TPendingUser> pendingUsers, bool showDetails)
		{
			var viewModelList = new List<IUserIndexView>();

			if (pendingUsers != null)
			{
				foreach (var pendingUser in pendingUsers)
				{
					viewModelList.Add(new PendingUserViewModel<TKey, TPendingUser>(pendingUser)
					{
						ShowDetails = showDetails
					});
				}
			}

			return viewModelList;
		}

		[HttpGet]
		public async Task<IActionResult> Index(Message? message)
		{
			//Serve index view with all pending users loaded
			try
			{
				if (message != null)
				{
					switch (message.Value)
					{
						case Message.UserApproved:
							break;

						case Message.UserDenied:
							break;

						case Message.UserRoleChanged:
							break;

						case Message.UserRoleCouldnotBeChanged:
							break;

						case Message.UserHasAlreadyBeenApproved:
							break;

						case Message.UserNotFound:
							break;

						case Message.ErrorProcessingRequest:
							break;

						default:
							break;
					}
				}

				//todo change to index view model
				var viewList = new List<PageViewModel>
				{
					new PageViewModel(this)
					{
						Title = "Added Pending Users",
						Description = "Pending Users that have been approved",
						Message = new MessageViewModel(),
						Data = convertToViewModelList((await PendingUserRepository.GetAll()).Where(x => x.HasUserBeenAdded == true), false)
					},
					new PageViewModel(this)
					{
						Title = "Pending Users",
						Description = "All Pending Users in the Database",
						Message = new MessageViewModel(),
						Data = convertToViewModelList((await PendingUserRepository.GetAll()).Where(x => x.HasUserBeenAdded == false), true)
					},
				};

				/*
				PageViewModel tableViewModel = new PageViewModel()
				{
					ControllerName = GetControllerName(this.GetType()),
					Title = "Pending Users",
					Action = "Index",
					Description = "All Pending Users in the Database",
					CreateButton = false,
					NestedView = "PendingUserIndex"
				};
				*/

				return View("~/Views/Shared/MultiPageContainer.cshtml", viewList);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//check for has been added... and don't return
		[HttpGet("{id}")]
		public async Task<IActionResult> Details(TKey id, Message? message)
		{
			try
			{
				if (id != null)
				{
					if (message != null)
					{
						switch (message.Value)
						{
							case Message.UserApproved: //don't need
								break;

							case Message.UserDenied: //don't need
								break;

							case Message.UserRoleChanged:
								break;

							case Message.UserRoleCouldnotBeChanged:
								break;

							case Message.UserHasAlreadyBeenApproved: //don't need
								break;

							case Message.UserNotFound: //don't need
								break;

							case Message.ErrorProcessingRequest:
								break;

							default:
								break;
						}
					}

					var pendingUser = await PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", id));

					if (pendingUser != null)
					{
						if (this.HasUserAlreadyBeenApproved(pendingUser))
						{
							return RedirectToAction(nameof(this.Index), new { message = Message.UserHasAlreadyBeenApproved });
						}

						ViewData["Roles"] = RoleHelper.SelectableRoleList();

						//return view with pending user details view model
						return View("~/Views/UserManager/PendingUserDetails.cshtml", new PendingUserDetails<TKey, TPendingUser>(pendingUser));
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Message.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed Pending User Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: add status messages
		//todo: this change role or redirect back to detailed action
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePendingUserRole(RoleChangeViewModel roleChange)
		{
			try
			{
				if (roleChange != null && roleChange.IsValid && ModelState.IsValid)
				{
					var pendingUser = await
						this.PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", roleChange.UserId));

					if (pendingUser != null)
					{
						pendingUser.RequestedRole = roleChange.NewRole;

						var updateResult = await this.PendingUserRepository.Update(pendingUser);
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserNotFound });
					}

					return RedirectToAction(nameof(this.Details), new { id = roleChange.UserId });
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { Message.ErrorProcessingRequest });
				}
			}
			catch (Exception ex)
			{
				string Message = "Change Pending User Role Failed";

				this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		//todo: email Notification
		/// <summary>
		/// Approves the user.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpPost("{id}"), ValidateAntiForgeryToken]
		public async Task<IActionResult> ApproveUser(TKey id)
		{
			//create user account
			//send email stating request approved
			try
			{
				if (id != null)
				{
					var pendingUser = await
						PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", id));

					if (pendingUser != null)
					{
						pendingUser.HasUserBeenAdded = true;

						var result = await PendingUserRepository.Update(pendingUser);

						if (result != null)
						{
							//todo: change status message, to user request approved
							return RedirectToAction(nameof(this.Index));
							//new { ManageMessageId = ManageMessageId.UserAccountCreationApproved });
						}
						else
						{
							throw new Exception("Failed updating pending user");
						}
					}
					else
					{
						return NotFound();
					}
				}
				else
				{
					return BadRequest();
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
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> DenyUser(TKey id) //todo: add reason why
		{
			//todo: send email stating request denied
			try             //delete user account request
			{
				if (id != null)
				{
					var user = await PendingUserRepository.Get(PendingUserRepository.IsMatchedExpression("Id", id));

					if (user != null)
					{
						var result = await PendingUserRepository.Delete(user);

						if (result != false)
						{
							//todo: add status message successfully pending user deleted
							return RedirectToAction(nameof(this.Index));
						}
						else
						{
							return BadRequest();
						}
					}
					else
					{
						return NotFound();
					}
				}
				else
				{
					return BadRequest();
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