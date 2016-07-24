using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class GenericViewModel
	{
		public string ControllerName { get; set; }

		/// <summary>
		/// Gets the nested view path by convention.
		/// the convention is: $"~/Views/{ControllerName}/{NestedView}.cshtml"
		/// </summary>
		public virtual string NestedViewPath {
			get { return ($"~/Views/{ControllerName}/{NestedView}.cshtml");}
		}


		protected string ContainerName { get; set; }

		protected virtual string ContainerFolder { get; set; } = "Shared";

		public virtual string ViewContainerPath
		{
			get { return ($"~/Views/{ContainerFolder}/{ContainerName}Container.cshtml"); }
		}

		public virtual string NestedView { get; set; }

		public virtual object Data { get; set; }

		/*
		public GenericViewModel(string controllerName)
		{
			this.ControllerName = controllerName;
		}
		*/
	}

	public static class ViewModelHelper
	{
		public static Microsoft.AspNetCore.Mvc.ViewResult ViewFromModel(this Microsoft.AspNetCore.Mvc.Controller controller, GenericViewModel viewModel)
		{
			return controller.View(viewModel.ViewContainerPath, viewModel);
		}

		public static Microsoft.AspNetCore.Mvc.ViewResult ViewFromModels(this Microsoft.AspNetCore.Mvc.Controller controller, IList<GenericViewModel> viewModels)
		{
			return controller.View("~/Views/Shared/MultiPageContainer.cshtml", viewModels);
		}

		public static Microsoft.AspNetCore.Mvc.ViewResult ViewFromModels(this Microsoft.AspNetCore.Mvc.Controller controller, IList<PageViewModel> viewModels)
		{
			return controller.View("~/Views/Shared/MultiPageContainer.cshtml", viewModels);
		}
	}
}
