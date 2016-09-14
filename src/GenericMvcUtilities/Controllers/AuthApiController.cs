using GenericMvcUtilities.ViewModels.UserManager.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	public interface IAuthApiController
	{
		Task<IActionResult> Register([FromBody] BaseRegisterViewModel model);

		Task<IActionResult> Login([FromBody] BaseLoginViewModel model);

		Task<IActionResult> Logout();
	}

	/// <summary>
	/// In terms of registering users, this is merely designed for registering non-privileged users
	/// In general, the use of this class is designed for non-privileged Users or privileged user whom already registered via the account controller class
	///
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TUser">The type of the user.</typeparam>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
	/// <seealso cref="GenericMvcUtilities.Controllers.IAuthApiController" />
	[Authorize]
	[Route("/api/[controller]/[action]/")]
	public class AuthApiController<TKey, TUser> : Controller, IAuthApiController
		where TKey : IEquatable<TKey>
		where TUser : IdentityUser<TKey>, new()
	{
		private readonly SignInManager<TUser> _signInManager;

		private readonly UserManager<TUser> _userManager;

		private readonly ILogger<TUser> _logger;

		public AuthApiController(SignInManager<TUser> signInManager, UserManager<TUser> userManager, ILogger<TUser> logger)
		{
			if (signInManager == null)
				throw new ArgumentNullException(nameof(signInManager));

			if (userManager == null)
				throw new ArgumentNullException(nameof(userManager));

			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			_signInManager = signInManager;

			_userManager = userManager;

			_logger = logger;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] BaseLoginViewModel model)
		{
			//EnsureDatabaseCreated(_userRepository.DataContext);

			if (ModelState.IsValid && (model != null))
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

				if (result.Succeeded)
				{
					return Json(result);
				}
				if (result.RequiresTwoFactor)
				{
					//no way to two factor bots at the moment
					return Unauthorized();
				}
				if (result.IsLockedOut)
				{
					return new StatusCodeResult(403);
				}
				else
				{
					//ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return Unauthorized();
				}
			}

			// If we got this far, something failed, send server error
			return new StatusCodeResult(500);
		}

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			return NoContent();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] BaseRegisterViewModel model)
		{
			if (model != null && ModelState.IsValid)
			{
				var user = new TUser();

				user.UserName = model.UserName;

				
				if (model is IdentifyingRegisterViewModel)
				{
					user.Email = (model as IdentifyingRegisterViewModel).Email;
				}

				if (model.Password != model.ConfirmPassword)
				{
					return BadRequest("Supplied Passwords are not the same");
				}

				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, false);

					_logger.LogInformation(3, "User create a new account with password.");

					return Json(result);
				}
				else
				{
					return new StatusCodeResult(500);
				}
			}
			else
			{
				return BadRequest(ModelState);
			}
		}
	}
}