using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.Client
{
	public interface IAuthClient
	{
		IEnumerable<string> Cookies { get; set; }

		string AuthCookie { get; set; }

		//routes, path
		string ApiPath { get; }

		string RegisterRoute { get; }

		string LoginRoute { get; }

		string LogOffRoute { get; }

		//param methods
		Task<bool> Register(IRegistrationModel model);

		Task<string> Login(ILoginModel model);

		//State machine methods
		Task Register();

		Task<string> Login();

		Task<bool> LogOff();
	}
}
