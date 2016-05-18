using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Reflection;

namespace GenericMvcUtilities.Controllers
{
	public abstract class SinglePageController<T, TKey> : BaseApiController<T, TKey>, ISinglePageController<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{

		public SinglePageController(BaseRepository<T> repository, ILogger<T> logger) : base(repository, logger)
		{

		}

		/// <summary>
		/// Construct the Single Page View Hierarchy here then return it
		/// </summary>
		/// <returns>the Single Page View container with associate view data</returns>
		public abstract Task<IActionResult> Index();
	}
}
