﻿using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Reflection;
using GenericMvcUtilities.Controllers;

namespace GenericMvcUtilities.Controllers
{
	public abstract class SinglePageController<T, TKey> : BaseGraphController<T, TKey>, ISinglePageController<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		//todo add static cache field
	   //private static 

		public SinglePageController(BaseEntityFrameworkRepositroy<T> repository, ILogger<T> logger) : base(repository, logger)
		{

		}

		/// <summary>
		/// Construct the Single Page View Hierarchy here then return it
		/// </summary>
		/// <returns>the Single Page View container with associate view data</returns>
		public abstract Task<IActionResult> Index();
	}
}
