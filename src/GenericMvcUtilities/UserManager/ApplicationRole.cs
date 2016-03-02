using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GenericMvcUtilities.UserManager
{
	public class ApplicationRole : IdentityRole<Guid>
	{
		public ApplicationRole(string roleName) : base(roleName)
		{
		}
	}
}
