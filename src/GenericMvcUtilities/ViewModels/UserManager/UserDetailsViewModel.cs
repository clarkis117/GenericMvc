using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using GenericMvcUtilities.ViewModels.Basic;

namespace GenericMvcUtilities.ViewModels.UserManager
{
	public interface IUserView
	{
		[Display(Name = "User Identifier")]
		object Id { get; set; }

		[Display(Name = "First Name")]
		string FirstName { get; set; }

		[Display(Name = "Last Name")]
		string LastName { get; set; }

		[Display(Name = "User Name")]
		string UserName { get; set; }

		[EmailAddress]
		[Display(Name ="Email")]
		string Email { get; set; }

		[Display(Name =	"Is Email Confirmed?")]
		bool EmailConfirmed { get; set; }

		[Display(Name = "Phone Number")]
		string PhoneNumber { get; set; }

		[Display(Name = "Is Phone Number Confirmed?")]
		bool PhoneNumberConfirmed { get; set; }

		[Display(Name = "Failed Login Attempts")]
		int AccessFailedCount { get; set; }

		[Display(Name ="Is the Account Locked?")]
		bool LockoutEnabled { get; set; }

		[Display(Name ="Locked out Until")]
		DateTimeOffset LockoutEnd { get; set; }

		[Display(Name ="Two-Factor Enabled?")]
		bool TwoFactorEnabled { get; set; }

		ICollection<string> Roles { get; }

		bool ShowActions { get; set; }

		MessageViewModel Message { get; set; }
	}


	/// <summary>
	/// todo: subject to change
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TUser">The type of the user.</typeparam>
	/// <typeparam name="TRole">The type of the role.</typeparam>
	/// <seealso cref="GenericMvcUtilities.ViewModels.UserManager.IUserView" />
	public class UserDetailsViewModel<TKey, TUser, TRole> : IUserView
		where TKey : IEquatable<TKey>
		where TUser : IdentityUser<TKey>, Models.IPrivilegedUserConstraints
		where TRole : IdentityRole<TKey>
	{
		public object Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string UserName { get; set; }

		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		public string PhoneNumber { get; set; }

		public bool PhoneNumberConfirmed { get; set; }

		public int AccessFailedCount { get; set; }

		public bool LockoutEnabled { get; set; }

		public DateTimeOffset LockoutEnd { get; set; }

		public bool TwoFactorEnabled { get; set; }

		public ICollection<string> Roles { get; }

		public bool ShowActions { get; set; } = true;

		public MessageViewModel Message { get; set; }

		//public ICollection<IdentityUserRole<TKey>> Roles { get; }

		//public ICollection<IdentityUserClaim<TKey>> Claims { get; }

		public UserDetailsViewModel(TUser user, RoleManager<TRole> roleManager, UserManager<TUser> userManager)
		{
			this.Roles = new List<string>();

			foreach (var role in user.Roles)
			{
				var foundRole = roleManager.FindByIdAsync(role.RoleId.ToString()).Result;

				Roles.Add(foundRole.Name);
			}

			this.Id = user.Id;

			/*
			dynamic use = user;

			this.FirstName = (string)use.FirstName;

			this.LastName = (string)use.LastName;
			*/

			//var userNames = user.GetType().GetRuntimeProperties()
			//.Where(x => x.GetType() == typeof (String));

			//this.FirstName = (string) userNames.First(x => x.Name == nameof(FirstName)).GetValue(user) ?? "No Name Given";

			//this.FirstName = (string) userNames.First(x => x.Name == nameof(LastName)).GetValue(user) ?? "No Name Given";

			this.FirstName = user.FirstName;

			this.LastName = user.LastName;

			this.UserName = user.UserName;

			this.Email = user.Email;

			this.EmailConfirmed = user.EmailConfirmed;

			this.PhoneNumber = user.PhoneNumber;

			this.PhoneNumberConfirmed = user.PhoneNumberConfirmed;

			this.AccessFailedCount = user.AccessFailedCount;

			//this is needed to see if user is locked out, the property on user just returns true
			this.LockoutEnabled = userManager.IsLockedOutAsync(user).Result;

			//todo: set this as a datetimeoffset and create model based
			this.LockoutEnd = (user.LockoutEnd ?? DateTimeOffset.Now);

			this.TwoFactorEnabled = user.TwoFactorEnabled;
		}
	}
}
