using GenericMvcUtilities.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.ViewModels.Basic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace GenericMvcUtilities.Controllers
{
	public enum Message
	{
		ItemHasBeenCreated,
		ItemHasBeenEdited,
		ItemHasBeenRetrived, //normal use don't need this
		ItemHasBeenDeleted,
		//error messages
		ItemNotFound,
		ItemCouldNotBeCreated,
		ItemCouldNotBeEdited,
		ItemCouldNotBeRetrived, //same as not found
		ItemIsNotValidAndChangesHaveNotBeenSaved,
		ErrorProcessingRequest,

		ErrorExecutingQuery,
		InvalidQuery
	}

	public abstract class BaseController<TKey, T> : Controller
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected IEntityRepository<T> Repository;

		protected readonly ILogger<T> Logger;

		protected static Microsoft.EntityFrameworkCore.Metadata.IEntityType DataModel;



		public BaseController(IEntityRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository != null)
				{
					Repository = repository;

					if (DataModel == null)
					{
						var model = Repository.DataContext.Model.FindEntityType(typeof(T).FullName);

						if (model == null)
							throw new ArgumentException($"Generic parameter {typeof(T).FullName} is not a member of the DB Context used by {typeof(BaseEntityRepository<T>)}");

						DataModel = model;
					}
				}
				else
				{
					throw new ArgumentNullException(nameof(repository));
				}

				if (logger != null)
				{
					Logger = logger;
				}
				else
				{
					throw new ArgumentNullException(nameof(logger));
				}
			}
			catch (Exception e)
			{
				string message = FormatExceptionMessage(this, $"Creation of {this.GetType().Name} Failed");

				this.Logger.LogCritical(message, e);

				throw new Exception(message, e);
			}
		}

		[NonAction]
		protected static string FormatLogMessage(string message, Microsoft.AspNetCore.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		[NonAction]
		protected static string FormatExceptionMessage(Controller controller, string message)
		{
			return (controller.GetType().ToString() + ": " + message + ": " + typeof(T).ToString());
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetSearchSelectList()
		{
			foreach (var property in DataModel.GetProperties())
			{
				var option = new SelectListItem()
				{
					Text = property.Name,
					Value = property.Name
				};

				yield return option;
			}

			var defaultItem = new SelectListItem()
			{
				Text = "Select Property",
				Value = "",
				Selected = true
			};

			yield return defaultItem;
		}

		//todo also add in action for unique name validation
		[Route("[controller]/[action]/"), HttpGet]
		public virtual async Task<IActionResult> Index(Message? message)
		{
			/*
			var error = Error == SearchErrorMessages.InvalidQuery ?
				new MessageViewModel()
				{
					MessageType = MessageType.Danger,
					Text = "Invalid Search Query, use another query and try again"
				}
				: Error == SearchErrorMessages.ErrorExecutingQuery ?
				new MessageViewModel()
				{
					MessageType = MessageType.Danger,
					Text = "An Error occurred while executing the query, please try a different query"
				}
				: null;

			*/
			try
			{
				var results = await Repository.GetAll();

				var indexViewModel = new IndexViewModel(this)
				{
					Data = results,
					Count = results.LongCount(),
					//Message = error ?? new MessageViewModel(),
					SearchViewModel = new SearchViewModel()
					{
						SelectPropertyList = GetSearchSelectList()
					}
				};

				//return view
				return this.ViewFromModel(indexViewModel);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				Logger.LogError(FormatLogMessage(Message, this.Request), ex);

				throw new Exception(FormatExceptionMessage(this, Message), ex);
			}
		}

		[HttpGet("[controller]/Index/[action]/")]
		public virtual async Task<IActionResult> Search([FromQuery] string PropertyName, [FromQuery] string Value)
		{
			try
			{
				if (PropertyName != null && Value != null && ModelState.IsValid)
				{
					var property = DataModel.FindProperty(PropertyName);
					//validate property
					if (property != null)
					{
						var typeConverter = TypeDescriptor.GetConverter(property.ClrType);

						object convertedValue = typeConverter.ConvertFromString(Value);

						var results = await Repository.GetMany(Repository.SearchExpression(property.Name, convertedValue));

						//create index view model and return view
						var indexViewModel = new IndexViewModel(this)
						{
							Action = "Index",
							NestedView = "Index",
							Data = results,
							Count = results.LongCount(),
							Description = "Search Results for Query",
							Message = new MessageViewModel(),
							SearchViewModel = new SearchViewModel()
							{
								SelectPropertyList = GetSearchSelectList()
							}
						};

						return this.ViewFromModel(indexViewModel);
					}
				}

				return RedirectToAction(nameof(Index), new { message = Message.InvalidQuery });
			}
			catch (Exception e)
			{
				var message = "Index Search Failed";

				Logger.LogError(FormatLogMessage(message, this.Request), e);

				return RedirectToAction(nameof(Index), new { message = Message.ErrorExecutingQuery });
			}
		}
	}
}
