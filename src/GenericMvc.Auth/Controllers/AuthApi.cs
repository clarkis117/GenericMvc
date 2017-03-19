using GenericMvc.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GenericMvc.Auth.Controllers
{
	/// <summary>
	/// In terms of registering users, this is merely designed for registering non-privileged users
	/// In general, the use of this class is designed for non-privileged Users or privileged user whom already registered via the account controller class
	///
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TUser">The type of the user.</typeparam>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
	/// <seealso cref="GenericMvcModels.Controllers.IAuthApiController" />
	[Route("/api/[controller]/[action]/"), Authorize]
	public class AuthApi<TKey, TUser, TRegisterViewModel, TLoginViewModel> : Controller, IAuthApi<TRegisterViewModel, TLoginViewModel>
		where TLoginViewModel : LoginViewModel
		where TRegisterViewModel : RegisterViewModel
		where TKey : IEquatable<TKey>
		where TUser : Models.User, new()
	{
		private readonly SignInManager<TUser> _signInManager;

		private readonly UserManager<TUser> _userManager;

		private readonly ILogger<TUser> _logger;

		public AuthApi(SignInManager<TUser> signInManager, UserManager<TUser> userManager, ILogger<TUser> logger)
		{
			_signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));

			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost, AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] TLoginViewModel viewModel)
		{
			if (viewModel == null || !ModelState.IsValid)
				return BadRequest();

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			var result = await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, viewModel.IsPersistent, lockoutOnFailure: false);

			if (result.Succeeded)
				return Json(result);

			//no way to two factor bots at the moment
			if (result.RequiresTwoFactor)
				return Unauthorized();

			if (result.IsLockedOut)
				return new StatusCodeResult(403);

			return Unauthorized();
		}

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			return Ok();
		}

		[HttpPost, AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] TRegisterViewModel viewModel)
		{
			if (viewModel == null && !ModelState.IsValid)
				return BadRequest(ModelState);

			//todo make this
			var user = new TUser()
			{
				UserName = viewModel.UserName
			};

			user.FromRegisterViewModel(viewModel);

			//if (model.Password != model.ConfirmPassword)
			//return BadRequest("Supplied Passwords are not the same");

			var result = await _userManager.CreateAsync(user, viewModel.Password);

			if (!result.Succeeded)
				return new StatusCodeResult(500);

			await _signInManager.SignInAsync(user, viewModel.IsPersistent);

			_logger.LogInformation($"{user.UserName} create a new account with password.");

			return Json(result);
		}
	}
}