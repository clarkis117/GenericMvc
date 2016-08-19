using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	public interface IBaseController<TKey, T> 
		where T : class
		where TKey : IEquatable<TKey>
	{
		//http get
		Task<IActionResult> Index(Message? message);

		//http get
		Task<IActionResult> Search(string PropertyName, string Value);

		//http get
		Task<IActionResult> Details(TKey id, Message? message);

		//http get
		IActionResult Create(Message? message);

		//http post
		Task<IActionResult> Create(T item);

		//http get
		Task<IActionResult> Edit(TKey id, Message? message);

		//http post
		Task<IActionResult> Edit(T item);

		//http get
		Task<IActionResult> Delete(TKey id, Message? message);

		//http post
		Task<IActionResult> DeleteConfirmed(TKey id);
	}
}
