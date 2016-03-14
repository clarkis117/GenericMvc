using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class GenericViewModel
	{
		/// <summary>
		/// Gets or sets the name of the controller.
		/// </summary>
		/// <value>
		/// The name of the controller.
		/// </value>
		public string ControllerName { get; set; }

		/// <summary>
		/// Gets the nested view path by convention.
		/// the convention is: $"~/Views/{ControllerName}/{NestedView}.cshtml"
		/// </summary>
		/// <value>
		/// The nested view path.
		/// </value>
		public virtual string NestedViewPath {
			get { return ($"~/Views/{ControllerName}/{NestedView}.cshtml");}
		}

		public virtual string NestedView { get; set; }

		public object Data { get; set; }


		/*
		public GenericViewModel(string controllerName)
		{
			this.ControllerName = controllerName;
		}
		*/
	}
}
