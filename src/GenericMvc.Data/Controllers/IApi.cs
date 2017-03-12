using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using GenericMvc.Models;

namespace GenericMvc.Data.Controllers
{
	/// <summary>
	/// Base Interface for all Web API nodes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IApi<T, TKey> : IReadOnlyApi<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		//todo: maybe a count method?

		//[HttpPost]
		Task<IActionResult> Create([FromBody] T item);

		Task<IActionResult> Creates([FromBody] IEnumerable<T> items);

		//[HttpPut("{id:int}")]
		Task<IActionResult> Update(TKey id, [FromBody] T item);

		//Task<IActionResult> Updates([FromBody] T[] items);

		//[HttpDelete("{id:int}")]
		Task<IActionResult> Delete(TKey id);
	}

	public interface IGraphApi<T, TKey> : IApi<T, TKey>
	where T : class
	where TKey : IEquatable<TKey>
	{
		Task<IActionResult> DeleteChild([FromBody] Newtonsoft.Json.Linq.JObject child);
	}
}
