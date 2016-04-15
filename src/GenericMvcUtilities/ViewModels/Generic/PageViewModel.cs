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

	public class BasicViewModel : PageViewModel
	{
		protected static string depluralizeController(string controllerName)
		{
			string conName = "";

			if (controllerName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
			{
				conName = controllerName.Substring(0, controllerName.Length - "s".Length);

				return conName;
			}
			else
			{
				return null;
			}
		}

		public override string NestedViewPath
		{
			get
			{
				return ($"~/Views/BasicMvc/{ControllerName}/{NestedView}.cshtml");
			}
		}

		public BasicViewModel(ActionContext context) : base(context)
		{

		}
	}

	public class IndexViewModel : BasicViewModel
	{
		public IndexViewModel(ActionContext context) : base(context)
		{
			Title = ControllerName;

			Description = $"All {ControllerName} in the Database";
		}
	}

	public class DetailsViewModel : BasicViewModel
	{
		public DetailsViewModel(ActionContext context): base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"All the data for this, {deplural}";
		}
	}

	public class CreateViewModel : BasicViewModel
	{
		public CreateViewModel(ActionContext context) : base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"Create a new, {deplural}";
		}
	}

	public class EditViewModel : BasicViewModel
	{
		public EditViewModel(ActionContext context) : base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"A form for editing the data for this, {deplural}";
		}
	}

	public class DeleteViewModel : BasicViewModel
	{
		public DeleteViewModel(ActionContext context) : base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"Are you sure you want to delete this, {deplural}";
		}
	}
}
