using GenericMvc.UserManager;
using System.ComponentModel.DataAnnotations;

namespace GenericMvc.ViewModels.UserManager
{
	/// <summary>
	/// View Model for change a User or PendingUser Role
	/// </summary>
	public class RoleChangeViewModel
	{
		public bool Inline { get; set; } = false;

		[Required]
		public string UserId { get; set; }

		[Required]
		[Display(Name = "New Role")]
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

		public RoleChangeViewModel()
		{

		}

		public RoleChangeViewModel(object userId, bool inline = false)
		{
			UserId = userId.ToString();

			Inline = inline;
		}
	}
}