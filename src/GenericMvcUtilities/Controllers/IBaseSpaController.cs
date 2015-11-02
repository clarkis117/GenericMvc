using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq.Expressions;

namespace GenericMvcUtilities.Controllers
{
	interface IBaseSpaController<T> where T : class
	{
		/// <summary>
		/// Creates the Single Page Application
		/// </summary>
		/// <returns></returns>
		Task<IActionResult> Index();
	}
}
