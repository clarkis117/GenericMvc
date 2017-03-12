using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace GenericMvc.Models
{
	public abstract class PrivilegedUser : IdentityUser, IPrivilegedUserConstraints
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
}