using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace GenericMvcUtilities.Models
{
	public abstract class BaseUser : IdentityUser, IUserConstraints
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

	public interface IUserConstraints
	{
		string FirstName { get; set; }

		string LastName { get; set; }

		string Email { get; set; }

		string PhoneNumber { get; set; }

		DateTime DateRegistered { get; set; }
	}

	/* todo: idea for later date, IUser
	public interface IUser<TKey> : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
	}
	*/
}