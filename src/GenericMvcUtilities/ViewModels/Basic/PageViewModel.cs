using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using GenericMvcUtilities.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenericMvcUtilities.ViewModels.Basic
{
	public class PageViewModel : BaseViewModel
	{
		public string Title { get; set; }

		public bool DisplayAction { get; set; } = true;

		public string Action { get; set; }

		public string Description { get; set; }

		public MessageViewModel Message { get; set; }

		public PageViewModel()
		{
			ContainerName = "Page";
		}

		public PageViewModel(Controller context)
		{
			ContainerName = "Page";

			ControllerName = context.RouteData.Values["controller"].ToString();


			Action = context.RouteData.Values["action"].ToString();

			/*
			Action = actionContext.ActionDescriptor.Name;

			ControllerName = actionContext.RouteData.Values["controller"].ToString();
			*/

			NestedView = this.Action;
		}
	}

	public class BasicViewModel : PageViewModel
	{
		public object Id { get; set; }

		public const string BasicMvc = "BasicMvc/";

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

		public bool UseNestedViewConventions { get; set; } = true;

		public override string NestedViewPath
		{
			get
			{
				if (UseNestedViewConventions)
				{
					return ($"~/Views/{BasicMvc}{ControllerName}/{NestedView}.cshtml");
				}

				return ($"~/Views/{ControllerName}/{NestedView}.cshtml");
			}
		}

		public BasicViewModel(Controller context) : base(context)
		{

		}
	}

	public class IndexViewModel : BasicViewModel
	{
		private const string IndexContainer = "Index";

		public long Count { get; set; }

		public bool ShowCreateButton { get; set; } = true;

		public bool ShowSearchForm { get; set; } = true;

		public Basic.SearchViewModel SearchViewModel { get; set; }

		public IndexViewModel(Controller context) : base(context)
		{
			ContainerName = BasicMvc + IndexContainer;

			Title = ControllerName;

			Description = $"All {ControllerName} in the Database";
		}
	}

	public class DetailsViewModel : BasicViewModel
	{
		private const string DetailsContainer = "Details";

		public DetailsViewModel(Controller context): base(context)
		{
			ContainerName = BasicMvc + DetailsContainer;

			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"All the data for this, {deplural}";
		}
	}


	public class CreateEditViewModel : BasicViewModel
	{
		protected const string CreateEditFieldset = "CreateEdit";

		/*
		public string CreateEditPath
		{
			get { return ($"~/Views/{ControllerName}/{CreateEditFieldset}.cshtml"); }
		}
		*/

		/*
		public override string NestedViewPath
		{
			get
			{
				 return ($"~/Views/{ContainerFolder}/{NestedView}Container.cshtml"); 
			}
		}
		*/

		public CreateEditViewModel(Controller context) : base(context)
		{
			ContainerName = BasicMvc+CreateEditFieldset;
			NestedView = CreateEditFieldset;
		}
	}

	public class CreateViewModel : CreateEditViewModel
	{
		public CreateViewModel(Controller context) : base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"Create a new, {deplural}";
		}
	}

	public class EditViewModel : CreateEditViewModel
	{
		public EditViewModel(Controller context) : base(context)
		{
			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"A form for editing the data for this, {deplural}";
		}
	}

	public class DeleteViewModel : BasicViewModel
	{
		private const string DeleteContainer = "Delete";

		public DeleteViewModel(Controller context) : base(context)
		{
			ContainerName = BasicMvc + DeleteContainer;

			var deplural = depluralizeController(ControllerName) ?? ControllerName;

			Title = deplural;

			Description = $"Are you sure you want to delete this {deplural}?";
		}
	}
}
