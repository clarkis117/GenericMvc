using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.ViewModels.UserManager.Account
{
	public class ConfirmUserViewModel : IUserConstraints 
	{
		[Required]
		public object PendingUserId { get; set; }

		[Required]
		public DateTime DateRegistered { get; set; }

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
		public string Email { get; set; }

		//Corresponds to requested role
		[Required]
		[Display(Name = "Assigned Role")]
		public string AssignedRole { get; set; }

		[Required]
		[Phone]
		[DataType(DataType.PhoneNumber)]
		[Display(Name = "Phone Number")]
		public string PhoneNumber { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Old Password")]
		public string Password { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm New Password")]
		[Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation new password do not match.")]
		public string ConfirmNewPassword { get; set; }

	}
}
