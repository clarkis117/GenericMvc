using Newtonsoft.Json;
using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public class ApiClient<T, TKey> : IDisposable, IApiClient<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		#region Fields

		private readonly HttpClient _client;

		public readonly string AuthCookie;

		public readonly Encoding EncodingType = Encoding.UTF8;

		public readonly string MimeType = "application/json";

		#endregion Fields

		#region Properties

		public string Protocal { get; set; } = "http://";

		public string HostName { get; set; } = "localhost";

		public string HostUrl => this.Protocal + this.HostName;

		//Begin Routes
		public string ApiPath => "/api/" + typeof(T).Name;

		public string GetAllRoute => ApiPath + "/getall";

		public string CreateRoute => ApiPath + "/create";

		public string CreatesRoute => ApiPath + "/creates";

		public string GetRoute => ApiPath + "/get";

		public string UpdateRoute => ApiPath + "/update";

		public string DeleteRoute => ApiPath + "/delete";


		//End Routes

		#endregion Properties

		public ApiClient(HttpClient fixtureClient, string authCookie, IList<JsonConverter> converters)
		{
			try
			{
				//set up httpclient
				if (fixtureClient != null)
				{
					this._client = fixtureClient;

					this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
				}
				else
				{
					throw new ArgumentNullException(nameof(fixtureClient));
				}

				//setup JsonConvert
				Func<JsonSerializerSettings> settingsMethod = () =>
				{
					var converterlist = converters;

					JsonSerializerSettings settings = new JsonSerializerSettings
					{
						Converters = converterlist,
					};

					return settings;
				};

				JsonConvert.DefaultSettings = settingsMethod;

				//login to rest and get cookie
				this.AuthCookie = authCookie;

			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + " Constructor Failed", ex);
			}
		}

		#region CrudMethods

		/// <summary>
		/// Sends the request.
		/// </summary>
		/// <param name="route">The route.</param>
		/// <param name="method">The method.</param>
		/// <param name="concatCookies">The concat cookies.</param>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Sending Http Request Failed</exception>
		public async Task<HttpResponseMessage> SendRequest(string route, string concatCookies, HttpMethod method, HttpContent content)
		{
			try
			{
				var message = new HttpRequestMessage(method, route);

				//set headers
				if (concatCookies != null)
				{
					message.Headers.Add("Cookie", concatCookies);
				}

				//set content
				if (content != null)
				{
					message.Content = content;
				}

				return await this._client.SendAsync(message);
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Sending http Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Get(TKey id, bool useAuth)
		{
			try
			{
				if (useAuth)
				{
					return await this.SendRequest(this.GetRoute + "?id=" + id, this.AuthCookie, HttpMethod.Get, null);
				}
				else
				{
					return await this.SendRequest(this.GetRoute + "?id=" + id, null, HttpMethod.Get, null);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> GetAll(bool useAuth)
		{
			try
			{
				if (useAuth)
				{
					return await this.SendRequest(this.GetAllRoute, this.AuthCookie, HttpMethod.Get, null);
				}
				else
				{
					return await this.SendRequest(this.GetAllRoute, null, HttpMethod.Get, null);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Create(T item, bool useAuth)
		{
			try
			{
				if (item != null)
				{
					var content = new StringContent(JsonConvert.SerializeObject(item), EncodingType, MimeType);

					if (useAuth)
					{
						return await this.SendRequest(this.CreateRoute, this.AuthCookie, HttpMethod.Post, content);
					}
					else
					{
						return await this.SendRequest(this.CreateRoute, null, HttpMethod.Post, content);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(item));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Create Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Create(bool useAuth, HttpContent contentOverride)
		{
			try
			{
				if (contentOverride != null)
				{
					if (useAuth)
					{
						return await this.SendRequest(this.CreateRoute, this.AuthCookie, HttpMethod.Post, contentOverride);
					}
					else
					{
						return await this.SendRequest(this.CreateRoute, null, HttpMethod.Post, contentOverride);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(contentOverride));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Create Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Creates(IEnumerable<T> items, bool useAuth)
		{
			try
			{
				if (items != null)
				{
					var content = new StringContent(JsonConvert.SerializeObject(items), EncodingType, MimeType);

					if (useAuth)
					{
						return await this.SendRequest(this.CreatesRoute, this.AuthCookie, HttpMethod.Post, content);
					}
					else
					{
						return await this.SendRequest(this.CreatesRoute, null, HttpMethod.Post, content);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(items));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Creates Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Creates(bool useAuth, HttpContent contentOverride)
		{
			try
			{
				if (contentOverride != null)
				{
					if (useAuth)
					{
						return await this.SendRequest(this.CreatesRoute, this.AuthCookie, HttpMethod.Post, contentOverride);
					}
					else
					{
						return await this.SendRequest(this.CreatesRoute, null, HttpMethod.Post, contentOverride);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(contentOverride));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Creates Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Update(T item, bool useAuth)
		{
			try
			{
				if (item != null)
				{
					var content = new StringContent(JsonConvert.SerializeObject(item), EncodingType, MimeType);

					if (useAuth)
					{
						return await this.SendRequest(this.UpdateRoute + "?id=" + item.Id, this.AuthCookie, HttpMethod.Post, content);
					}
					else
					{
						return await this.SendRequest(this.UpdateRoute + "?id=" + item.Id, null, HttpMethod.Post, content);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(item));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Update Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Update(bool useAuth, TKey id, HttpContent contentOverride)
		{
			try
			{
				if (contentOverride != null)
				{
					if (useAuth)
					{
						return await this.SendRequest(this.UpdateRoute + "?id=" + id, this.AuthCookie, HttpMethod.Post, contentOverride);
					}
					else
					{
						return await this.SendRequest(this.UpdateRoute + "?id=" + id, null, HttpMethod.Post, contentOverride);
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(contentOverride));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Update Request Failed", ex);
			}
		}

		public async Task<HttpResponseMessage> Delete(TKey id, bool useAuth)
		{
			try
			{
				if (useAuth)
				{
					return await this.SendRequest(this.DeleteRoute + "?id=" + id, this.AuthCookie, HttpMethod.Delete, null);
				}
				else
				{
					return await this.SendRequest(this.DeleteRoute + "?id=" + id, null, HttpMethod.Delete, null);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Delete Request Failed", ex);
			}
		}

		#endregion CrudMethods

		#region EasyCrudMethods

		public async Task<T> Get(TKey id)
		{
			try
			{
				var response = await this.Get(id, true);

				if (response.IsSuccessStatusCode)
				{
					var item = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(T));

					return (T) item;
				}
				else if (response.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}
				else
				{
					throw new Exception("Response returned non-success code");
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get Request Failed", ex);
			}
		}

		public async Task<IEnumerable<T>> GetAll()
		{
			try
			{
				var response = await this.GetAll(true);

				if (response.IsSuccessStatusCode)
				{
					var items = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(IEnumerable<T>));

					return (IEnumerable<T>) items;
				}
				else
				{
					throw new Exception("Response returned non-success code");
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get All Request Failed", ex);
			}
		}

		public async Task<T> Create(T item)
		{
			try
			{
				if (item != null)
				{
					var response = await this.Create(item, true);

					if (response.IsSuccessStatusCode)
					{
						if (response.StatusCode != HttpStatusCode.Conflict)
						{
							var createdItem = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(T));

							return (T) createdItem;
						}
						else
						{
							return null;
						}
					}
					else
					{
						throw new Exception("Response returned non-success code");
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(item));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Create Request Failed", ex);
			}
		}

		public async Task<IEnumerable<T>> Creates(IEnumerable<T> items)
		{
			try
			{
				if (items != null)
				{
					var response = await this.Creates(items, true);

					if (response.IsSuccessStatusCode)
					{
						var createdItems = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(IEnumerable<T>));

						return (IEnumerable<T>) createdItems;
					}
					else
					{
						throw new Exception("Response returned non-success code");
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(items));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Creates Request Failed", ex);
			}
		}

		public async Task<bool> Update(T item)
		{
			try
			{
				if (item != null)
				{
					var response = await this.Update(item, true);

					return response.IsSuccessStatusCode;
				}
				else
				{
					throw new ArgumentNullException(nameof(item));
				}
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Update Request Failed", ex);
			}
		}

		public async Task<bool> Delete(TKey id)
		{
			try
			{
				var response = await this.Delete(id, true);

				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Delete Request Failed", ex);
			}
		}

		#region IDisposable Support

		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					//dispose managed state (managed objects).
					this._client.Dispose();
				}

				// free unmanaged resources (unmanaged objects) and override a finalizer below.
				// set large fields to null.

				_disposedValue = true;
			}
		}

		// override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~TestClient() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support

		#endregion EasyCrudMethods
	}
}