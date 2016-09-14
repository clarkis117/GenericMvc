using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
	//todo this could use cleaned up
	public abstract class IFileApiController<T, TKey> : BaseApiController<T, TKey>
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
		public override async Task<bool> IsValid(T Model, ModelStateDictionary ModelState, bool Updating = false)
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

		public override async Task<bool> IsValid(T[] Models, ModelStateDictionary ModelState, bool updating = false)
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

	//todo make more readable
	public abstract class PairedApiController<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> : IFileApiController<TViewModel, TKey>
		where TKey : IEquatable<TKey>
		where TViewModel : class, IRelatedFile<TKey>, new()
		where TEntity : class, IModelWithFilename<TKey>
		where TFileRepo : IFileRepository
		where TEntityRepo : IRepository<TEntity>
	{
		private readonly PairedRepository<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> _repository;

		public PairedApiController(PairedRepository<TKey,TViewModel,TEntity,TEntityRepo,TFileRepo> repository, ILogger<TViewModel> logger) : base(repository, logger)
		{
			_repository = repository;
		}

		//todo add check to see if its update mode and the file hasnt changed
		//if the file has changed then check
		//if the file hasn't change dont' check
		[AcceptVerbs("Get", "Post"), Route("[controller]/[action]/")]
		public override async Task<IActionResult> VerifyName(string Name, TKey Id)
		{
			bool isValid = false;

			var Filename = Utilities.IsValidFileName(Name);

			//check for valid name
			if (Filename.IsValid)
			{
				isValid = true;
			}
			else
			{
				return Json(data: Filename.ErrorMessage);
			}

			//if we find both conditions then that means we're updating and the file hasn't changed
			if (await _repository._entityRepo.Any(x => x.Id.Equals(Id) && x.Filename == Name))
			{
				return Json(data: true);
			}
			else
			{
				//see if any name matches
				var NameMatch = await Utilities.IsFileNameUnique<TViewModel>(Name, _repository);

				if (NameMatch.IsValid)
				{
					isValid = true;
				}
				else
				{
					return Json(data: NameMatch.ErrorMessage);
				}

				return Json(data: isValid);
			}
		}

		//fixed: fix design oversight to have whole object deleted
		[Route("api/[controller]/[action]/"), HttpDelete]
		public async Task<IActionResult> PairedDelete([FromQuery] TKey id, [FromQuery] TKey parentId)
		{
			try
			{
				if (id != null && parentId != null && ModelState.IsValid)
				{
					//Get Item, this causes EF to begin tracking it
					var item = await Repository.Get(x => x.Id.Equals(id));

					item.ParentObjectId = parentId;

					if (item != null)
					{
						//This causes EF to Remove the Item from the Database
						var result = await Repository.Delete(item);

						if (result != false)
						{
							//If success return 201 response
							return new NoContentResult();
						}
						else
						{
							throw new Exception("Deleting Item Failed");
						}
					}
					else
					{
						//Send 404 if object is not in Database
						return NotFound(id);
					}
				}
				else
				{
					//Send 400 Response
					return BadRequest("Object Identifier is Null");
				}
			}
			catch (Exception ex)
			{
				string message = "Deleting Item Failed";

				this.Logger.LogError(FormatLogMessage(message, this.Request));

				throw new Exception(FormatExceptionMessage(this, message), ex);
			}
		}
	}
}
