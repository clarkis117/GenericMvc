using System.Collections.Generic;

namespace GenericMvcUtilities.ViewModels.SinglePageApp
{
	public class SideMenuPage
	{
		public List<Page> Pages { get; set; }

		public List<Page> Modals { get; set; }

		public Page MenuListViewModel { get; set; }
	}

	public static class SideMenu
	{


		public static Microsoft.AspNet.Mvc.ViewResult SideMenuView(this Microsoft.AspNet.Mvc.Controller controller, SideMenuPage sideMenu)
		{
			return controller.View("~/Views/Shared/SideMenuContainer.cshtml", sideMenu);
		}
	}
}