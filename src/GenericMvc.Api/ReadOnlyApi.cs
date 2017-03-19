using GenericMvc.Models;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericMvc.Api
{
	public class ReadOnlyApi<T, TKey> : Controller, IReadOnlyApi<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly IRepository<T> Repository;

		protected readonly ILogger<T> Logger;

		protected static readonly Type typeOfT = typeof(T);

		public ReadOnlyApi(IRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				Repository = repository ?? throw new ArgumentNullException(nameof(repository));

				Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			}
			catch (Exception ex)
			{
				string message = $"Creation of API Controller:{typeOfT.Name} Failed";

				Logger.LogCritical(message, ex);

				throw new Exception(message, ex);
			}
		}

		[Route("api/[controller]/[action]/"), HttpGet]
		public virtual Task<IEnumerable<T>> GetAll()
		{
			try
			{
				return Repository.GetAll();
			}
			catch (Exception ex)
			{
				string message = $"Get:{typeOfT.Name} by Id Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}

		[Route("api/[controller]/[action]/"), HttpGet]
		public virtual async Task<IActionResult> Get(TKey id)
		{
			try
			{
				if (id == null || !ModelState.IsValid)
					return BadRequest(ModelState);

				if (!await Repository.Any(x => x.Id.Equals(id)))
					return NotFound(id);

				var item = await Repository.Get(x => x.Id.Equals(id), WithNestedData: true);

				if (item == null)
					throw new Exception("Entity Framework Error");

				return Json(item);
			}
			catch (Exception ex)
			{
				string message = $"Get:{typeOfT.Name} by Id Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}

		[Route("api/[controller]/[action]/"), HttpGet]
		public virtual async Task<IActionResult> GetMany(string propertyName, string value)
		{
			if (propertyName == null || value == null || !ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				//todo work this out somehow

				var item = await Repository.GetMany(Repository.IsMatchedExpression<T>(propertyName, value), WithNestedData: false);

				if (item != null)
				{
					return Json(item);
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				string message = $"Get:{typeOfT.Name} by Property Query Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}
	}
}