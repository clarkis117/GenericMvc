using GenericMvcUtilities.Models;
using GenericMvcUtilities.ViewModels.UserManager.Account;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Client
{
	public interface IAuthClient
	{
		IEnumerable<string> Cookies { get; set; }

		string AuthToken { get; set; }

		//routes, path
		string ApiPath { get; }

		string RegisterRoute { get; }

		string LoginRoute { get; }

		string LogOffRoute { get; }

		//param methods
		//todo: fix
		Task<bool> Register(BaseRegisterViewModel model);

		Task<string> Login(BaseLoginViewModel model);

		Task<bool> Logout();
	}

	/// <summary>
	/// User Xamarin connectivity package to test connection status on creation
	/// </summary>
	/// <seealso cref="GenericMvcUtilities.Client.IAuthClient" />
	/// <seealso cref="System.IDisposable" />
	public class AuthClient : IAuthClient, IDisposable
	{
		private readonly HttpClient _client;

		private readonly JsonSerializer _serailizer;

		//private readonly JsonTextWriter _jsonTextWriter;

		private readonly BaseLoginViewModel _loginInfo;

		private readonly bool _cacheLoginInfo;

		public BaseLoginViewModel LoginInfo { get { return _loginInfo; }}

		public IEnumerable<string> Cookies { get; set; }

		public string AuthToken { get; set; }

		public string ApiPath => "/account/";

		public string RegisterRoute => ApiPath + "register";

		public string LoginRoute => ApiPath + "login";

		public string LogOffRoute => ApiPath + "logout";

		public AuthClient(HttpClient client, bool cacheLoginInfo = false)
		{
			if (client != null)
			{
				_client = client;

				_serailizer = JsonSerializer.Create();
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
				//Create the cookie container our client
				var cookieJar = new CookieContainer();

				//Post Password and stuff to Rest Server, Response should contain the password token cookie
				using (HttpResponseMessage response = await this._client.PostAsync(this.LoginRoute,
													new StringContent(JsonConvert.SerializeObject(model))))
				{
					//Grab the cookies
					var cookies = response.Headers.GetValues("Set-Cookie"); //First(x => x.StartsWith(".AspNetCore.Microsoft.Identity.Application"));

					var processedCookie = ProcessCookies(CleanCookies(cookies));

					this.AuthToken = processedCookie;

					return processedCookie;
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
													new StringContent(JsonConvert.SerializeObject(registrationInfo))))
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
				using (HttpResponseMessage response = await this._client.PostAsync(this.LogOffRoute, new StringContent("Log me out")))
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
		#endregion
	}
}