using GenericMvc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericMvc.Auth.Models
{
	public interface IUser
	{
		string UserName { get; }

		void FromRegisterViewModel<TRegisterViewModel>(TRegisterViewModel viewModel) where TRegisterViewModel : RegisterViewModel;
	}
}
