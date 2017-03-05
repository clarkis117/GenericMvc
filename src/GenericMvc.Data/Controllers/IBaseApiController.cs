using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using GenericMvc.Models;

namespace GenericMvc.Controllers
{
	/// <summary>
	/// Base Interface for all Web API nodes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBaseApiController<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		//todo: maybe a count method?

		//[HttpGet("{id:int}")]
		Task<IActionResult> Get(TKey id);

		Task<IActionResult> GetMany(string propertyName, string value);

		//[HttpGet]
		Task<IEnumerable<T>> GetAll();

		//[HttpPost]
		Task<IActionResult> Create([FromBody] T item);

		//todo: see if this should return IEnumerable<T>
		//[HttpPost]
		Task<IActionResult> Creates([FromBody] T[] items);

		//[HttpPut("{id:int}")]
		Task<IActionResult> Update(TKey id, [FromBody] T item);

		//Task<IActionResult> Updates([FromBody] T[] items);

		//[HttpDelete("{id:int}")]
		Task<IActionResult> Delete(TKey id);
	}

	public interface IBaseGraphController<T, TKey> : IBaseApiController<T, TKey>
	where T : class
	where TKey : IEquatable<TKey>
	{
		Task<IActionResult> DeleteChild([FromBody] Newtonsoft.Json.Linq.JObject child);
	}


	public interface ISinglePageController<T, TKey> : IBaseGraphController<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		IActionResult Index();
	}
}
