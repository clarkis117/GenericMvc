using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.UserManager.Account
{
	public class SysOwnerChangeDefaultsViewModel
	{
		[Required]
		[StringLength(150, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
		[DataType(DataType.Text)]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[StringLength(150, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
		[DataType(DataType.Text)]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email")]
		public string NewEmail { get; set; }

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Confirm Email")]
		[Compare(nameof(NewEmail), ErrorMessage = "The email address and confirmation email address do not match.")]
		public string ConfirmNewEmail { get; set; }

		[Required]
		[Phone]
		[DataType(DataType.PhoneNumber)]
		[Display(Name = "Phone Number")]
		public string NewPhoneNumber { get; set; }

		[Required]
		[Phone]
		[DataType(DataType.PhoneNumber)]
		[Display(Name = "Confirm Phone Number")]
		[Compare(nameof(NewPhoneNumber), ErrorMessage = "The Phone Number and confirmation Phone Number do not match.")]
		public string ConfirmNewPhoneNumber { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current Password")]
		public string CurrentPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		[StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		public string NewPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm New Password")]
		[StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		[Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmNewPassword { get; set; }
	}
}
