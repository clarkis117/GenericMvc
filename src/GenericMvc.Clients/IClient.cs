using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvc.Models;
using System.Net;
using System.Net.Http;

namespace GenericMvc.Clients
{
	public interface IClient<T, TKey> : IReadOnlyClient<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		string CreateRoute { get; }

		string CreatesRoute { get; }

		string UpdateRoute { get; }

		string DeleteRoute { get; }

		Task<HttpResponseMessage> Create(T item, bool useAuth);

		Task<HttpResponseMessage> Create(bool useAuth, HttpContent contentOverride);

		Task<HttpResponseMessage> Creates(IEnumerable<T> item, bool useAuth);

		Task<HttpResponseMessage> Creates(bool useAuth, HttpContent contentOverride);

		Task<HttpResponseMessage> Update(T item, bool useAuth);

		Task<HttpResponseMessage> Update(bool useAuth, TKey id, HttpContent contentOverride);

		Task<HttpResponseMessage> Delete(TKey id, bool useAuth);

		Task<T> Create(T item);

		Task<IEnumerable<T>> Creates(IEnumerable<T> items);

		Task<bool> Update(T item);

		Task<bool> Delete(TKey id);
	}
}
