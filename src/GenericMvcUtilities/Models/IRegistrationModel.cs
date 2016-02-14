using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
	public class RegistrationModel : IRegistrationModel
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
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public interface IRegistrationModel
	{
		string FirstName { get; set; }

		string LastName { get; set; }

		string Email { get; set; }

		string Password { get; set; }

		string ConfirmPassword { get; set; }
	}
}
