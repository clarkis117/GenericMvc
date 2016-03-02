using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// An application role manager from this stackoverflow
	/// post: http://stackoverflow.com/questions/19697226/creating-roles-in-asp-net-identity-mvc-5/34381864#34381864 
	/// </summary>
	/// <seealso cref="Microsoft.AspNet.Identity.RoleManager{Microsoft.AspNet.Identity.EntityFramework.IdentityRole}" />
	public class ApplicationRoleManager : RoleManager<IdentityRole>
	{
		public ApplicationRoleManager(
			IRoleStore<IdentityRole> store,
			IEnumerable<IRoleValidator<IdentityRole>> roleValidators,
			ILookupNormalizer keyNormalizer,
			IdentityErrorDescriber errors,
			ILogger<RoleManager<IdentityRole>> logger,
			IHttpContextAccessor contextAccessor)
			: base(store, roleValidators, keyNormalizer, errors, logger, contextAccessor)
		{
		}
	}

	/// <summary>
	/// Generic Version of the Above Role Manager
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <seealso cref="Microsoft.AspNet.Identity.RoleManager{Microsoft.AspNet.Identity.EntityFramework.IdentityRole{TKey}}" />
	public class GenericRoleManager<TKey> : RoleManager<IdentityRole<TKey>>
		where TKey : IEquatable<TKey>
	{
		public GenericRoleManager(
			IRoleStore<IdentityRole<TKey>> store,
			IEnumerable<IRoleValidator<IdentityRole<TKey>>> roleValidators,
			ILookupNormalizer keyNormalizer,
			IdentityErrorDescriber errors,
			ILogger<RoleManager<IdentityRole<TKey>>> logger,
			IHttpContextAccessor contextAccessor)
			: base(store, roleValidators, keyNormalizer, errors, logger, contextAccessor)
		{
		}
	}
}
