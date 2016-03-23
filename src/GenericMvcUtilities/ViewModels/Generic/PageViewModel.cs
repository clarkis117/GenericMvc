using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class PageViewModel : GenericViewModel
	{
		public string Title { get; set; }

		public string Action { get; set; }

		public string Description { get; set; }

		public PageViewModel()
		{
			ContainerName = "Page";
		}

		public PageViewModel(ActionContext actionContext)
		{
			ContainerName = "Page";

			Action = actionContext.ActionDescriptor.Name;

			ControllerName = actionContext.RouteData.Values["controller"].ToString();

			NestedView = this.Action;
		}
	}
}
