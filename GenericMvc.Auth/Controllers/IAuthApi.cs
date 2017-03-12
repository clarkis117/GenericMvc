using GenericMvc.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Auth.Controllers
{
	public interface IAuthApi<TRegisterViewModel, TLoginViewModel>
		where TLoginViewModel : LoginViewModel
		where TRegisterViewModel : RegisterViewModel
	{
		Task<IActionResult> Register([FromBody] TRegisterViewModel viewModel);

		Task<IActionResult> Login([FromBody] TLoginViewModel viewModel);

		Task<IActionResult> Logout();
	}
}
