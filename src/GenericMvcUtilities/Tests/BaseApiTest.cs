using GenericMvcUtilities.Client;
using GenericMvcUtilities.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GenericMvcUtilities.Tests
{
	//todo: fix url concat problem in bad request series of tests... append bad url on to apipath
	public abstract class BaseApiTest<T, TKey> : IDisposable, IBaseApiTest<T>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		#region Fields

		private readonly ApiClient<T, TKey> _client;

		protected const int NumberOfWhiteListedObjects = 2;

		protected readonly ILogger Logger;

		//bad urls
		protected static readonly string[] BadDeleteUrls = { "/delete", "/delete?", "/delete?id", "/delete?id=", "/delete?id=a", "/delete?id=alllllll" };

		protected static readonly string[] BadGetUrls = { "/get", "/get?", "/get?id", "/get?id=", "/get?id=a", "/get?id=adaldskjkd" };

		protected static readonly string[] BadUpdateUrls = { "/update", "/update?", "/update?id", "/update?id=", "/update?id=a", "/update?id=aaddlskdksl" };

		//http verbs
		protected static readonly HttpMethod[] Verbs = { HttpMethod.Get, HttpMethod.Options,
			HttpMethod.Post, HttpMethod.Put, HttpMethod.Trace, HttpMethod.Head, HttpMethod.Delete };

		#endregion Fields

		#region Properties

		public string TestDataPath { get; } = "JSON\\" + typeof(T).Name + ".json";

		public string BadDataPath { get; } = "JSON\\" + "Bad" + typeof(T).Name + ".json";

		public string SerializedTestData { get; set; }

		public T DeserializedTestData { get; set; }

		public T CreatedTestData { get; set; }

		public List<TKey> WhiteListedTestDataIds { get; set; }

		public IList<JsonConverter> Converters { get; set; }

		#endregion Properties

		public BaseApiTest(ITestFixture fixture, IList<JsonConverter> converters)
		{
			if (fixture != null)
			{
				//setup logger
				this.Logger = fixture.LogFactory.CreateLogger(this.GetType().Name);

				//setup white list
				WhiteListedTestDataIds = new List<TKey>();

				//setup converters
				this.Converters = converters;

				//get the client
				this._client = new ApiClient<T, TKey>(fixture.TestServer.CreateClient(), fixture.AuthCookie, converters);

				//setup serialized test data
				this.SerializedTestData = this.GetTestData(this.TestDataPath).Result;

				//set deserialized test data, create at least two objects and record ids
				//this.DeserializedTestData = (T)JsonConvert.DeserializeObject(this.SerializedTestData, typeof(T), this.Converters.ToArray());
				using (var jsonReader = new JsonTextReader(new StringReader(this.SerializedTestData)))
				{
					var serial = JsonSerializer.Create(new JsonSerializerSettings()
					{
						Converters = converters,
						TypeNameHandling = TypeNameHandling.Auto
					});

					this.DeserializedTestData = (T)serial.Deserialize(jsonReader, typeof(T));
				}

				//create white listed data
				for (int i = 0; i <= NumberOfWhiteListedObjects; i++)
				{
					var response = this._client.Create(DeserializedTestData).Result;

					if (response != null)
					{
						if (i == 0)
						{
							this.CreatedTestData = response;
						}

						this.WhiteListedTestDataIds.Add(response.Id);
					}
					else
					{
						throw new NullReferenceException("Constructor data creation failed");
					}
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(fixture), "Constructor setup failed");
			}
		}

		#region Utility

		private void LogAndThrowException(string message, Exception innerException)
		{
			this.Logger.LogError(message, innerException);

			throw new Exception(message, innerException);
		}

		private static string Arrayify(string serializedData, int count)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("[ ");

			for (int i = 0; i < count; i++)
			{
				builder.Append(serializedData);
				builder.Append(", ");
			}

			builder.Append(" ]");

			return builder.ToString();
		}

		public virtual Task<bool> CheckData(T item)
		{
			return Task.Run(() => { return this.WhiteListedTestDataIds.Exists(x => x.Equals(item.Id)); });
		}

		public abstract Task<bool> CompareData(T item1, T item2);

		public async Task<string> GetTestData(string path)
		{
			using (StreamReader reader = new StreamReader(System.IO.File.OpenRead(path)))
			{
				return await reader.ReadToEndAsync();
			}
		}

		public abstract Task<T> MutateData(T item);

		#endregion Utility

		[Fact]
		public async Task BadCreate()
		{
			try
			{
				var baddata = await this.GetTestData(this.BadDataPath);

				var response = await this._client.Create(true, new StringContent(baddata, this._client.EncodingType, this._client.MimeType));

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Create Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task BadCreates()
		{
			try
			{
				var baddatum = await this.GetTestData(this.BadDataPath);

				var baddata = Arrayify(baddatum, 2);

				var response = await this._client.Create(true, new StringContent(baddata, this._client.EncodingType, this._client.MimeType));

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Creates Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task BadDelete()
		{
			try
			{
				var excludedMethod = Verbs.Where(x => x != HttpMethod.Delete);

				foreach (var url in BadDeleteUrls)
				{
					foreach (var verb in excludedMethod)
					{
						var response = await this._client.SendRequest(_client.ApiPath+url, this._client.AuthCookie, verb, null);

						Assert.NotNull(response);

						Assert.False(response.IsSuccessStatusCode);

						Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);
					}
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Delete Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task BadGet()
		{
			try
			{
				var excludedMethod = Verbs.Where(x => x != HttpMethod.Get);

				foreach (var url in BadGetUrls)
				{
					foreach (var verb in excludedMethod)
					{
						var response = await this._client.SendRequest(_client.ApiPath + url, this._client.AuthCookie, verb, null);

						Assert.NotNull(response);

						Assert.False(response.IsSuccessStatusCode);

						Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);
					}
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Get Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task BadGetAll()
		{
			try
			{
				var badverbs = Verbs.Where(x => x != HttpMethod.Get);

				foreach (var item in badverbs)
				{
					var response = await this._client.SendRequest(this._client.GetAllRoute, this._client.AuthCookie, item, null);

					Assert.NotNull(response);

					Assert.False(response.IsSuccessStatusCode);

					Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Get All Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task BadUpdate()
		{
			try
			{
				var baddatum = await this.GetTestData(this.BadDataPath);

				var excludedMethod = Verbs.Where(x => x != HttpMethod.Put);

				foreach (var url in BadUpdateUrls)
				{
					foreach (var verb in excludedMethod)
					{
						var response = await this._client.SendRequest(_client.ApiPath + url, this._client.AuthCookie, verb,
									new StringContent(baddatum, this._client.EncodingType, this._client.MimeType));

						Assert.NotNull(response);

						Assert.False(response.IsSuccessStatusCode);

						Assert.True((int)response.StatusCode <= 400);
					}
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Bad Update Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task CreatesDeleteTest()
		{
			try
			{
				var content = new StringContent(this.SerializedTestData, this._client.EncodingType, this._client.MimeType);

				var createResponse = await this._client.Create(true, content);

				Assert.NotNull(createResponse);

				Assert.True(createResponse.IsSuccessStatusCode);

				Assert.True(createResponse.StatusCode == System.Net.HttpStatusCode.Created);

				Assert.NotNull(createResponse.Content);

				var created = (T)JsonConvert.DeserializeObject(await createResponse.Content.ReadAsStringAsync(), typeof(T), this.Converters.ToArray());

				Assert.NotNull(created);

				//Assert.True(created.Id != 0);

				var deleteResponse = await this._client.Delete(created.Id, true);

				Assert.NotNull(deleteResponse);

				Assert.True(deleteResponse.IsSuccessStatusCode);

				Assert.True(deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Create Delete Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task CrudTest()
		{
			try
			{
				var content = new StringContent(this.SerializedTestData, this._client.EncodingType, this._client.MimeType);

				var createResponse = await this._client.Create(true, content);

				Assert.NotNull(createResponse);

				Assert.True(createResponse.IsSuccessStatusCode);

				Assert.True(createResponse.StatusCode == System.Net.HttpStatusCode.Created);

				Assert.NotNull(createResponse.Content);

				var created = (T)JsonConvert.DeserializeObject(await createResponse.Content.ReadAsStringAsync(), typeof(T), this.Converters.ToArray());

				Assert.NotNull(created);

				//todo: re evalutate this
				//Assert.True(created.Id > 0);

				this.WhiteListedTestDataIds.Add(created.Id);

				//get, compare, then mutate, and finally update
				var getResponse = await this._client.Get(created.Id);

				Assert.NotNull(getResponse);

				//todo: re evalutate this
				//Assert.True(getResponse.Id > 0);

				Assert.True(await this.CompareData(created, getResponse));

				var mutatedData = await this.MutateData(created);

				var mutatedContent = new StringContent(JsonConvert.SerializeObject(mutatedData), this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Update(true, mutatedData.Id, mutatedContent);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.NoContent);

				var deleteResponse = await this._client.Delete(created.Id, true);

				Assert.NotNull(deleteResponse);

				Assert.True(deleteResponse.IsSuccessStatusCode);

				Assert.True(deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Create Retrieve Update Delete Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task EmptyCreate()
		{
			try
			{
				var response = await this._client.Create(true, new StringContent("", this._client.EncodingType, this._client.MimeType));

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Empty Create Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task EmptyCreates()
		{
			try
			{
				var response = await this._client.Creates(true, new StringContent("", this._client.EncodingType, this._client.MimeType));

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Empty Creates Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task EmptyUpdate()
		{
			try
			{
				var response = await this._client.Update(true, this.WhiteListedTestDataIds.First(), new StringContent("", this._client.EncodingType, this._client.MimeType));

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Empty Update Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task GetAll()
		{
			try
			{
				var response = await this._client.GetAll(true);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Content);

				var items = (IEnumerable<T>)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(IEnumerable<T>));

				Assert.NotNull(items);

				Assert.NotEmpty(items);

				//Check data
				foreach (var id in this.WhiteListedTestDataIds)
				{
					Assert.True(items.Any(x => x.Id.Equals(id)));
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Get All Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task Get()
		{
			try
			{
				var response = await this._client.Get(this.WhiteListedTestDataIds.First(), true);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Content);

				var item = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(T));

				Assert.NotNull(item);

				Assert.True(item is T);

				Assert.True(await this.CheckData((T)item));
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Get Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task Create()
		{
			try
			{
				var content = new StringContent(this.SerializedTestData, this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Create(true, content);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created);

				//read and deserialize
				var item = (T)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(T));

				Assert.NotNull(item);

				Assert.True(item is T);

				//then add Id to white list
				this.WhiteListedTestDataIds.Add(item.Id);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Create Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task Creates()
		{
			try
			{
				var content = new StringContent(Arrayify(this.SerializedTestData, 20), this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Creates(true, content);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created);

				//read and deserialize
				var items = (IEnumerable<T>)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), typeof(IEnumerable<T>));

				Assert.NotNull(items);

				Assert.NotEmpty(items);

				Assert.True(items is IEnumerable<T>);

				//then add Ids to white list
				foreach (var item in items)
				{
					this.WhiteListedTestDataIds.Add(item.Id);
				}
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Creates Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task Update()
		{
			try
			{
				var mutatedData = await this.MutateData(this.CreatedTestData);

				var content = new StringContent(JsonConvert.SerializeObject(mutatedData), this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Update(true, mutatedData.Id, content);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.NoContent);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Update Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task Delete()
		{
			try
			{
				var response = await this._client.Delete(this.WhiteListedTestDataIds.First(), true);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.NoContent);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Delete Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		/// <summary>
		/// Unauthenticated create request should fail
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task UnauthCreate()
		{
			try
			{
				var content = new StringContent(this.SerializedTestData, this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Create(false, content);

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Found);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Create Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		/// <summary>
		/// Unauthenticated Creates request should fail
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task UnauthCreates()
		{
			try
			{
				var content = new StringContent(Arrayify(this.SerializedTestData, 2), this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Creates(false, content);

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Found);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Creates Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		/// <summary>
		/// Unauthenticated delete request should fail
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task UnauthDelete()
		{
			try
			{
				var response = await this._client.Delete(this.WhiteListedTestDataIds.First(), false);

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Found);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Delete Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		/// <summary>
		/// Unauthenticated get should succeed
		/// </summary>
		/// <returns></returns>
		[Fact]
		public virtual async Task UnauthGet()
		{
			try
			{
				var response = await this._client.Get(this.WhiteListedTestDataIds.First(), false);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Get Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		/// <summary>
		/// get all should succeed unauthenticated
		/// </summary>
		/// <returns></returns>
		[Fact]
		public virtual async Task UnauthGetAll()
		{
			try
			{
				var response = await this._client.GetAll(false);

				Assert.NotNull(response);

				Assert.True(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Get All Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		[Fact]
		public async Task UnauthUpdate()
		{
			try
			{
				var mutatedData = await this.MutateData(this.DeserializedTestData);

				var content = new StringContent(JsonConvert.SerializeObject(mutatedData), this._client.EncodingType, this._client.MimeType);

				var response = await this._client.Update(false, this.WhiteListedTestDataIds.First(), content);

				Assert.NotNull(response);

				Assert.False(response.IsSuccessStatusCode);

				Assert.True(response.StatusCode == System.Net.HttpStatusCode.Found);
			}
			catch (Exception ex) when (ex.GetType() != typeof(Xunit.Sdk.XunitException))
			{
				var message = "Unauthenticated Update Test Failed";

				this.LogAndThrowException(message, ex);
			}
		}

		#region IDisposable Support

		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				//delete white listed items
				foreach (var id in this.WhiteListedTestDataIds)
				{
					var result = this._client.Delete(id, true).Result;

					if (result.IsSuccessStatusCode)
					{
					}
				}

				if (disposing)
				{
					//dispose managed state (managed objects).
					this._client.Dispose();

				}

				// free unmanaged resources (unmanaged objects) and override a finalizer below.

				// set large fields to null.
				this.SerializedTestData = null;

				this.DeserializedTestData = null;

				this.CreatedTestData = null;

				this.WhiteListedTestDataIds = null;

				this.Converters = null;

				_disposedValue = true;
			}
		}

		// override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~BaseApiTest() {
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