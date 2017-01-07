using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Models
{

	public class LoginModel : ILoginModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public interface ILoginModel
	{
		string Email { get; set; }

		string Password { get; set; }

		bool RememberMe { get; set; }
	}
}
