using System.ComponentModel.DataAnnotations;

namespace GenericMvc.Models.ViewModels
{
	/// <summary>
	/// Basic Registration View model for non privileged users
	/// </summary>
	public class RegisterViewModel : IRegisterViewModel
	{
		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public bool IsPersistent { get; set; }
	}
}