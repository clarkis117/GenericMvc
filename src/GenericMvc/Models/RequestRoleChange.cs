using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using GenericMvc.UserManager;

namespace GenericMvc.Models
{
	//todo move to view model folder
	// or delete since it isn't used
	public class RequestRoleChange
	{
		[Required]
		public object Id { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string RequestedRole { get; set; }

		/// <summary>
		/// Returns true if Requested Role is valid, according to the Roles defined in mutable roles in rolehelper class.
		/// </summary>
		/// <returns>boolean</returns>
		public bool IsValid()
		{
			foreach (var role in RoleHelper.MutableRoles)
			{
				if (RequestedRole == role)
				{
					return true;
				}
			}

			return false;
		}
	}
}
