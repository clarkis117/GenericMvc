using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class TableViewModel : GenericViewModel
	{
		public string Title { get; set; }

		public string Action { get; set; }

		public string Description { get; set; }

		public bool CreateButton { get; set; }

		public TableViewModel()
		{
			ContainerName = "Table";
		}

		public TableViewModel(ActionContext actionContext)
		{
			ContainerName = "Table";

			Action = actionContext.ActionDescriptor.Name;

			ControllerName = actionContext.RouteData.Values["controller"].ToString();

			NestedView = this.Action;
		}
	}
}
