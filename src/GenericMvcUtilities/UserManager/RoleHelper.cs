using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using GenericMvcUtilities.Models;


namespace GenericMvcUtilities.UserManager
{
	public class AuthorizeUserManaging: AuthorizeAttribute
	{
		public AuthorizeUserManaging()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin}";
		}
	}

	public class AuthorizeContentCRUD : AuthorizeAttribute
	{
		public AuthorizeContentCRUD()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin},{RoleHelper.ContentAdmin}";
		}
	}

	public class AuthorizeContentViewing : AuthorizeAttribute
	{
		public AuthorizeContentViewing()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin},{RoleHelper.ContentAdmin},{RoleHelper.ContentViewer}";
		}
	}

	public class SystemOwnerHelper<TUser, TRole, TKey>
		where TUser : IdentityUser<TKey>, IPrivilegedUserConstraints, new()
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{
		private UserManager<TUser> UserManager;

		private RoleManager<TRole> RoleManager;

		public SystemOwnerHelper(UserManager<TUser> userManager, RoleManager<TRole> roleManager)
		{
			if (userManager != null)
			{
				UserManager = userManager;
			}
			else
			{
				throw new ArgumentNullException(nameof(userManager));
			}

			if (roleManager != null)
			{
				RoleManager = roleManager;
			}
			else
			{
				throw new ArgumentNullException(nameof(roleManager));
			}
		}

		public async Task Create(string email, string password)
		{
			if (email == null && email.Length > 0)
			{
				throw new ArgumentNullException(nameof(email));
			}

			if (password == null && password.Length > 0)
			{
				throw new ArgumentNullException(nameof(password));
			}

			TUser systemOwner = new TUser()
			{
				FirstName = "SystemOwner",
				LastName = "SystemOwner",
				Email = email,
				UserName = email,
				DateRegistered = DateTime.Now
			};

			var doesUserExist = await UserManager.FindByEmailAsync(email);

			/*
			doesUserExist.Roles

			var doesSystemOwnerRoleExsist = await RoleManager.user

			if (doesUserExist == null)
			{
				var result = await UserManager.CreateAsync(systemOwner, password);

				if (result.Succeeded)
				{
					var admin = await UserManager.FindByEmailAsync(email);
					if (!(await userManager.AddToRoleAsync(admin, RoleHelper.UserAdmin)).Succeeded)
					{
						throw new Exception("creation of hardcoded admin failed");
					}
				}
				else
				{

				}
			}
			*/
		}
	}

	/// <summary>
	/// Class also defines roles
	/// </summary>
	public static class RoleHelper
	{
		public const string SystemOwner = "SystemOwner";

		public const string UserAdmin = "UserAdmin";

		public const string ContentAdmin = "ContentAdmin";

		public const string ContentViewer = "ContentViewer";

		public static readonly string[] MutableRoles = { "UserAdmin", "ContentAdmin", "ContentViewer"};

		private static async Task EnsureRoleCreated(RoleManager<IdentityRole> roleManager, string roleName)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
		}

		public static async Task EnsureRolesCreated(this RoleManager<IdentityRole> roleManager)
		{
			// add all roles, that should be in database, here
			await EnsureRoleCreated(roleManager, SystemOwner);
			await EnsureRoleCreated(roleManager, UserAdmin);
			await EnsureRoleCreated(roleManager, ContentAdmin);
			await EnsureRoleCreated(roleManager, ContentViewer);
		}



		/// <summary>
		/// Selectable role list for Mvc.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<SelectListItem> SelectableRoleList()
		{
			var roleListViewModel = new List<SelectListItem>();

			foreach (var role in RoleHelper.MutableRoles)
			{
				var listItem = new SelectListItem()
				{
					Text = role,
					Value = role
				};

				roleListViewModel.Add(listItem);
			}

			return roleListViewModel;
		}
	}
}
