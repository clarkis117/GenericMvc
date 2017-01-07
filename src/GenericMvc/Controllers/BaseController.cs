using GenericMvc.Models;
using GenericMvc.Repositories;
using GenericMvc.ViewModels.Basic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Controllers
{
	public enum Status : byte
	{
		ItemHasBeenCreated,
		ItemHasBeenEdited,
		ItemHasBeenRetrived, //normal use don't need this
		ItemHasBeenDeleted,

		//error messages
		ItemNotFound,

		ItemCouldNotBeCreated,
		ItemCouldNotBeEdited,
		ItemCouldNotBeDeleted, //todo add message for
		ItemIsNotValidAndChangesHaveNotBeenSaved,
		ErrorProcessingRequest,

		RequestOrQueryIsInvalid,
		ErrorExecutingQuery,
		InvalidQuery
	}

	public abstract class BaseController<TKey, T> : Controller
		where T : class, IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		protected readonly IEntityRepository<T> Repository;

		protected readonly ILogger<T> Logger;

		protected static readonly Type typeOfT = typeof(T);

		protected static Microsoft.EntityFrameworkCore.Metadata.IEntityType DataModel;

		public BaseController(IEntityRepository<T> repository, ILogger<T> logger)
		{
			try
			{
				if (repository == null)
					throw new ArgumentNullException(nameof(repository));

				if (logger == null)
					throw new ArgumentNullException(nameof(logger));

				Repository = repository;

				Logger = logger;

				//if data model equals null get it from the context
				if (DataModel == null)
				{
					var model = Repository.DataContext.Model.FindEntityType(typeOfT.FullName);

					if (model == null)
						throw new ArgumentException($"Generic parameter {typeOfT.FullName} is not a member of the DB Context used by {typeof(BaseEntityRepository<T>)}");

					DataModel = model;
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
			return message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString();
		}

		[NonAction]
		protected static string FormatExceptionMessage(Controller controller, string message)
		{
			return controller.GetType().Name + ": " + message + ": " + typeOfT.Name;
		}

		[NonAction]
		public static MessageViewModel GetMessageFromEnum(Status? message)
		{
			if (message != null)
			{
				MessageViewModel messageViewModel = null;

				switch (message.Value)
				{
					case Status.ItemHasBeenCreated:
						messageViewModel = new MessageViewModel(MessageType.Success, "Item has been successfully created");
						break;

					case Status.ItemHasBeenEdited:
						messageViewModel = new MessageViewModel(MessageType.Success, "Item has been successfully edited");
						break;

					case Status.ItemHasBeenRetrived:
						messageViewModel = new MessageViewModel(MessageType.Success, "Item has been successfully retrieved");
						break;

					case Status.ItemHasBeenDeleted:
						messageViewModel = new MessageViewModel(MessageType.Success, "Item has been successfully deleted");
						break;

					case Status.ItemNotFound:
						messageViewModel = new MessageViewModel(MessageType.Warning, "System was unable to find the Item");
						break;

					case Status.ItemCouldNotBeCreated:
						messageViewModel = new MessageViewModel(MessageType.Danger, "System was not able to save the new Item");
						break;

					case Status.ItemCouldNotBeEdited:
						messageViewModel = new MessageViewModel(MessageType.Danger, "System was not able to save the changes to the Item");
						break;

					case Status.ItemIsNotValidAndChangesHaveNotBeenSaved:
						messageViewModel = new MessageViewModel(MessageType.Warning, "The current Item is not valid and any changes have not been saved");
						break;

					case Status.ErrorProcessingRequest:
						messageViewModel = new MessageViewModel(MessageType.Danger, "System encountered an error processing the request");
						break;

					case Status.ErrorExecutingQuery:
						messageViewModel = new MessageViewModel(MessageType.Danger, "System encountered an error executing the query");
						break;

					case Status.InvalidQuery:
						messageViewModel = new MessageViewModel(MessageType.Warning, "The specified query is not valid");
						break;

					case Status.RequestOrQueryIsInvalid:
						messageViewModel = new MessageViewModel(MessageType.Warning, "The Specified Request or Query is not valid");
						break;

					case Status.ItemCouldNotBeDeleted:
						messageViewModel = new MessageViewModel(MessageType.Warning, "The Specified Item could not be deleted");
						break;

					default:
						messageViewModel = new MessageViewModel();
						break;
				}

				return messageViewModel;
			}
			else
			{
				return new MessageViewModel();
			}
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
		public virtual async Task<IActionResult> Index(Status? message)
		{
			try
			{
				var messageViewModel = GetMessageFromEnum(message);

				var results = await Repository.GetAll();

				var indexViewModel = new IndexViewModel(this)
				{
					Data = results,
					DisplayingCount = results.LongCount(),
					TotalCount = await Repository.Count(),
					Message = messageViewModel ?? new MessageViewModel(),
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
							DisplayingCount = results.LongCount(),
							TotalCount = await Repository.Count(),
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

				return RedirectToAction(nameof(Index), new { message = Status.InvalidQuery });
			}
			catch (Exception e)
			{
				var message = "Index Search Failed";

				Logger.LogError(FormatLogMessage(message, this.Request), e);

				return RedirectToAction(nameof(Index), new { message = Status.ErrorExecutingQuery });
			}
		}
	}
}