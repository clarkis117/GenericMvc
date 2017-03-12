using GenericMvc.Attributes;
using GenericMvc.Models;
using GenericMvc.Models.ViewModels;
using GenericMvc.Repositories;
using GenericMvc.Services;
using GenericMvc.ViewModels.Basic;
using GenericMvc.ViewModels.UserManager.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

//todo change login view model in this case to privildge login view model, to elminate confusion
	//in other words create a new model
//Todo: add in account confirmation phase for added pending users
//todo: add generic mvc views modeling to it for bootstrap material design
namespace GenericMvc.UserManager
{
	[Authorize, Route("[controller]/[action]/")]
	public class AccountController<TKey, TUser, TPendingUser> : Controller
		where TKey : IEquatable<TKey>
		where TUser : IdentityUser<TKey>, IPrivilegedUserConstraints, new()
		where TPendingUser : PendingUser<TKey>, new()
	{
		private readonly UserManager<TUser> _userManager;
		private readonly SignInManager<TUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ISmsSender _smsSender;
		private readonly BaseEntityRepository<TUser> _userRepository;
		private readonly BaseEntityRepository<TPendingUser> _pendingUserRepository;
		private readonly PasswordHasher<TPendingUser> _passwordHasher;

		private static bool _databaseChecked;

		public AccountController(
			UserManager<TUser> userManager,
			SignInManager<TUser> signInManager,
			IEmailSender emailSender,
			ISmsSender smsSender,
			BaseEntityRepository<TUser> userRepository,
			BaseEntityRepository<TPendingUser> pendingUserRepository,
			PasswordHasher<TPendingUser> passwordHasher)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_smsSender = smsSender;
			_userRepository = userRepository;
			_pendingUserRepository = pendingUserRepository;
			_passwordHasher = passwordHasher;
		}

		// GET: /Account/Login
		[HttpGet, AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			//set return URL if we have one
			if (returnUrl != null)
			{
				ViewData["ReturnUrl"] = returnUrl;
			}

			return View();
		}

		//todo check for system owner... and default password post
		// POST: /Account/Login
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			//early out for bad requests
			if (model == null || !ModelState.IsValid)
				return View(model);

			EnsureDatabaseCreated(_userRepository.DataContext);

