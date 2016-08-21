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

		RequestOrQueryIsInvalid,
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

		protected static readonly Type typeOfT = typeof(T);

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
			return (controller.GetType().Name + ": " + message + ": " + typeOfT.Name);
		}

		[NonAction]
		public static MessageViewModel GetMessageFromEnum(Message? message)
		{
			if (message != null)
			{
				MessageViewModel messageViewModel = null;
				 
				switch (message.Value)
				{
					case Message.ItemHasBeenCreated:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Success,
							Text = "Item has been successfully created"
						};
						break;
					case Message.ItemHasBeenEdited:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Success,
							Text = "Item has been successfully edited"
						};
						break;
					case Message.ItemHasBeenRetrived:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Success,
							Text = "Item has been successfully retrieved"
						};
						break;
					case Message.ItemHasBeenDeleted:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Success,
							Text = "Item has been successfully deleted"
						};
						break;
					case Message.ItemNotFound:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Warning,
							Text = "System was unable to find the Item"
						};
						break;
					case Message.ItemCouldNotBeCreated:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "System was not able to save the new Item"
						};
						break;
					case Message.ItemCouldNotBeEdited:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "System was not able to save the changes to the Item"
						};
						break;
					case Message.ItemCouldNotBeRetrived:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "System could not retrieve the item"
						};
						break;
					case Message.ItemIsNotValidAndChangesHaveNotBeenSaved:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Warning,
							Text = "The current Item is not valid and any changes have not been saved"
						};
						break;
					case Message.ErrorProcessingRequest:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "System encountered an error processing the request"
						};
						break;
					case Message.ErrorExecutingQuery:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Danger,
							Text = "System encountered an error executing the query"
						};
						break;
					case Message.InvalidQuery:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Warning,
							Text = "The specified query is not valid"
						};
						break;
					case Message.RequestOrQueryIsInvalid:
						messageViewModel = new MessageViewModel()
						{
							MessageType = MessageType.Warning,
							Text = "The Specified Request or Query is not valid"
						};
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
		public virtual async Task<IActionResult> Index(Message? message)
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
