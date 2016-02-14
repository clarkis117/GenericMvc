using GenericMvcUtilities.Models;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Tests
{
	public interface IBaseApiTest<T> where T : IModel
	{
		Task<string> GetTestData(string path);

		Task<T> MutateData(T item);

		Task<bool> CheckData(T item);

		Task<bool> CompareData(T item1, T item2);

		//Test Senerios, these are authenticated

		/// <summary>
		/// checks and see if it can complete a successful get request, and that it returns correct data
		/// </summary>
		/// <returns></returns>
		Task Get();

		Task GetAll();

		Task Create();

		Task Creates();

		Task Update();

		Task Delete();

		/// <summary>
		/// Creates and deletes an object.
		/// </summary>
		/// <returns></returns>
		Task CreatesDeleteTest();

		/// <summary>
		/// Does a create, update, retrieve, and delete scenario.
		/// </summary>
		/// <returns></returns>
		Task CrudTest();

		//Empty Post/Put requests, these are authenticated

		/// <summary>
		/// Sends an empty create request.
		/// </summary>
		/// <returns></returns>
		Task EmptyCreate();

		/// <summary>
		/// Sends an empty creates request.
		/// </summary>
		/// <returns></returns>
		Task EmptyCreates();

		/// <summary>
		/// Sends and empty update request.
		/// </summary>
		/// <returns></returns>
		Task EmptyUpdate();

		//Bad Requests... post/puts test with bad data
		//get/deletes/bodyless test with malformed requests
		/// <summary>
		/// Sends a create request with bad data
		/// </summary>
		/// <returns></returns>
		Task BadCreate();

		/// <summary>
		/// Sends a creates request with bad data
		/// </summary>
		/// <returns></returns>
		Task BadCreates();

		/// <summary>
		/// Sends a get with a malformed url.
		/// </summary>
		/// <returns></returns>
		Task BadGet();

		/// <summary>
		/// Send a bad get request with malformed url.
		/// </summary>
		/// <returns></returns>
		Task BadGetAll();

		/// <summary>
		/// Sends a update request with a bad payload
		/// </summary>
		/// <returns></returns>
		Task BadUpdate();

		/// <summary>
		/// Send a bad delete request with malformed url.
		/// </summary>
		/// <returns></returns>
		Task BadDelete();

		//Unauthenticated reuests
		Task UnauthCreate();

		Task UnauthCreates();

		Task UnauthGet(); //should work without auth

		Task UnauthGetAll(); //should work without auth

		Task UnauthUpdate();

		Task UnauthDelete();
	}
}