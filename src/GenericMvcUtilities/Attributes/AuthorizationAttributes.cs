using GenericMvcUtilities.UserManager;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Attributes
{
	public class AuthorizeUserAdmin : AuthorizeAttribute
	{
		public AuthorizeUserAdmin()
		{
			Roles = $"{RoleHelper.SystemOwner},{RoleHelper.UserAdmin}";
		}
	}

	public class AuthorizeCrud : AuthorizeAttribute
	{
		public AuthorizeCrud()
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
}
