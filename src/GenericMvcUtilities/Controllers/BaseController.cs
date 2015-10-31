using GenericMvcUtilities.Repositories;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMvcUtilities.Controllers
{
	[Authorize]
	//[Route("[controller]/[action]/")]
	public class BaseController<T> : Controller, IBaseController<T> where T : class
	{
		protected readonly BaseRepository<T> Repository;

		/// <summary>
		/// Gets or sets the controller view model.
		/// </summary>
		/// <value>
		/// The controller view model.
		/// </value>
		public ViewModels.ControllerViewData ControllerViewModel { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseController{T}" /> class.
		/// </summary>
		/// <param name="Repo">The repo.</param>
		public BaseController(BaseRepository<T> Repo)
		{
			try
			{
				if (Repo != null)
				{
					//Set repo to repo field
					this.Repository = Repo;

					//Get Controler Name
					var controllerName = this.GetControllerName(this.GetType());

					//Create Controller View Model here
					this.ControllerViewModel = new ViewModels.ControllerViewData(controllerName);
				}
				else
				{
					throw new ArgumentNullException("Repository argument is null");
				}
			}
			catch (Exception ex)
			{
				string Message = this.FormatExceptionMessage("Creation of Controller Failed");

				throw new Exception(Message, ex);
			}
		}

		/// <summary>
		/// Gets the name of the controller.
		/// </summary>
		/// <param name="controllerType">Type of the controller.</param>
		/// <returns></returns>
		private string GetControllerName(Type controllerType)
		{
			string controllerName = controllerType.Name;

			if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
			{
				controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
			}

			return controllerName;
		}

		protected string FormatLogMessage(string message, Microsoft.AspNet.Http.HttpRequest request)
		{
			return (message + ": \nHTTP Request: \n" + "Header: " + request.Headers.ToString() + "\nBody: " + request.Body.ToString());
		}

		protected string FormatExceptionMessage(string message)
		{
			return (this.GetType().ToString() + ": " + message + ": " + typeof(T).ToString());
		}

		// GET: /<controller>/
		[HttpGet]
		public virtual async Task<IActionResult> Index()
		{
			string instructions = "All "+this.ControllerViewModel.ControllerName+"s in the Database";

			ICollection<ViewModels.ActionViewData> actionViewModels = new List<ViewModels.ActionViewData>();

			try
			{

				var actionViewModel = new ViewModels.ActionViewData(
					this.ControllerViewModel,
					this.ActionContext.RouteData.Values["action"].ToString(),
					instructions,
					await Repository.GetAll());

				actionViewModels.Add(actionViewModel);

				//return view
				return View(this.ControllerViewModel.SharedViewPath, actionViewModels);
			}
			catch (Exception ex)
			{
				string Message = "Get All / Index Failed";

				//logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id:int}")]
		public virtual async Task<IActionResult> Details(int? id)
		{
			string instructions = "All "+this.ControllerViewModel.ControllerName+"s in the Database";
			
			try
			{
				if (id != null)
				{
					var item = await Repository.GetCompleteItem(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						return View(item);
					}
					else
					{
						return HttpNotFound();
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Detailed View Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id:int}")]
		public virtual async Task<IActionResult> Edit(int? id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						return View(item);
					}
					else
					{
						Type type = typeof(T);
						T result = (T)Activator.CreateInstance(type);

						return View(result);
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Edit View / Get By Id Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Edit(T item)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await Repository.Update(item);

					return RedirectToAction("Index");
				}

				return View(item);
			}
			catch (Exception ex)
			{
				string Message = "Posting Edit Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet]
		public virtual IActionResult Create()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				string Message = "Get Create View Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpPost()]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Create(T item)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await Repository.Insert(item);

					return RedirectToAction("Index");
				}

				return View(item);
			}
			catch (Exception ex)
			{
				string Message = "Create Post Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet]
		public virtual async Task<IEnumerable<T>> GetAll()
		{
			try
			{
				return await Repository.GetAll();
			}
			catch (Exception ex)
			{
				string Message = "Get All Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpGet("{id:int}")]
		public virtual async Task<T> Get(int? id)
		{
			try
			{
				var item = await Repository.Get(Repository.MatchByIdExpression(id));

				if (item != null)
				{
					return item;
				}
				else
				{
					Type type = typeof(T);
					dynamic result = (T)Activator.CreateInstance(type);
					result.Id = id;

					return result;
				}
			}
			catch (Exception ex)
			{
				string Message = "Get by Id Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}

		[Route("[controller]/[action]/")]
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int? id)
		{
			try
			{
				if (id != null)
				{
					var item = await Repository.Get(Repository.MatchByIdExpression(id));

					if (item != null)
					{
						var result = await Repository.Delete(item);

						if (result != false)
						{
							return RedirectToAction("Index");
						}
						else
						{
							return HttpBadRequest();
						}
					}
					else
					{
						return HttpNotFound();
					}
				}
				else
				{
					return HttpBadRequest();
				}
			}
			catch (Exception ex)
			{
				string Message = "Delete by Id Failed";

				//this.Logger.LogError(this.FormatLogMessage(Message, this.Request), ex);

				throw new Exception(this.FormatExceptionMessage(Message), ex);
			}
		}
	}
}