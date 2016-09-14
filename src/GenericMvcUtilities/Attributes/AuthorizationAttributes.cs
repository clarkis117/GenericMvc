using GenericMvcUtilities.UserManager;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Attributes
{
	public sealed class AuthorizeSystemOwnerOnly : AuthorizeAttribute
	{
		public AuthorizeSystemOwnerOnly()
		{
			Roles = RoleHelper.SystemOwner;
		}
	}

	public sealed class AuthorizeUserAdmin : AuthorizeAttribute
	{
		public AuthorizeUserAdmin()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin}";
		}
	}

	public sealed class AuthorizeCrud : AuthorizeAttribute
	{
		public AuthorizeCrud()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin},{RoleHelper.ContentAdmin}";
		}
	}

	public sealed class AuthorizeContentViewing : AuthorizeAttribute
	{
		public AuthorizeContentViewing()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin},{RoleHelper.ContentAdmin},{RoleHelper.ContentViewer}";
		}
	}
}
