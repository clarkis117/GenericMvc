using System.Collections.Generic;

namespace GenericMvc.ViewModels.SinglePageApp
{
	public class SinglePageGraph
	{
		public SinglePageGraph(string title,
			Page menuListViewModel,
			Page pageListView,
			IEnumerable<Page> pages,
			IEnumerable<Page> modals)
		{
			Title = title;
			MenuListViewModel = menuListViewModel;
			PageListView = pageListView;
			Pages = pages;
			Modals = modals;
		}

		public string Title { get; }

		public IEnumerable<Page> Pages { get; }

		public IEnumerable<Page> Modals { get; }

		public Page MenuListViewModel { get; }

		public Page PageListView { get; }
	}

	public static class SinglePage
	{
		public static Microsoft.AspNetCore.Mvc.ViewResult SideMenuView(this Microsoft.AspNetCore.Mvc.Controller controller, SinglePageGraph sideMenu)
		{
			return controller.View("~/Views/Shared/SideMenuContainer.cshtml", sideMenu);
		}
	}
}