using GenericMvc.Models;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Api
{
	public class GraphApi<T, TKey> : Api<T, TKey>, IGraphApi<T, TKey>
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly new IGraphRepository<T> Repository;

		public GraphApi(IGraphRepository<T> repository, ILogger<T> logger) : base(repository, logger)
		{
			Repository = repository;
		}

		//todo more design work
		//todo: finish
		//todo: add unit test
		[HttpDelete, Route("[controller]/[action]/")]
		public async Task<IActionResult> DeleteChild([FromBody]Newtonsoft.Json.Linq.JObject child)
		{
			try
			{
				if (child != null)
				{
					if (ModelState.IsValid)
					{
						object dbObj = null;

						foreach (var type in Repository.EntityTypes)
						{
							if (type.FullName == child["$type"].ToString())
							{
								dbObj = child.ToObject(type);
								break;
							}
						}

						if (await Repository.DeleteChild(dbObj))
						{
							return NoContent();
						}
						else
						{
							throw new Exception("Object was not removed from DB");
						}

					}
				}

				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				string message = $"Deleting Child:{child["$type"].ToString()} of Type:{typeOfT.Name} Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}
	}
}