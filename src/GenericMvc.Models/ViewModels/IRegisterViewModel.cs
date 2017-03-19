using System;
using System.Collections.Generic;
using System.Text;

namespace GenericMvc.Models.ViewModels
{
	public interface IRegisterViewModel
	{
		string UserName { get; }

		string Password { get; }

		bool IsPersistent { get; }
	}
}
