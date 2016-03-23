﻿using GenericMvcUtilities.UserManager;
using System.ComponentModel.DataAnnotations;

namespace GenericMvcUtilities.ViewModels.UserManager
{
	/// <summary>
	/// View Model for change a User or PendingUser Role
	/// </summary>
	public class RoleChangeViewModel
	{
		[Required]
		public object UserId { get; set; }

		[Required]
		public string NewRole { get; set; }

		public bool IsValid
		{
			get
			{
				var valid = false;

				if (NewRole != null)
				{
					foreach (var role in RoleHelper.MutableRoles)
					{
						if (NewRole == role)
						{
							valid = true;
							break;
						}
					}
				}

				return valid;
			}
		}
	}
}