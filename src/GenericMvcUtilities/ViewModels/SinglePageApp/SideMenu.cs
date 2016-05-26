using System.Collections.Generic;

namespace GenericMvcUtilities.ViewModels.SinglePageApp
{
	public class SinglePageGraph
	{
		public List<Page> Pages { get; set; }

		public List<Page> Modals { get; set; }

		public Page MenuListViewModel { get; set; }
	}

	public static class SinglePage
	{
		public static Microsoft.AspNetCore.Mvc.ViewResult SideMenuView(this Microsoft.AspNetCore.Mvc.Controller controller, SinglePageGraph sideMenu)
		{
			return controller.View("~/Views/Shared/SideMenuContainer.cshtml", sideMenu);
		}
	}
}