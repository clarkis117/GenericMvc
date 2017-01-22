using GenericMvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	public class BasicModelsController : Controller
	{
		//protected static List<>

		public static void Register(Controller controller)
		{
			if (controller == null)
				throw new ArgumentNullException(nameof(controller));

			var controllerName = controller.RouteData.Values["controller"].ToString();
		}

		public BasicModelsController()
		{

		}
	}
}
