using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GenericMvcUtilities.Models
{
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
