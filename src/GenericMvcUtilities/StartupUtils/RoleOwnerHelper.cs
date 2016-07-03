using GenericMvcUtilities.Models;
using GenericMvcUtilities.UserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.StartupUtils
{
	public class SystemOwnerHelper<TUser, TRole, TKey>
	where TUser : IdentityUser<TKey>, IPrivilegedUserConstraints, new()
	where TRole : IdentityRole<TKey>, new()
	where TKey : IEquatable<TKey>
	{
		private UserManager<TUser> _userManager;

		private RoleManager<TRole> _roleManager;

		public SystemOwnerHelper(UserManager<TUser> userManager, RoleManager<TRole> roleManager)
		{
			if (userManager != null)
			{
				_userManager = userManager;
			}
			else
			{
				throw new ArgumentNullException(nameof(userManager));
			}

			if (roleManager != null)
			{
				_roleManager = roleManager;
			}
			else
			{
				throw new ArgumentNullException(nameof(roleManager));
			}
		}

		private static List<Exception> ConvertIdentityErrorsToExceptions(IEnumerable<IdentityError> errors)
		{
			var exceptions = new List<Exception>();

			foreach (var error in errors)
			{
				exceptions.Add(new Exception(error.Code + ": " + error.Description));
			}

			return exceptions;
		}

		private static async Task EnsureRoleCreated(RoleManager<TRole> roleManager, string roleName)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{
				await roleManager.CreateAsync((TRole)Activator.CreateInstance(typeof(TRole), roleName));
			}
		}

		public async Task EnsureRolesCreated()
		{
			// add all roles, that should be in database, here
			await EnsureRoleCreated(_roleManager, RoleHelper.SystemOwner);
			await EnsureRoleCreated(_roleManager, RoleHelper.UserAdmin);
			await EnsureRoleCreated(_roleManager, RoleHelper.ContentAdmin);
			await EnsureRoleCreated(_roleManager, RoleHelper.ContentViewer);
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

			//attempt to find user
			var user = await _userManager.FindByEmailAsync(email);

			//if user does not exists then create it
			if (user != null)
			{
				var roles = await _userManager.GetRolesAsync(user);

				//get roles
				if (roles != null && roles.Count() > 0)
				{
					bool isSystemOwner = false;

					//check for system owner role
					foreach (var role in roles)
					{
						if (role == RoleHelper.SystemOwner)
						{
							isSystemOwner = true;
							break;
						}
					}

					if (isSystemOwner)
					{
						return;
					}
					else //if not throw exception, since this would basically be a privilege escalation attack
					{
						throw new Exception("Existing User cannot be promoted to System Owner");
					}
				}
				else // if no roles add user to role
				{
					var result = await _userManager.AddToRoleAsync(user, RoleHelper.SystemOwner);

					if (result.Succeeded)
					{
						return;
					}
					else
					{
						throw new AggregateException("Error adding user to System Owner Role", ConvertIdentityErrorsToExceptions(result.Errors));
					}
				}
			}
			else //then create user
			{
				TUser systemOwner = new TUser()
				{
					FirstName = "SystemOwner",
					LastName = "SystemOwner",
					Email = email,
					UserName = email,
					DateRegistered = DateTime.Now
				};

				var result = await _userManager.CreateAsync(systemOwner, password);

				if (result.Succeeded) //success creating User for System Owner now add to owner role
				{
					var admin = await _userManager.FindByEmailAsync(email);

					var createOwner = await _userManager.AddToRoleAsync(admin, RoleHelper.SystemOwner);

					if (createOwner.Succeeded)
					{
						return; //throw new Exception("creation of System Owner Failed");
					}
					else
					{
						throw new AggregateException("Adding new user to role, for system owner role failed", ConvertIdentityErrorsToExceptions(createOwner.Errors));
					}
				}
				else
				{
					throw new AggregateException("Creating User for System Owner Failed", ConvertIdentityErrorsToExceptions(result.Errors));
				}
			}
		}
	}
}
