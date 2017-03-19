using GenericMvc.Attributes;
using GenericMvc.Models;
using GenericMvc.Repositories;
using GenericMvc.ViewModels.Basic;
using GenericMvc.ViewModels.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.UserManager
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
		protected readonly EntityRepository<TPendingUser> PendingUserRepository;

		protected readonly ILogger<PendingUserController<TPendingUser, TKey>> Logger;

		public PendingUserController(
			EntityRepository<TPendingUser> pendingUserRepository,
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
		public enum Message : byte
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

			InvalidQueryOrRequest,

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
				MessageViewModel messageViewModel = null;

				if (message != null)
				{
					switch (message.Value)
					{
						case Message.UserApproved:
							messageViewModel = new MessageViewModel(MessageType.Success, "The User's request has been successfully approved");
							break;

						case Message.UserDenied:
							messageViewModel = new MessageViewModel(MessageType.Success, "The User's request had been successfully denied");
							break;

						case Message.UserHasAlreadyBeenApproved:
							messageViewModel = new MessageViewModel(MessageType.Warning, "This user has already been approved, no further action can be taken");
							break;

						case Message.UserNotFound:
							messageViewModel = new MessageViewModel(MessageType.Warning, "The specified user account cannot be found");
							break;

						case Message.InvalidQueryOrRequest:
							messageViewModel = new MessageViewModel(MessageType.Danger, "Either the request or the URL query is invalid");
							break;

						case Message.ErrorProcessingRequest:
							messageViewModel = new MessageViewModel(MessageType.Danger, "The System encountered an error processing your request");
							break;

						default:
							messageViewModel = new MessageViewModel();
							break;
					}
				}

				//todo change to index view model
				var viewList = new List<PageViewModel>
				{
					new PageViewModel(this)
					{
						ControllerName = "UserManager/PendingUser",
						Title = "Approved Pending Users",
						Description = "Pending Users that have been approved",
						Message = messageViewModel ?? new MessageViewModel(),
						Data = convertToViewModelList((await PendingUserRepository.GetAll()).Where(x => x.HasUserBeenAdded == true), false)
					},
					new PageViewModel(this)
					{
						ControllerName = "UserManager/PendingUser",
						Title = "Pending Users",
						Description = "All Pending Users in the Database",
						Message = new MessageViewModel(),
						Data = convertToViewModelList((await PendingUserRepository.GetAll()).Where(x => x.HasUserBeenAdded == false), true)
					},
				};

				return View("~/Views/Shared/MultiPageContainer.cshtml", viewList);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		/*
		public MessageViewModel GetMessageForDetails(Message? message)
		{
			MessageViewModel messageViewModel = null;

			if (message != null)
			{
				switch (message.Value)
				{
					case Message.UserRoleChanged:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Success,
							Text = "Pending User's Role has been Successfully changed"
						};
						break;
					case Message.UserRoleCouldnotBeChanged:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "Pending User's Role could not be changed"
						};
						break;
					case Message.ErrorProcessingRequest:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "The System could not process your request"
						};
						break;
					default:
						messageViewModel = new MessageViewModel();
						break;
				}
			}

			return messageViewModel;
		}
*/

		//check for has been added... and don't return
		[HttpGet("{id}")]
		public async Task<IActionResult> Details(TKey id, Message? message)
		{
			try
			{
				if (id != null && ModelState.IsValid)
				{
					var pendingUser = await PendingUserRepository.Get(PendingUserRepository.MatchByIdExpression(id));

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
				string errorMessage = "Detailed Pending User Failed";

				this.Logger.LogError(this.FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(this.FormatExceptionMessage(Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Message.ErrorProcessingRequest });
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
					var typeConverter = TypeDescriptor.GetConverter(typeof(TKey));

					TKey convertedValue = (TKey)typeConverter.ConvertFromString(roleChange.UserId);

					var pendingUser = await
						this.PendingUserRepository.Get(PendingUserRepository.MatchByIdExpression(convertedValue));

					if (this.HasUserAlreadyBeenApproved(pendingUser))
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserHasAlreadyBeenApproved });
					}

					if (pendingUser != null)
					{
						pendingUser.RequestedRole = roleChange.NewRole;

						var updateResult = await this.PendingUserRepository.Update(pendingUser);

						if (updateResult != null)
						{
							return RedirectToAction(nameof(this.Details), new { id = roleChange.UserId, message = Message.UserRoleChanged });
						}
						else
						{
							throw new Exception("Could not change user's role");
						}
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Message.InvalidQueryOrRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Change Pending User Role Failed";

				this.Logger.LogError(this.FormatLogMessage(errorMessage, this.Request), ex);

				return RedirectToAction(nameof(this.Index), new { message = Message.ErrorProcessingRequest });
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
				if (id != null && ModelState.IsValid)
				{
					var pendingUser = await
						PendingUserRepository.Get(PendingUserRepository.MatchByIdExpression(id));

					if (this.HasUserAlreadyBeenApproved(pendingUser))
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserHasAlreadyBeenApproved });
					}

					if (pendingUser != null)
					{
						pendingUser.HasUserBeenAdded = true;

						var result = await PendingUserRepository.Update(pendingUser);

						if (result != null)
						{
							return RedirectToAction(nameof(this.Index), new { message = Message.UserApproved});
						}
						else
						{
							throw new Exception("Failed updating pending user");
						}
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Message.InvalidQueryOrRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Approving Pending User Failed";

				this.Logger.LogError(this.FormatLogMessage(errorMessage, this.Request), ex);

				return RedirectToAction(nameof(this.Index), new { message = Message.ErrorProcessingRequest });
			}
		}

		//delete user account request
		//todo: email Notification
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> DenyUser(TKey id)
		{
			try            
			{
				if (id != null && ModelState.IsValid)
				{
					var user = await PendingUserRepository.Get(PendingUserRepository.MatchByIdExpression(id));

					if (this.HasUserAlreadyBeenApproved(user))
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserHasAlreadyBeenApproved });
					}

					if (user != null)
					{
						var result = await PendingUserRepository.Delete(user);

						if (result != false)
						{
							return RedirectToAction(nameof(this.Index), new { message = Message.UserDenied });
						}
						else
						{
							throw new Exception("Could not delete/remove user");
						}
					}
					else
					{
						return RedirectToAction(nameof(this.Index), new { message = Message.UserNotFound });
					}
				}
				else
				{
					return RedirectToAction(nameof(this.Index), new { message = Message.InvalidQueryOrRequest });
				}
			}
			catch (Exception ex)
			{
				string errorMessage = "Deleting Pending User by Id Failed";

				this.Logger.LogError(this.FormatLogMessage(errorMessage, this.Request), ex);

				//throw new Exception(this.FormatExceptionMessage(Message), ex);
				return RedirectToAction(nameof(this.Index), new { message = Message.ErrorProcessingRequest });
			}
		}
	}
}