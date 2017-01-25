using GenericMvc.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace GenericMvc.ViewModels.UserManager
{
	public interface IPendingUserView
	{
		[Display(Name = "User Identifier")]
		object Id { get; set; }

		[Display(Name = "First Name")]
		string FirstName { get; set; }

		[Display(Name = "Last Name")]
		string LastName { get; set; }

		[EmailAddress]
		[Display(Name = "Email")]
		string Email { get; set; }

		[Phone]
		[Display(Name = "Phone Number")]
		string PhoneNumber { get; set; }

		[DataType(DataType.DateTime)]
		[Display(Name ="Date Registered")]
		DateTime DateRegistered { get; set; }

		[DataType(DataType.Text)]
		[Display(Name ="Requested Role")]
		string RequestedRole { get; set; }
	}

	/// <summary>
	/// todo: subject to change
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TPendingUser">The type of the user.</typeparam>
	/// <seealso cref="GenericMvcModels.ViewModels.UserManager.IPendingUserView" />
	public class PendingUserDetails<TKey, TPendingUser> : IPendingUserView
		where TKey : IEquatable<TKey>
		where TPendingUser : PendingUser<TKey>, Models.IPrivilegedUserConstraints
	{
		public object Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public string PhoneNumber { get; set; }

		public DateTime DateRegistered { get; set; }

		public string RequestedRole { get; set; }

		public PendingUserDetails(TPendingUser user)
		{
			this.Id = user.Id;

			this.FirstName = user.FirstName;

			this.LastName = user.LastName;

			this.Email = user.Email;

			this.PhoneNumber = user.PhoneNumber;

			this.DateRegistered = user.DateRegistered;

			this.RequestedRole = user.RequestedRole;
		}
	}
}