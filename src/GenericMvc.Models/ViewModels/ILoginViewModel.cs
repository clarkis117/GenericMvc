using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Models.ViewModels
{
	public interface ILoginViewModel
	{
		string UserName { get; }

		string Password { get; }

		bool IsPersistent { get; }
	}
}
