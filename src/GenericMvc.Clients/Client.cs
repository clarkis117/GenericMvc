using GenericMvc.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public class Client<T, TKey> : ReadOnlyClient<T, TKey>, IClient<T, TKey>, IDisposable
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		#region Properties

		public string CreateRoute => ApiPath + "/create";

		public string CreatesRoute => ApiPath + "/creates";

		public string UpdateRoute => ApiPath + "/update";

		public string DeleteRoute => ApiPath + "/delete";

		#endregion Properties

		public Client(
				HttpClient fixtureClient,
				string authCookie,
				IList<JsonConverter> converters)
			: base(
				fixtureClient,
				authCookie,
				converters)
		{
		}

		#region CrudMethods

		public async Task<HttpResponseMessage> Create(T item, bool useAuth)
		{
			try
			{
				if (item == null)
					throw new ArgumentNullException(nameof(item));

				var content = new JsonContent(item);

				if (useAuth)
					return await SendRequest(CreateRoute, AuthCookie, HttpMethod.Post, content);

				return await SendRequest(CreateRoute, null, HttpMethod.Post, content);
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
				if (contentOverride == null)
					throw new ArgumentNullException(nameof(contentOverride));

				if (useAuth)
					return await SendRequest(CreateRoute, AuthCookie, HttpMethod.Post, contentOverride);

				return await SendRequest(CreateRoute, null, HttpMethod.Post, contentOverride);
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
				if (items == null)
					throw new ArgumentNullException(nameof(items));

				var content = new JsonContent(items);

				if (useAuth)
					return await SendRequest(CreatesRoute, AuthCookie, HttpMethod.Post, content);

				return await SendRequest(CreatesRoute, null, HttpMethod.Post, content);
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
				if (contentOverride == null)
					throw new ArgumentNullException(nameof(contentOverride));

				if (useAuth)
					return await SendRequest(CreatesRoute, AuthCookie, HttpMethod.Post, contentOverride);

				return await SendRequest(CreatesRoute, null, HttpMethod.Post, contentOverride);
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
				if (item == null)
					throw new ArgumentNullException(nameof(item));

				var content = new JsonContent(item);

				if (useAuth)
					return await SendRequest(UpdateRoute + "?id=" + item.Id, AuthCookie, HttpMethod.Post, content);

				return await SendRequest(UpdateRoute + "?id=" + item.Id, null, HttpMethod.Post, content);
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
				if (contentOverride == null)
					throw new ArgumentNullException(nameof(contentOverride));

				if (useAuth)
					return await SendRequest(UpdateRoute + "?id=" + id, AuthCookie, HttpMethod.Post, contentOverride);

				return await SendRequest(UpdateRoute + "?id=" + id, null, HttpMethod.Post, contentOverride);
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
					return await SendRequest(DeleteRoute + "?id=" + id, AuthCookie, HttpMethod.Delete, null);

				return await SendRequest(DeleteRoute + "?id=" + id, null, HttpMethod.Delete, null);
			}
			catch (Exception ex)
			{
				throw new Exception(typeof(T).Name + ": Delete Request Failed", ex);
			}
		}

		#endregion CrudMethods

		public async Task<T> Create(T item)
		{
			try
			{
				if (item == null)
					throw new ArgumentNullException(nameof(item));

				var response = await Create(item, true);

				if (!response.IsSuccessStatusCode)
					throw new Exception("Response returned non-success code");

				if (response.StatusCode == HttpStatusCode.Conflict)
					return null;

				return JsonConvert.DeserializeObject<T>
						(await response.Content.ReadAsStringAsync());
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
				if (items == null)
					throw new ArgumentNullException(nameof(items));

				var response = await Creates(items, true);

				if (!response.IsSuccessStatusCode)
					throw new Exception("Response returned non-success code");

				return JsonConvert.DeserializeObject<IEnumerable<T>>
						(await response.Content.ReadAsStringAsync());
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
				if (item == null)
					throw new ArgumentNullException(nameof(item));

				var response = await Update(item, true);

				return response.IsSuccessStatusCode;
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
				var response = await Delete(id, true);

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
					_client.Dispose();
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
	}
}