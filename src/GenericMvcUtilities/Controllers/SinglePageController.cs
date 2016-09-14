using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using GenericMvcUtilities.Controllers;
using GenericMvcUtilities.ViewModels.SinglePageApp;

namespace GenericMvcUtilities.Controllers
{
	public abstract class SinglePageController<T, TKey> : BaseGraphController<T, TKey>, ISinglePageController<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		public SinglePageController(IGraphRepository<T> repository, ILogger<T> logger) : base(repository, logger)
		{

		}

		public abstract SinglePageGraph ViewGraph { get; }

		/// <summary>
		/// Construct the Single Page View Hierarchy here then return it
		/// </summary>
		/// <returns>the Single Page View container with associate view data</returns>
		[Route("[controller]/[action]/")]
		public IActionResult Index()
		{
			return this.SideMenuView(ViewGraph);
		}
	}
}
