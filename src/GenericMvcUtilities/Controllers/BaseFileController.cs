using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	public class BaseFileController : Controller
	{
		// GET: /<controller>/
		public IActionResult Index()
		{
			//File.

			return View();
		}
	}
}
