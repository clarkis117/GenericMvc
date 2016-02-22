using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// Class also defines roles
	/// </summary>
	public static class RoleHelper
	{
		public const string UserAdmin = "UserAdmin";

		public const string ContentAdmin = "ContentAdmin";

		public const string ContentViewer = "ContentViewer";

		private static readonly string[] _roles = {"UserAdmin", "ContentAdmin", "ContentViewer"};

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
			await EnsureRoleCreated(roleManager, UserAdmin);
			await EnsureRoleCreated(roleManager, ContentAdmin);
			await EnsureRoleCreated(roleManager, ContentViewer);
		}
	}
}
