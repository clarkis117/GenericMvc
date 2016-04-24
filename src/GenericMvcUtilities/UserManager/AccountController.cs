﻿using GenericMvcUtilities.Models;
using GenericMvcUtilities.ViewModels.Generic;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.Services;
using GenericMvcUtilities.ViewModels.UserManager.Account;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;

//Todo: add in account confirmation phase for added pending users
//todo: add generic mvc views modeling to it for bootstrap material design
namespace GenericMvcUtilities.UserManager
{
	[Authorize]
	[Route("[controller]/[action]/")]
	public class AccountController<TKey, TUser, TPendingUser> : Controller
		where TKey : IEquatable<TKey>
		where TUser : IdentityUser<TKey>, IUserConstraints, new()
		where TPendingUser : PendingUser<TKey>, new()
	{
		private readonly UserManager<TUser> _userManager;
		private readonly SignInManager<TUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ISmsSender _smsSender;

		private readonly BaseRepositroy<TUser> _userRepository;

		private readonly BaseRepositroy<TPendingUser> _pendingUserRepository;

		private readonly PasswordHasher<TPendingUser> _passwordHasher;

		private static bool _databaseChecked;

		public AccountController(
			UserManager<TUser> userManager,
			SignInManager<TUser> signInManager,
			IEmailSender emailSender,
			ISmsSender smsSender,
			BaseRepositroy<TUser> userRepository,
			BaseRepositroy<TPendingUser> pendingUserRepository,
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

		//
		// GET: /Account/Login
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		/// <summary>
		/// Utility Method for creating confirm user view model.
		/// </summary>
		/// <param name="pendingUser">The pending user.</param>
		/// <returns></returns>
		private ConfirmUserViewModel CreateConfirmUserViewModel(TPendingUser pendingUser, string token)
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
		private TUser UserFromPending(TPendingUser PendingUser)
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

		/// <summary>
		/// Serves the confirmation view
		/// verify ids, check if user has been added, create model, assign ids to model, send view
		/// </summary>
		/// <param name="pendingUserId">The pending user identifier.</param>
		/// <param name="userId">The user identifier.</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> ConfirmUser(TKey pendingUserId, string secToken)
		{
			if (pendingUserId != null)
			{
				var pendingUser = await
					_pendingUserRepository.Get(_pendingUserRepository.IsMatchedExpression("Id", pendingUserId));

				//create token for comparison 
				var token = new OneTimeToken(OneTimeToken.GetBytes(pendingUser.HashedPassword),
						pendingUser.SecurityStamp,
						pendingUser.StampExpiration);

				if (pendingUser != null &&
					pendingUser.HasUserBeenAdded &&
					await token.VerifyToken(secToken, pendingUser.StampExpiration))
				{
					return this.ViewFromModel(getConfirmViewModel(pendingUser, secToken));
				}
				else
				{
					//HttpNotFound and anything else leaks information
					return HttpBadRequest();
				}
			}
			else
			{
				return HttpBadRequest();
			}
		}

		private PageViewModel getConfirmViewModel(ConfirmUserViewModel model)
		{
			//construct view model
			return new PageViewModel(ActionContext)
			{
				Title = "Confirm your Account",
				Description = "Your Request has been reviewed by a User Administrator and approved, please confirm your information.",
				Data = model
			};
		}

		private PageViewModel getConfirmViewModel(TPendingUser pending, string token)
		{
			//construct view model
			return new PageViewModel(ActionContext)
			{
				Title = "Confirm your Account",
				Description = "Your Request has been reviewed by a User Administrator and approved, please confirm your information.",
				Data = CreateConfirmUserViewModel(pending, token)
			};
		}

		/// <summary>
		/// Receives form post, verify model state, check data
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmUser(ConfirmUserViewModel model)
		{
			if (model != null)
			{
				if (ModelState.IsValid)
				{
					//get pending user from DB
					var pendingUser = await 
						_pendingUserRepository.Get(_pendingUserRepository.IsMatchedExpression("Id", model.PendingUserId));

					//create token for comparison 
					var token = new OneTimeToken(OneTimeToken.GetBytes(pendingUser.HashedPassword),
							pendingUser.SecurityStamp,
							pendingUser.StampExpiration);

					if ((await token.VerifyToken(model.AuthToken, pendingUser.StampExpiration)) == false)
					{
						return HttpUnauthorized();
					}

					var passwordResult = _passwordHasher.VerifyHashedPassword(pendingUser, pendingUser.HashedPassword, model.Password);

					//hash old password and compare with stored hash
					if (passwordResult == PasswordVerificationResult.Success || passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
					{
						//then add user to the system
						TUser newUser = UserFromPending(pendingUser);

						//Add pending user to user system, with password
						var result = await _userManager.CreateAsync(newUser, model.NewPassword);

						if (result.Succeeded) //then add role
						{
							var roleResult = await _userManager.AddToRoleAsync(newUser, pendingUser.RequestedRole);

							if (roleResult.Succeeded)
							{
								//sign user in and redirect to home page
								var signInResult =
									await _signInManager.PasswordSignInAsync(newUser, model.NewPassword, false, false);

								if (signInResult.Succeeded)
								{
									//todo test this
									await _pendingUserRepository.Delete(pendingUser);

									return RedirectToAction("Index", "Home");
								}
								else
								{
									throw new Exception("Failed signing in new user");
								}
							}
						}

						//if password errors redisplay form with data and errors
						AddErrors(result);

						return this.ViewFromModel(getConfirmViewModel(pendingUser, model.AuthToken));
					}
					else
					{
						return HttpUnauthorized();
					}
				}
				else
				{
					//return HttpBadRequest(ModelState);

					return this.ViewFromModel(getConfirmViewModel(model));
				}
			}
			else
			{
				return HttpBadRequest();
			}
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);

			if (returnUrl != null)
			{
				ViewData["ReturnUrl"] = returnUrl;
			}

			if (ModelState.IsValid)
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

				if (result.Succeeded)
				{
					return RedirectToLocal(returnUrl);
				}
				if (result.RequiresTwoFactor)
				{
					return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				}
				if (result.IsLockedOut)
				{
					return View("Lockout");
				}
				//begin check for pending user
				if (result.Succeeded == false &&
					result.IsLockedOut == false &&
					result.RequiresTwoFactor == false)
				{
					//Check for pending user here, query by email address for pending user
					var pendingUser = await _pendingUserRepository.Get(x => x.Email == model.Email);

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

								var stampResult = await _pendingUserRepository.Update(pendingUser);;

								if (stampResult)
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

					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					//return View(model);
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		// POST: /Account/Login
		/// <summary>
		/// Logins to the API.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns></returns>
		//[Route("[controller]/[action]")]
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> LoginApi([FromBody]LoginViewModel model)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);

			if (ModelState.IsValid && (model != null))
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

				if (result.Succeeded)
				{
					return Json(result);
				}
				if (result.RequiresTwoFactor)
				{
					//no way to two factor bots at the moment
					return HttpUnauthorized();
				}
				if (result.IsLockedOut)
				{
					return new HttpStatusCodeResult(403);
				}
				else
				{
					//ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return HttpUnauthorized();
				}
			}

			// If we got this far, something failed, send server error
			return new HttpStatusCodeResult(500);
		}

		[HttpGet]
		public IActionResult LoginTest()
		{
			return new NoContentResult();
		}

		//
		// GET: /Account/Register
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			//Add Roles to view bag / view data
			ViewData["Roles"] = RoleHelper.SelectableRoleList();

			var viewModel = new PageViewModel(ActionContext)
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
		private TPendingUser CreatePendingUser(RegisterViewModel model)
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

		// todo: change to add pending users to the database
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);

