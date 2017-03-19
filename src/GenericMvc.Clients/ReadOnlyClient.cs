using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public class ReadOnlyClient<T, TKey> : IReadOnlyClient<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		protected readonly HttpClient _client;

		public readonly string AuthCookie;

		public readonly string MimeType = "application/json";

		public string Protocal { get; set; } = "http://";

		public string HostName { get; set; } = "localhost";

		public string HostUrl => Protocal + HostName;

		public string ApiPath => "/api/" + typeof(T).Name;

		public string GetAllRoute => ApiPath + "/getall";

		public string GetRoute => ApiPath + "/get";

		public ReadOnlyClient(
			HttpClient fixtureClient,
			string authCookie,
			IList<JsonConverter> converters)
		{
			try
			{
				_client = fixtureClient ?? throw new ArgumentNullException(nameof(fixtureClient));

				_client.DefaultRequestHeaders
					.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				_client.DefaultRequestHeaders
					.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

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
				AuthCookie = authCookie;
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + " Constructor Failed", ex);
			}
		}

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
					message.Headers.Add("Cookie", concatCookies);

				//set content
				if (content != null)
					message.Content = content;

				return await _client.SendAsync(message);
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
					return await SendRequest(GetRoute + "?id=" + id, AuthCookie, HttpMethod.Get, null);

				return await SendRequest(GetRoute + "?id=" + id, null, HttpMethod.Get, null);
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
					return await SendRequest(GetAllRoute, AuthCookie, HttpMethod.Get, null);

				return await SendRequest(GetAllRoute, null, HttpMethod.Get, null);
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get Request Failed", ex);
			}
		}

		public async Task<T> Get(TKey id)
		{
			try
			{
				var response = await this.Get(id, true);

				if (response.IsSuccessStatusCode)
				{
					return JsonConvert.DeserializeObject<T>
						(await response.Content.ReadAsStringAsync());
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
				var response = await GetAll(true);

				if (!response.IsSuccessStatusCode)
					throw new Exception("Response returned non-success code");

				return JsonConvert.DeserializeObject<IEnumerable<T>>
					(await response.Content.ReadAsStringAsync());
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Get All Request Failed", ex);
			}
		}
	}
}