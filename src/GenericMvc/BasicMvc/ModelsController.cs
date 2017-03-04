using GenericMvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericMvc.Controllers
{
	public class BasicModelsController : Controller
	{
		private static Type _appAssembly;

		private static Assembly _currentAssembly;

		public static Assembly CurrentAssembly { get { return _currentAssembly; } }

		public static void Initialize(Type typeInCurrentAssembly)
		{
			if (typeInCurrentAssembly == null)
				throw new ArgumentNullException(nameof(typeInCurrentAssembly));

			_appAssembly = typeInCurrentAssembly;

			_currentAssembly = typeInCurrentAssembly.GetTypeInfo().Assembly;

			var types = CurrentAssembly.GetTypes();

			var controls = types.Where(x => x.GetTypeInfo().GetInterfaces()
			   .Any(y => y == typeof(IBaseController)) && !x.GetTypeInfo().IsAbstract);

			foreach (var item in controls)
			{
				Register(item);
			}
		}

		public struct ViewModel
		{
			public string Name { get; }

			public Type Controller { get; }

			public ViewModel(string name, Type type)
			{
				Name = name;
				Controller = type;
			}
		}

		protected static List<ViewModel> viewModels = new List<ViewModel>();

		public static void Register(Type controller)
		{
			if (controller == null)
				throw new ArgumentNullException(nameof(controller));

			var controllerName = "Controller";

			string name = controller.Name;

			if (controllerName.EndsWith(controllerName, StringComparison.OrdinalIgnoreCase))
				name = name.Substring(0, name.Length - controllerName.Length);

			var viewModel = new ViewModel(name, controller);

			viewModels.Add(viewModel);
		}

		[Route("[controller]/[action]/"), HttpGet]
		public IActionResult Index()
		{
			return View("~/Views/Shared/BasicMvc/Models.cshtml", viewModels);
		}
	}
}