			if (ModelState.IsValid)
			{
				var pendingUser = CreatePendingUser(model);

				//var result = await _userManager.CreateAsync(user, model.Password);

				var result = await _pendingUserRepository.Insert(pendingUser);

				if (result)
				{
					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
					// Send an email with this link
					//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					//var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
					//await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
					//    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
					//await _signInManager.SignInAsync(user, isPersistent: false);

					return RedirectToAction("Index", "Home");
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

			var viewModel = new PageViewModel(ActionContext)
			{
				Title = "Request Access",
				Description = "Create a new account request. The account will be reviewed by a User Administrator before being added to the System.",
				Data = model
			};

			// If we got this far, something failed, redisplay form
			return this.ViewFromModel(viewModel);
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogOff()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		//
		// POST: /Account/ExternalLogin
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			EnsureDatabaseCreated(_userRepository.DataContext);
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		//
		// GET: /Account/ExternalLoginCallback
		[HttpGet]
		[AllowAnonymous]
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
				var email = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Email);
				return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
			}
		}

		//
		// POST: /Account/ExternalLoginConfirmation
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
		{
			if (User.IsSignedIn())
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
		[HttpGet]
		[AllowAnonymous]
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

		//
		// GET: /Account/ForgotPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
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

		//
		// GET: /Account/ForgotPasswordConfirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		//
		// GET: /Account/ResetPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPassword(string code = null)
		{
			return code == null ? View("Error") : View();
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
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

		//
		// GET: /Account/ResetPasswordConfirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		//
		// GET: /Account/SendCode
		[HttpGet]
		[AllowAnonymous]
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

		//
		// POST: /Account/SendCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
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

		//
		// GET: /Account/VerifyCode
		[HttpGet]
		[AllowAnonymous]
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

		//
		// POST: /Account/VerifyCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
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
			return await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
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