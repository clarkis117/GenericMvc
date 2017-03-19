using GenericMvc.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public interface IAuthClient
	{
		IEnumerable<string> Cookies { get; set; }

		string AuthToken { get; set; }

		//routes, path

		string RegisterRoute { get; }

		string LoginRoute { get; }

		string LogoutRoute { get; }

		//param methods
		//todo: fix
		Task<bool> Register(RegisterViewModel model);

		Task<string> Login(LoginViewModel model);

		Task<bool> Logout();
	}

	/// <summary>
	/// todo User Xamarin connectivity package to test connection status on creation
	/// </summary>
	/// <seealso cref="GenericMvc.Client.IAuthClient" />
	public class AuthClient : IAuthClient
	{
		private readonly HttpFixture _fixture;

		private LoginViewModel _loginInfo;

		private readonly bool _cacheLoginInfo;

		public string Api { get; set; } = "api";

		public string ControllerName { get; set; } = "auth";

		public string LoginAction { get; set; } = "login";

		public string RegisterAction { get; set; } = "register";

		public string LogoutAction { get; set; } = "logout";

		public IEnumerable<string> Cookies { get; set; }

		public string AuthToken { get; set; }

		public string RegisterRoute => $"/{Api}/{ControllerName}/{RegisterAction}";

		public string LoginRoute => $"/{Api}/{ControllerName}/{LoginAction}";

		public string LogoutRoute => $"/{Api}/{ControllerName}/{LogoutAction}";

		public AuthClient(HttpFixture fixture, bool cacheLoginInfo = true)
		{
			if (fixture == null)
				throw new ArgumentNullException(nameof(fixture));

			_fixture = fixture;

			_cacheLoginInfo = cacheLoginInfo;
		}

		//ToDo: investigate a better alternative to this
		public static IList<String> CleanCookies(IEnumerable<string> cookies)
		{
			return cookies.Select(item => item.Split(';').First()).ToList();
		}

		public static string ProcessCookies(IEnumerable<string> cookies)
		{
			var concatCookies = new StringBuilder();

			if (cookies != null)
			{
				foreach (var item in cookies)
				{
					concatCookies.Append(item + "; ");
				}
			}

			return concatCookies.ToString();
		}

		public Task<LoginViewModel> GetLoginInfoFromFile(string path)
		{
			if (path != null && System.IO.File.Exists(path))
			{
				return Task.Run(async () =>
				{
					LoginViewModel loginInfo;

					using (var reader = new StreamReader(System.IO.File.OpenRead(path)))
					{
						var info = await reader.ReadToEndAsync();
						loginInfo = (LoginViewModel) JsonConvert.DeserializeObject(info, typeof(LoginViewModel));
					}

					return loginInfo;
				});
			}
			else
			{
				throw new ArgumentException(nameof(path) + " :either null or does not exist");
			}
		}

		public Task<bool> SaveLoginInfoToFile(string path, LoginViewModel loginInfo)
		{
			if (path != null)
			{
				LoginViewModel login;

				if (_cacheLoginInfo == false && loginInfo != null)
				{
					login = loginInfo;
				}
				else
				{
					if (_loginInfo == null)
						throw new NullReferenceException(nameof(_loginInfo));

					login = _loginInfo;
				}

				return Task.Run(async () =>
				{
					try
					{
						using (var textWriter = System.IO.File.CreateText(path))
						{
							var info = JsonConvert.SerializeObject(login);

							await textWriter.WriteAsync(info);
							await textWriter.FlushAsync();
						}

						return true;
					}
					catch (Exception)
					{
						return false;
					}
				});
			}
			else
			{
				throw new ArgumentNullException(nameof(path));
			}
		}

		public async Task<string> Login(LoginViewModel model)
		{
			if (model != null)
			{
				_loginInfo = model;

				//Create the cookie container our client
				var cookieJar = new CookieContainer();

				//Post Password and stuff to Rest Server, Response should contain the password token cookie
				using (HttpResponseMessage response = await _fixture.Client.PostAsync(LoginRoute,
										new JsonContent(JsonConvert.SerializeObject(model))))
				{
					if (response.IsSuccessStatusCode)
					{
						//Grab the cookies
						var cookies = response.Headers.GetValues("Set-Cookie"); //First(x => x.StartsWith(".AspNetCore.Microsoft.Identity.Application"));

						var processedCookie = ProcessCookies(CleanCookies(cookies));

						//cache cookie
						this.AuthToken = processedCookie;

						//return ref to cookie
						return processedCookie;
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(model));
			}
		}

		public async Task<string> Login()
		{
			return await this.Login(this._loginInfo);
		}

		//todo: fix
		public async Task<bool> Register(RegisterViewModel registrationInfo)
		{
			if (registrationInfo != null)
			{
				using (HttpResponseMessage response = await _fixture.Client.PostAsync(RegisterRoute,
								new JsonContent(JsonConvert.SerializeObject(registrationInfo))))
				{
					if (response.IsSuccessStatusCode)
					{
						//Grab the cookies
						var cookies = response.Headers.GetValues("Set-Cookie"); //First(x => x.StartsWith(".AspNetCore.Microsoft.Identity.Application"));

						var processedCookie = ProcessCookies(CleanCookies(cookies));

						this.AuthToken = processedCookie;

						return true;
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(registrationInfo));
			}
		}

		public async Task<bool> Logout()
		{
			try
			{
				using (HttpResponseMessage response = await _fixture.Client.PostAsync(this.LogoutRoute,
					new JsonContent("Log me out")))
				{
					if (response.IsSuccessStatusCode)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}