using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq.Expressions;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	public interface IBaseController<TKey, T> 
		where T : class
		where TKey : IEquatable<TKey>
	{
		Task<IActionResult> Index();

		Task<IActionResult> Details(TKey id);

		//Create Pair
		IActionResult Create();

		Task<IActionResult> Create(T item);

		//Edit Pair
		Task<IActionResult> Edit(TKey id);

		Task<IActionResult> Edit(T item);

		
		Task<IActionResult> Delete(TKey id);

		Task<IActionResult> DeleteConfirmed(TKey id);
	}
}