			//set return URL if we have one
			if (returnUrl != null)
			{
				ViewData["ReturnUrl"] = returnUrl;
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.IsPersistent, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				//system owner default filter goes here
				if (await DefaultSystemOwnerFilter(model))
				{
					//redirect to system owner defaults change action
					return RedirectToAction(nameof(this.SysOwnerChangeDefaults));
				}

				return RedirectToLocal(returnUrl);
			}
			else if (result.RequiresTwoFactor)
			{
				return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.IsPersistent });
			}
			else if (result.IsLockedOut)
			{
				return View("Lockout");
			}
			//begin check for pending user
			else if (result.Succeeded == false
					&& result.IsLockedOut == false
					&& result.RequiresTwoFactor == false)
			{
				//Check for pending user here, query by email address for pending user
				var pendingUser = await _pendingUserRepository.Get(x => x.Email == model.UserName);

				//check for result and correctness of result
				if (pendingUser != null)
				{
					//check to see if the user has been added
					if (pendingUser.HasUserBeenAdded)
					{
						var passwordResult = _passwordHasher.VerifyHashedPassword(pendingUser, pendingUser.HashedPassword, model.Password);

						if (passwordResult == PasswordVerificationResult.Success || passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
						{
							//create one-time authentication cookie
							var token = new OneTimeToken(OneTimeToken.GetBytes(pendingUser.HashedPassword));

							//set token expiration date
							pendingUser.StampExpiration = token.ExpirationDate;

							//set token random number stamp for validation
							pendingUser.SecurityStamp = token.TokenStamp;

							var stampResult = await _pendingUserRepository.Update(pendingUser); ;

							if (stampResult != null)
							{
								return RedirectToAction(nameof(this.ConfirmUser),
									new { PendingUserId = pendingUser.Id, secToken = await token.GenerateToken() });
							}
							else
							{
								throw new Exception("Updating Pending User Failed");
							}
						}
					}
				}
			}

			ModelState.AddModelError(string.Empty, "Invalid login attempt.");

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		#region System Owner Actions

		[NonAction]
		private async Task<bool> DefaultSystemOwnerFilter(LoginViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.UserName);
			//var user = await GetCurrentUserAsync();

			if (await _userManager.IsInRoleAsync(user, RoleHelper.SystemOwner))
			{
				if (user.Email == SystemOwnerDefaults.UserNameEmail || model.Password == SystemOwnerDefaults.Password)
				{
					return true;
				}
			}

			return false;
		}

		[NonAction]
		private static PageViewModel CreateSysOwnerDefaultsViewModel(Controller controller, SysOwnerChangeDefaultsViewModel viewModel = null)
		{
			return new PageViewModel(controller)
			{
				Title = "Change System Owner Account Defaults",
				Description = "Please Fill this out to Change the Defaults for this Account",
				Data = viewModel ?? new SysOwnerChangeDefaultsViewModel(),
				Message = new MessageViewModel(MessageType.Danger,
				"If you don't fill this out and submit it, the System will be vulnerable.", false)
			};
		}

		[HttpGet, AuthorizeSystemOwnerOnly]
		public async Task<IActionResult> SysOwnerChangeDefaults(string returnUrl)
		{
			var user = await GetCurrentUserAsync();

			//if user is not valid log them out and redirect
			if (!(await _userManager.IsInRoleAsync(user, RoleHelper.SystemOwner)))
			{
				await _signInManager.SignOutAsync();

				return RedirectToActionPermanent(nameof(this.Login));
			}

			//set return URL if we have one
			if (returnUrl != null)
			{
				ViewData["ReturnUrl"] = returnUrl;
			}

			return this.ViewFromModel(CreateSysOwnerDefaultsViewModel(this));
		}

		[HttpPost, ValidateAntiForgeryToken, AuthorizeSystemOwnerOnly]
		public async Task<IActionResult> SysOwnerChangeDefaults(SysOwnerChangeDefaultsViewModel viewModel, string returnUrl = null)
		{
			var user = await GetCurrentUserAsync();

			//if invalid redisplay form
			if (viewModel == null && !ModelState.IsValid)
				return this.ViewFromModel(CreateSysOwnerDefaultsViewModel(this, viewModel));

			//if user is not valid log them out and redirect
			if (!(await _userManager.IsInRoleAsync(user, RoleHelper.SystemOwner)))
			{
				await _signInManager.SignOutAsync();

				return RedirectToActionPermanent(nameof(this.Login));
			}

			//set return URL if we have one
			if (returnUrl != null)
			{
				ViewData["ReturnUrl"] = returnUrl;
			}

			//attempt to change password
			var changePassResult = await _userManager.ChangePasswordAsync(user, viewModel.CurrentPassword, viewModel.NewPassword);

			if (changePassResult.Succeeded)
			{
				user.FirstName = viewModel.FirstName;

				user.LastName = viewModel.LastName;

				user.PhoneNumber = viewModel.NewPhoneNumber;

				var updatedUser = await _userRepository.Update(user);

				//change email address
				var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(updatedUser, viewModel.NewEmail);

				var changeEmailResult = await _userManager.ChangeEmailAsync(updatedUser, viewModel.NewEmail, changeEmailToken);

				//change user name
				var changeUserNameResult = await _userManager.SetUserNameAsync(updatedUser, viewModel.NewEmail);

				//both of these must succeed
				if (changeEmailResult.Succeeded && changeUserNameResult.Succeeded)
				{
					//final success path
					if (returnUrl != null)
					{
						return LocalRedirect(returnUrl);
					}
					else
					{
						return RedirectToActionPermanent("Index", "Home");
					}
				}

				AddErrors(changeEmailResult);

				AddErrors(changeUserNameResult);
			}

			AddErrors(changePassResult);

			return this.ViewFromModel(CreateSysOwnerDefaultsViewModel(this, viewModel));
		}

		#endregion System Owner Actions

		#region Pending User Actions

		/// <summary>
		/// Utility Method for creating confirm user view model.
		/// </summary>
		/// <param name="pendingUser">The pending user.</param>
		/// <returns></returns>
		private static ConfirmUserViewModel CreateConfirmUserViewModel(TPendingUser pendingUser, string token)
		{
			return new ConfirmUserViewModel()
			{
				PendingUserId = pendingUser.Id.ToString(),
				AuthToken = token,
				FirstName = pendingUser.FirstName,
				LastName = pendingUser.LastName,
				Email = pendingUser.Email,
				AssignedRole = pendingUser.RequestedRole,
				PhoneNumber = pendingUser.PhoneNumber
			};
		}

		/// <summary>
		/// Users from pending.
		/// </summary>
		/// <param name="PendingUser">The pending user.</param>
		/// <returns></returns>
		private static TUser UserFromPending(TPendingUser PendingUser)
		{
			return new TUser()
			{
				FirstName = PendingUser.FirstName,
				LastName = PendingUser.LastName,
				UserName = PendingUser.Email,
				PhoneNumber = PendingUser.PhoneNumber,
				Email = PendingUser.Email,
				DateRegistered = PendingUser.DateRegistered,
			};
		}

		private static PageViewModel getConfirmViewModel(ConfirmUserViewModel model, Controller controller)
		{
			//construct view model
			return new PageViewModel(controller)
			{
				Title = "Confirm your Account",
				Description = "Your Request has been reviewed by a User Administrator and approved, please confirm your information.",
				Data = model
			};
		}

		private static PageViewModel getConfirmViewModel(TPendingUser pending, string token, Controller controller)
		{
			//construct view model
			return new PageViewModel(controller)
			{
				Title = "Confirm your Account",
				Description = "Your Request has been reviewed by a User Administrator and approved, please confirm your information.",
				Data = CreateConfirmUserViewModel(pending, token)
			};
		}

		/// <summary>
		/// Serves the confirmation view
		/// verify ids, check if user has been added, create model, assign ids to model, send view
		/// </summary>
		/// <param name="pendingUserId">The pending user identifier.</param>
		/// <param name="userId">The user identifier.</param>
		/// <returns></returns>
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> ConfirmUser(TKey pendingUserId, string secToken)
		{
			if (pendingUserId == null || secToken == null)
				return RedirectToActionPermanent(nameof(this.Login));

			var pendingUser = await
				_pendingUserRepository.Get(_pendingUserRepository.MatchByIdExpression<TPendingUser,TKey>(pendingUserId));

			//create token for comparison
			var token = new OneTimeToken(OneTimeToken.GetBytes(pendingUser.HashedPassword),
					pendingUser.SecurityStamp,
					pendingUser.StampExpiration);

			if (pendingUser != null
				&& pendingUser.HasUserBeenAdded
				&& await token.VerifyToken(secToken, pendingUser.StampExpiration))
			{
				return this.ViewFromModel(getConfirmViewModel(pendingUser, secToken, this));
			}

			return RedirectToActionPermanent(nameof(this.Login));
		}

		/// <summary>
		/// Receives form post, verify model state, check data
		/// </summary>
		/// <param name="viewModel">The model.</param>
		/// <returns></returns>
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmUser(ConfirmUserViewModel viewModel)
		{
			if (viewModel == null || !ModelState.IsValid)
				return this.ViewFromModel(getConfirmViewModel(viewModel, this));

			var typeConverter = TypeDescriptor.GetConverter(typeof(TKey));

			TKey pendingUserId = (TKey)typeConverter.ConvertFromString(viewModel.PendingUserId);

			//get pending user from DB
			var pendingUser = await
				_pendingUserRepository.Get(_pendingUserRepository.MatchByIdExpression<TPendingUser, TKey>(pendingUserId));

			//create token for comparison
			var token = new OneTimeToken(OneTimeToken.GetBytes(pendingUser.HashedPassword),
					pendingUser.SecurityStamp,
					pendingUser.StampExpiration);

			if (!(await token.VerifyToken(viewModel.AuthToken, pendingUser.StampExpiration)))
				return RedirectToActionPermanent(nameof(this.Login));

			//hash old password and compare with stored hash
			var passwordResult = _passwordHasher.VerifyHashedPassword(pendingUser, pendingUser.HashedPassword, viewModel.Password);

			//if password result fails, redirect to login
			if (passwordResult == PasswordVerificationResult.Success || passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
				return RedirectToActionPermanent(nameof(this.Login));

			//then add user to the system
			TUser newUser = UserFromPending(pendingUser);

			//Add pending user to user system, with password
			var createUserResult = await _userManager.CreateAsync(newUser, viewModel.NewPassword);

			//if user created then add to role
			if (createUserResult.Succeeded) 
			{
				var roleResult = await _userManager.AddToRoleAsync(newUser, pendingUser.RequestedRole);

				//if role succeeded sign in user
				if (roleResult.Succeeded)
				{
					//sign user in and redirect to home page
					var signInResult =
						await _signInManager.PasswordSignInAsync(newUser, viewModel.NewPassword, false, false);

					//if sign in delete pending user 
					if (signInResult.Succeeded)
					{
						await _pendingUserRepository.Delete(pendingUser);

						return RedirectToAction("Index", "Home");
					}
					else
					{
						throw new Exception("Failed signing in new user");
					}
				}

				AddErrors(roleResult);
			}

			//if password errors redisplay form with data and errors
			AddErrors(createUserResult);

			return this.ViewFromModel(getConfirmViewModel(pendingUser, viewModel.AuthToken, this));
		}

		#endregion Pending User Actions

		// GET: /Account/Register
		[HttpGet, AllowAnonymous]
		public IActionResult Register()
		{
			//Add Roles to view bag / view data
			ViewData["Roles"] = RoleHelper.SelectableRoleList();

			var viewModel = new PageViewModel(this)
			{
				Title = "Request Access",
				Description = "Create a new account request. The account will be reviewed by a User Administrator before being added to the System.",
			};

			return this.ViewFromModel(viewModel);
		}

		/// <summary>
		/// Creates the pending user.
		/// Utility method for creating pending user models from registration model
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Error hashing pending user password</exception>
		[NonAction]
		private TPendingUser CreatePendingUser(PrivilegedRegisterViewModel model)
		{
			var pendingUser = new TPendingUser()
			{
				FirstName = model.FirstName,
				LastName = model.LastName,
				Email = model.Email,
				PhoneNumber = model.PhoneNumber,
				RequestedRole = model.RequestedRole,
				DateRegistered = model.DateRegistered,
			};

			var hashedpassword = _passwordHasher.HashPassword(pendingUser, model.Password);

			if (hashedpassword != null)
			{
				pendingUser.HashedPassword = hashedpassword;
			}
			else
			{
				throw new Exception("Error hashing pending user password");
			}

			return pendingUser;
		}

		[HttpGet, AllowAnonymous]
		public IActionResult ConfirmRegistration()
		{
			var page = new PageViewModel(this)
			{
				Title = "Registration Confirmation",
				Description = "Your Request for Access has been submitted for approval.",
				DisplayAction = false
			};

			return this.ViewFromModel(page);
		}

		// POST: /Account/Register
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(PrivilegedRegisterViewModel model)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);

			if (ModelState.IsValid)
			{
				var pendingUser = CreatePendingUser(model);

				//var result = await _userManager.CreateAsync(user, model.Password);

				var result = await _pendingUserRepository.Create(pendingUser);

				if (result != null)
				{
					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
					// Send an email with this link
					//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					//var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
					//await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
					//    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
					//await _signInManager.SignInAsync(user, isPersistent: false);

					return RedirectToAction("ConfirmRegistration", "Account");
				}
				else
				{
					ModelState.AddModelError(String.Empty, "Failed Inserting Pending User into DB");
				}

				//AddErrors(result);
			}

			//Fix redisplay of form with generic mvc container
			//Add Roles to view bag / view data
			ViewData["Roles"] = RoleHelper.SelectableRoleList();

			var viewModel = new PageViewModel(this)
			{
				Title = "Request Access",
				Description = "Create a new account request. The account will be reviewed by a User Administrator before being added to the System.",
				Data = model
			};

			// If we got this far, something failed, redisplay form
			return this.ViewFromModel(viewModel);
		}

		// POST: /Account/LogOff
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> LogOff()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		// POST: /Account/ExternalLogin
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		// GET: /Account/ExternalLoginCallback
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
		{
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return RedirectToAction(nameof(Login));
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
			if (result.Succeeded)
			{
				return RedirectToLocal(returnUrl);
			}
			if (result.RequiresTwoFactor)
			{
				return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
			}
			if (result.IsLockedOut)
			{
				return View("Lockout");
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ViewData["ReturnUrl"] = returnUrl;
				ViewData["LoginProvider"] = info.LoginProvider;
				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
			}
		}

		// POST: /Account/ExternalLoginConfirmation
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
		{
			if (_signInManager.IsSignedIn(User))
			{
				return RedirectToAction("Index", "Manage");
			}

			if (ModelState.IsValid)
			{
				// Get the information about the user from the external login provider
				var info = await _signInManager.GetExternalLoginInfoAsync();
				if (info == null)
				{
					return View("ExternalLoginFailure");
				}
				var user = new TUser { UserName = model.Email, Email = model.Email };
				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						await _signInManager.SignInAsync(user, isPersistent: false);
						return RedirectToLocal(returnUrl);
					}
				}
				AddErrors(result);
			}

			ViewData["ReturnUrl"] = returnUrl;
			return View(model);
		}

		// GET: /Account/ConfirmEmail
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return View("Error");
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return View("Error");
			}
			var result = await _userManager.ConfirmEmailAsync(user, code);
			return View(result.Succeeded ? "ConfirmEmail" : "Error");
		}

		// GET: /Account/ForgotPassword
		[HttpGet, AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		// POST: /Account/ForgotPassword
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(model.Email);
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				}

				// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
				// Send an email with this link
				//var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				//var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
				//await _emailSender.SendEmailAsync(model.Email, "Reset Password",
				//   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
				//return View("ForgotPasswordConfirmation");
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		// GET: /Account/ForgotPasswordConfirmation
		[HttpGet, AllowAnonymous]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		// GET: /Account/ResetPassword
		[HttpGet, AllowAnonymous]
		public IActionResult ResetPassword(string code = null)
		{
			return code == null ? View("Error") : View();
		}

		// POST: /Account/ResetPassword
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await _userManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			AddErrors(result);
			return View();
		}

		// GET: /Account/ResetPasswordConfirmation
		[HttpGet, AllowAnonymous]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		// GET: /Account/SendCode
		[HttpGet, AllowAnonymous]
		public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
		{
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
			var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
			return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		// POST: /Account/SendCode
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> SendCode(SendCodeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}

			// Generate the token and send it
			var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
			if (string.IsNullOrWhiteSpace(code))
			{
				return View("Error");
			}

			var message = "Your security code is: " + code;
			if (model.SelectedProvider == "Email")
			{
				await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
			}
			else if (model.SelectedProvider == "Phone")
			{
				await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
			}

			return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
		}

		// GET: /Account/VerifyCode
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
		{
			// Require that the user has already logged in via username/password or external login
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		// POST: /Account/VerifyCode
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// The following code protects for brute force attacks against the two factor codes.
			// If a user enters incorrect codes for a specified amount of time then the user account
			// will be locked out for a specified amount of time.
			var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
			if (result.Succeeded)
			{
				return RedirectToLocal(model.ReturnUrl);
			}
			if (result.IsLockedOut)
			{
				return View("Lockout");
			}
			else
			{
				ModelState.AddModelError("", "Invalid code.");
				return View(model);
			}
		}

		#region Helpers

		// The following code creates the database and schema if they don't exist.
		// This is a temporary workaround since deploying database through EF migrations is
		// not yet supported in this release.
		// Please see this http://go.microsoft.com/fwlink/?LinkID=615859 for more information on how to do deploy the database
		// when publishing your application.
		private static void EnsureDatabaseCreated(DbContext context)
		{
			if (!_databaseChecked)
			{
				_databaseChecked = true;
				context.Database.Migrate();
			}
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		private async Task<TUser> GetCurrentUserAsync()
		{
			return await _userManager.FindByIdAsync(this._userManager.GetUserId(this.User));
		}

		private IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

		#endregion Helpers
	}
}