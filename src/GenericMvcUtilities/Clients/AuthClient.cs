using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.Client
{
	public class AuthClient : IAuthClient, IDisposable
	{
		private readonly HttpClient _client;

		private readonly ILoginModel _credentials;

		public IEnumerable<string> Cookies { get; set; }

		public string AuthCookie { get; set; }

		public string ApiPath => "/account/";

		public string RegisterRoute => ApiPath + "register";

		public string LoginRoute => ApiPath + "loginapi";

		public string LogOffRoute => ApiPath + "logoff";

		public AuthClient(HttpClient client, ILoginModel loginInfo)
		{
			if (client != null)
			{
				//set client
				this._client = client;


				if (loginInfo != null)
				{
					//set credsJson
					this._credentials = loginInfo;
				}
				else
				{
					throw new ArgumentNullException(nameof(loginInfo));
				}
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

		public static ILoginModel GetLoginInfoFromFile(string filepath)
		{
			ILoginModel valid;

			using (var reader = new StreamReader(System.IO.File.OpenRead(filepath)))
			{
				var serial = new JsonSerializer();

				valid = (ILoginModel)serial.Deserialize(reader, typeof(LoginModel));
			}

			return valid;
		}

		public async Task<string> Login(ILoginModel model)
		{
			if (model != null)
			{
				//Create the cookie container our client
				var cookieJar = new CookieContainer();

				//Post Password and stuff to Rest Server, Response should contain the password token cookie
				HttpResponseMessage response = await this._client.PostAsync(this.LoginRoute, new StringContent(JsonConvert.SerializeObject(model)));

				//Grab the cookies
				var cookies = response.Headers.GetValues("Set-Cookie"); //First(x => x.StartsWith(".AspNetCore.Microsoft.Identity.Application"));

				var processedCookie = ProcessCookies(CleanCookies(cookies));

				this.AuthCookie = processedCookie;

				return processedCookie;
			}
			else
			{
				throw new ArgumentNullException(nameof(model));
			}
		}

		public async Task<string> Login()
		{
			return await this.Login(this._credentials);
		}

		//todo: fix
		public Task<bool> Register()
		{
			throw new NotImplementedException();
		}

		public Task<bool> LogOff()
		{
			throw new NotImplementedException();
		}

		#region IDisposable Support
		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~LoginClient() {
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
