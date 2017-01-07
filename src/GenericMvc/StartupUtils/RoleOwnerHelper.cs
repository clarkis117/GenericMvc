using GenericMvc.Models;
using GenericMvc.UserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMvc.StartupUtils
{
	/// <summary>
	///	Check to see if all roles have been created
	///		if not create them
	///	Check if the system owner account has been created
	///		if not create the default system owner account
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	/// <typeparam name="TRole"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public class SystemOwnerHelper<TUser, TRole, TKey>
	where TUser : IdentityUser<TKey>, IPrivilegedUserConstraints, new()
	where TRole : IdentityRole<TKey>, new()
	where TKey : IEquatable<TKey>
	{
		private readonly UserManager<TUser> _userManager;

		private readonly RoleManager<TRole> _roleManager;

		public SystemOwnerHelper(UserManager<TUser> userManager, RoleManager<TRole> roleManager)
		{
			if (userManager == null)
				throw new ArgumentNullException(nameof(userManager));

			_userManager = userManager;

			if (roleManager == null)
				throw new ArgumentNullException(nameof(roleManager));

			_roleManager = roleManager;
		}

		public async Task Initialize()
		{
			await EnsureRolesCreated();

			if (!await HasSystemOwnerBeenCreated())
			{
				await CreateDefaultSystemOwner();
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

		private async Task EnsureRolesCreated()
		{
			// add all roles, that should be in database, here
			await EnsureRoleCreated(_roleManager, RoleHelper.SystemOwner);
			await EnsureRoleCreated(_roleManager, RoleHelper.UserAdmin);
			await EnsureRoleCreated(_roleManager, RoleHelper.ContentAdmin);
			await EnsureRoleCreated(_roleManager, RoleHelper.ContentViewer);
		}

		private async Task<bool> HasSystemOwnerBeenCreated()
		{
			var users = await _userManager.GetUsersInRoleAsync(RoleHelper.SystemOwner);

			if (users.Count > 1)
				throw new Exception("There cannot be more than one user with the System Owner Role");

			if (users.Count == 1)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private async Task CreateDefaultSystemOwner()
		{
			/*
			if (user != null) //if user is already created
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
			*/

			//if user does not exists then create it
			TUser systemOwner = new TUser()
			{
				FirstName = SystemOwnerDefaults.Name,
				LastName = SystemOwnerDefaults.Name,
				Email = SystemOwnerDefaults.UserNameEmail,
				UserName = SystemOwnerDefaults.UserNameEmail,
				DateRegistered = DateTime.Now
			};

			var result = await _userManager.CreateAsync(systemOwner, SystemOwnerDefaults.Password);

			//success creating User for System Owner now add to owner role
			if (result.Succeeded)
			{
				var admin = await _userManager.FindByEmailAsync(SystemOwnerDefaults.UserNameEmail);

				var createOwner = await _userManager.AddToRoleAsync(admin, RoleHelper.SystemOwner);

				if (createOwner.Succeeded)
				{
					return;
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