using GenericMvc.Files.Models;
using GenericMvc.Files.Repositories;
using GenericMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericMvc.Files.Controllers
{
	//todo make more readable
	public abstract class PairedApiController<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> : IFileApiController<TViewModel, TKey>
		where TKey : IEquatable<TKey>
		where TViewModel : class, IRelatedFile<TKey>, new()
		where TEntity : class, IModelWithFilename<TKey>
		where TFileRepo : IFileRepository
		where TEntityRepo : IRepository<TEntity>
	{
		private readonly PairedRepository<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> _repository;

		public PairedApiController(PairedRepository<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> repository, ILogger<TViewModel> logger) : base(repository, logger)
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
				string message = $"Delete:{typeOfT.Name} by Id Failed";

				Logger.LogError(0, ex, message);

				throw new Exception(message, ex);
			}
		}
	}
}
