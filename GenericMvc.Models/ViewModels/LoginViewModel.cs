using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Models.ViewModels
{
	public class LoginViewModel : ILoginViewModel
	{
		[Required]
		public string UserName { get; set; }

		[Required, DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool IsPersistent { get; set; }
	}
}
