using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.ViewModels.UserManager
{
	public interface IUserIndexView
	{
		[Display(Name = "User Identifier")]
		object Id { get; }

		[Display(Name = "First Name")]
		string FirstName { get; }

		[Display(Name = "Last Name")]
		string LastName { get; }

		[Display(Name = "User Name")]
		string UserName { get; }

		[EmailAddress]
		[Display(Name = "Email")]
		string Email { get; }

		[DataType(DataType.DateTime)]
		[Display(Name = "Date Registered")]
		DateTime DateRegistered { get; }

		bool ShowDetails { get; }
	}


	public class PendingUserViewModel<TKey, TPendingUser> : IUserIndexView
		where TKey : IEquatable<TKey>
		where TPendingUser : PendingUser<TKey>
	{
		public object Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string UserName { get; set; }

		public string Email { get; set; }

		public DateTime DateRegistered { get; set; }

		public bool ShowDetails { get; set; } = true;

		public PendingUserViewModel(TPendingUser pendingUser)
		{
			this.Id = pendingUser.Id;

			this.FirstName = pendingUser.FirstName;

			this.LastName = pendingUser.LastName;

			this.Email = pendingUser.Email;

			this.DateRegistered = pendingUser.DateRegistered;
		}
	}

	/// <summary>
	/// todo: subject to change
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TUser">The type of the user.</typeparam>
	/// <seealso cref="GenericMvcUtilities.ViewModels.UserManager.IUserIndexView" />
	public class UserIndexViewModel<TKey, TUser> : IUserIndexView
		where TKey : IEquatable<TKey>
		where TUser : IdentityUser<TKey>, Models.IPrivilegedUserConstraints
	{
		public object Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string UserName { get; set; }

		public string Email { get; set; }

		public DateTime DateRegistered { get; set; }

		public bool ShowDetails { get; set; } = true;


		public UserIndexViewModel(TUser user)
		{
			this.Id = user.Id;

			this.FirstName = user.FirstName;

			this.LastName = user.LastName;

			this.UserName = user.UserName;

			this.Email = user.Email;

			this.DateRegistered = user.DateRegistered;
		}
	}
}