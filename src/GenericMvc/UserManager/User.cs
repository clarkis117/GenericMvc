using GenericMvc.ViewModels.UserManager.Account;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace GenericMvc.Models
{
	public abstract class BasePrivilegedUser : IdentityUser, IPrivilegedUserConstraints
	{
		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		//already in identity user public string Email { get; set; }

		//already in identity user public string PhoneNumber { get; set; }

		[Required]
		public DateTime DateRegistered { get; set; }
	}

	public interface IPrivilegedUserConstraints
	{
		string FirstName { get; set; }

		string LastName { get; set; }

		string Email { get; set; }

		string PhoneNumber { get; set; }

		DateTime DateRegistered { get; set; }
	}


	public interface IBaseIdentifyingUser : IBaseUser
	{
		string Email { get; set; }
	}

	public interface IBaseUser
	{
		string UserName { get; set; }

		string Password { get; set; }
	}

	/// <summary>
	/// Base abstract class to represent non-privileged users
	/// These users have no roles or claims external to their identity
	/// They should only be allowed to edit data that lives under their data object
	/// They should only be allowed to view data that is not access controlled
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUser" />
	public class BaseUser : IdentityUser
	{
		public BaseUser()
		{

		}

		public BaseUser(BaseRegisterViewModel model) : base(model.UserName)
		{
			if (model != null)
			{

				if (model is IdentifyingRegisterViewModel)
				{
					Email = (model as IdentifyingRegisterViewModel).Email;
				}

				if (model.Password != model.ConfirmPassword)
				{
					throw new ArgumentException("Supplied Passwords are not the same");
				}

			}
			else
			{
				throw new ArgumentNullException(nameof(model));
			}
		}
	}





	/* todo: idea for later date, IUser
	public interface IUser<TKey> : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
	}
	*/
}