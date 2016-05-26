using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenericMvcUtilities.UserManager
{
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
