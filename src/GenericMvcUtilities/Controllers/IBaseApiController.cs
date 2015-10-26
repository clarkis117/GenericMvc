using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq.Expressions;

namespace GenericMvcUtilities.Controllers
{
	public interface IBaseApiController<T> where T : class
	{

		[HttpGet]
		Task<IEnumerable<T>> GetAll();

		[HttpGet("{id:int}")]
		Task<T> Get(int? id);

		[HttpPost]
		Task<IActionResult> Create([FromBody] T item);

		[HttpPut("{id:int}")]
		Task<IActionResult> Update(int? id, [FromBody] T item);

		[HttpDelete("{id:int}")]
		Task<IActionResult> Delete(int? id);
	}
}
