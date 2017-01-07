using GenericMvc.ViewModels.UserManager.Account;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Client
{
	public class JsonContent : StringContent
	{
		public const string ContentType = "application/json";

		public JsonContent(string content) : base(content)
		{
			this.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
		}
	}


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
		Task<bool> Register(BaseRegisterViewModel model);

		Task<string> Login(BaseLoginViewModel model);

		Task<bool> Logout();
	}

	public struct RouteInfo
	{
		public string Api;

		public string ControllerName;

		public string Login;

		public string Register;

		public string Logout;

		public RouteInfo(string controllerName = "auth", string login = "login", string register = "register", string logout = "logout")
		{
			ControllerName = controllerName;

			Login = login;

			Register = register;

			Logout = logout;

			Api = "/api/";
		}
	}

	/// <summary>
	/// User Xamarin connectivity package to test connection status on creation
	/// </summary>
	/// <seealso cref="GenericMvc.Client.IAuthClient" />
	/// <seealso cref="System.IDisposable" />
	public class AuthClient : IAuthClient, IDisposable
	{
		private readonly HttpClient _client;

		public readonly string MimeType = "application/json";

		private readonly JsonSerializer _serailizer;

		private readonly RouteInfo _info;

		private BaseLoginViewModel _loginInfo;

		private readonly bool _cacheLoginInfo;


		public IEnumerable<string> Cookies { get; set; }

		public string AuthToken { get; set; }

		public string RegisterRoute => _info.Api + _info.ControllerName + "/" + _info.Register;

		public string LoginRoute => _info.Api + _info.ControllerName + "/" + _info.Login;

		public string LogoutRoute => _info.Api + _info.ControllerName + "/" + _info.Logout;

		public AuthClient(RouteInfo routeInfo, HttpClient client, bool cacheLoginInfo = true)
		{
			if (client != null)
			{
				_client = client;

				_info = routeInfo;

				_serailizer = JsonSerializer.Create();

				this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
			else
			{
				throw new ArgumentNullException(nameof(client));
			}
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

		public Task<BaseLoginViewModel> GetLoginInfoFromFile(string path)
		{
			if (path != null && System.IO.File.Exists(path))
			{
				return Task.Run(() =>
				{
					BaseLoginViewModel loginInfo;

					using (var reader = new StreamReader(System.IO.File.OpenRead(path)))
					{
						loginInfo = (BaseLoginViewModel)_serailizer.Deserialize(reader, typeof(BaseLoginViewModel));
					}

					return loginInfo;
				});
			}
			else
			{
				throw new ArgumentException(nameof(path) + " :either null or does not exist");
			}
		}

		public Task<bool> SaveLoginInfoToFile(string path, BaseLoginViewModel loginInfo)
		{
			if (path != null)
			{
				BaseLoginViewModel login;

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

				return Task.Run(() =>
				{
					try
					{
						using (var textWriter = System.IO.File.CreateText(path))
						{
							_serailizer.Serialize(textWriter, login);
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

		public async Task<string> Login(BaseLoginViewModel model)
		{
			if (model != null)
			{
				_loginInfo = model;

				//Create the cookie container our client
				var cookieJar = new CookieContainer();

				//Post Password and stuff to Rest Server, Response should contain the password token cookie
				using (HttpResponseMessage response = await this._client.PostAsync(this.LoginRoute,
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
		public async Task<bool> Register(BaseRegisterViewModel registrationInfo)
		{
			if (registrationInfo != null)
			{
				using (HttpResponseMessage response = await this._client.PostAsync(this.RegisterRoute,
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
				using (HttpResponseMessage response = await this._client.PostAsync(this.LogoutRoute,
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

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_client.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~AuthClient() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support
	}
}