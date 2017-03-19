using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Clients
{
	public class JsonContent : StringContent
	{
		public const string ContentType = "application/json";

		public static readonly Encoding EncodingType = Encoding.UTF8;

		public JsonContent(object content) : base(JsonConvert.SerializeObject(content), EncodingType)
		{
			Headers.ContentType = new MediaTypeHeaderValue(ContentType);
		}
	}
}
