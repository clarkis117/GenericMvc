using GenericMvc.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace GenericMvc.Auth.Models
{
	/// <summary>
	/// Base abstract class to represent non-privileged users
	/// These users have no roles or claims external to their identity
	/// They should only be allowed to edit data that lives under their data object
	/// They should only be allowed to view data that is not access controlled
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUser" />
	public abstract class User : IdentityUser, IUser
	{
		public User(): base()
		{

		}

		public abstract void FromRegisterViewModel<TRegisterViewModel>(TRegisterViewModel viewModel) where TRegisterViewModel : RegisterViewModel;
	}
}