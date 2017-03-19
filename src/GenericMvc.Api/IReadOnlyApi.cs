using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Api
{
	public interface IReadOnlyApi<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		Task<IActionResult> Get(TKey id);

		Task<IActionResult> GetMany(string propertyName, string value);

		Task<IEnumerable<T>> GetAll();
	}
}
