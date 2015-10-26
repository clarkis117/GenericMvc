using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq.Expressions;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	public interface IBaseController<T> where T : class
	{

		IActionResult Create();

		Task<IActionResult> Create(T item);

		Task<IActionResult> Index();

		Task<IActionResult> Details(int? id);

		Task<IEnumerable<T>> GetAll();

		Task<T> Get(int? id);

		Task<IActionResult> Edit(int? id);

		Task<IActionResult> Delete(int? id);
	}
}
