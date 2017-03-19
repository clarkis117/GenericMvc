using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public interface IReadOnlyClient<T, TKey>
	{
		string Protocal { get; set; }

		string HostName { get; set; }

		string HostUrl { get; }

		string ApiPath { get; }

		string GetAllRoute { get; }

		string GetRoute { get; }

		Task<HttpResponseMessage> SendRequest(string route, string concatCookies, HttpMethod method, HttpContent content);

		Task<HttpResponseMessage> Get(TKey id, bool useAuth);

		Task<HttpResponseMessage> GetAll(bool useAuth);

		Task<T> Get(TKey id);

		Task<IEnumerable<T>> GetAll();
	}
}
