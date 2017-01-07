using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvc.Models;
using System.Net;
using System.Net.Http;

namespace GenericMvc.Client
{
	public interface IApiClient<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		//Begin Routes
		string Protocal { get; set; }

		string HostName { get; set; }

		string HostUrl { get; }

		string ApiPath { get; }

		string GetAllRoute { get; }

		string CreateRoute { get; }

		string CreatesRoute { get; }

		string GetRoute { get; }

		string UpdateRoute { get; }

		string DeleteRoute { get; }
		//End Routes

		//Uility CRUD Methods
		Task<HttpResponseMessage> SendRequest(string route, string concatCookies, HttpMethod method, HttpContent content);

		Task<HttpResponseMessage> Get(TKey id, bool useAuth);

		Task<HttpResponseMessage> GetAll(bool useAuth);

		Task<HttpResponseMessage> Create(T item, bool useAuth);

		Task<HttpResponseMessage> Create(bool useAuth, HttpContent contentOverride);

		Task<HttpResponseMessage> Creates(IEnumerable<T> item, bool useAuth);

		Task<HttpResponseMessage> Creates(bool useAuth, HttpContent contentOverride);

		Task<HttpResponseMessage> Update(T item, bool useAuth);

		Task<HttpResponseMessage> Update(bool useAuth, TKey id, HttpContent contentOverride);

		Task<HttpResponseMessage> Delete(TKey id, bool useAuth);

		//All these use auth
		Task<T> Get(TKey id);

		Task<IEnumerable<T>> GetAll();

		Task<T> Create(T item);

		Task<IEnumerable<T>> Creates(IEnumerable<T> items);

		Task<bool> Update(T item);

		Task<bool> Delete(TKey id);
	}
}
