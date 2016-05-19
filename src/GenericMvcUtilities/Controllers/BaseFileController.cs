using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	public interface FileController
	{
		void Get();
		void Create();
		void Update();
		void Delete();
	}


	public class BaseFileController : Controller
	{
		// GET: /<controller>/
		public IActionResult Index()
		{
			//File.

			return View();
		}

		public IActionResult Get()
		{
			return new NoContentResult();
			//return VirtualFileResult
		}
	}
}
