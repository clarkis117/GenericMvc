using GenericMvc.Files.Models;
using GenericMvc.Models;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvc.Files.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GenericMvc.Api;

namespace GenericMvc.Files.Controllers
{
	//todo this could use cleaned up
	public abstract class IFileApiController<T, TKey> : Api<T, TKey>
		where T : class, IModelFile<TKey>
		where TKey : IEquatable<TKey>
	{
		public IFileApiController(IRepository<T> repository, ILogger<T> logger) :base(repository, logger)
		{

		}

		protected abstract string[] whiteListedMimes { get; }

		//use this to avoid name check on repos that change file name during save 
		protected bool CheckFileName { get; set; } = true;

		//add check to see if it is create or update
		protected override async Task<bool> IsValid(T Model, ModelStateDictionary ModelState, bool Updating = false)
		{
			if (await base.IsValid(Model, ModelState))
			{
				int errors = 0;

				//check if name is valid file name
				if (CheckFileName)
				{
					var Filename = Utilities.IsValidFileName(Model.Name);

					//check for valid name
					if (!Filename.IsValid)
					{
						errors++;
						ModelState.AddModelError(nameof(Model.Name), Filename.ErrorMessage);
					}

					if (!Updating)
					{
						//see if any name matches
						var NameMatch = await Utilities.IsFileNameUnique<T>(Model.Name, Repository);

						if (!NameMatch.IsValid)
						{
							errors++;
							ModelState.AddModelError(nameof(Model.Name), NameMatch.ErrorMessage);
						}
					}
				}

				//check if mime type is valid
				var MimeTypeValid = Utilities.IsWhiteListedContentType<T>(Model, whiteListedMimes);

				if (!MimeTypeValid.IsValid)
				{
					errors++;
					ModelState.AddModelError(nameof(Model.ContentType), MimeTypeValid.ErrorMessage);
				}

				if (errors > 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		protected override async Task<bool> IsValid(IEnumerable<T> Models, ModelStateDictionary ModelState, bool updating = false)
		{
			if (ModelState.IsValid)
			{
				List<bool> truths = new List<bool>();

				foreach (var item in Models)
				{
					truths.Add(await IsValid(item, ModelState));
				}

				if (truths.Any(x => x == false))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		[AcceptVerbs("Get", "Post"), Route("[controller]/[action]/")]
		public IActionResult VerifyContentType(string ContentType)
		{
			var whiteListResult = Utilities.IsWhiteListedContentType(ContentType, whiteListedMimes);

			if (whiteListResult.IsValid)
			{
				return Json(data: true);
			}
			else
			{
				return Json(data: whiteListResult.ErrorMessage);
			}
		}

		//todo add check to see if its update mode and the file hasnt changed
		//if the file has changed then check
		//if the file hasn't change dont' check
		[AcceptVerbs("Get", "Post"), Route("[controller]/[action]/")]
		public abstract Task<IActionResult> VerifyName(string Name, TKey Id);
	}
}
