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

		/// <summary>
		/// Selectable role list for Mvc.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<SelectListItem> SelectableRoleList()
		{
			foreach (var role in RoleHelper.MutableRoles)
			{
				var listItem = new SelectListItem()
				{
					Text = role,
					Value = role
				};

				yield return listItem;
			}

			var defaultOption = new SelectListItem()
			{
				Selected = true,
				Text = "Change User Role",
				Value = ""
			};

			yield return defaultOption;
		}
	}
}
