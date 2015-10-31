namespace GenericMvcUtilities.ViewModels
{
	/// <summary>
	/// This is the generic view model for all view controllers
	/// </summary>
	public class ControllerViewData
	{
		public ControllerViewData(string controllerName)
		{
			this.ControllerName = controllerName;

			Conventionalize();
		}

		public ControllerViewData(string controllerName, string sharedView)
		{
			this.ControllerName = controllerName;

			Conventionalize();

			if (sharedView != null)
			{
				this.SharedViewPath = sharedView;
			}
		}

		/// <summary>
		/// Conventionalizes ModelViewPath and CreateEdit form.
		/// Override to change conventions.
		/// </summary>
		protected virtual void Conventionalize()
		{
			this.ModelViewPath = "~/Views/ModelViews/" + this.ControllerName + "/";
			this.CreateEditForm = "CreateEdit" + this.ControllerName;
		}

		public string ControllerName { get; set; }

		/// <summary>
		/// Gets or sets the path to the model's views.
		/// </summary>
		/// <value>
		/// The model view path.
		/// </value>
		public string ModelViewPath { get; set; } 

		/// <summary>
		/// Gets or sets the create edit form file name.
		/// </summary>
		/// <value>
		/// The create edit form.
		/// </value>
		public string CreateEditForm { get; set; }

		/// <summary>
		/// Gets or sets the shared view container path.
		/// </summary>
		/// <value>
		/// The shared view path.
		/// </value>
		public string SharedViewPath { get; set; } = "~/Views/Shared/_PageWellSection.cshtml";
	}

	/// <summary>
	/// The Generic View model for each mvc action.
	/// This should generally consume the controller view model
	/// </summary>
	public class ActionViewData
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionViewData" /> class.
		/// </summary>
		/// <param name="controllerViewData">The controller view data.</param>
		/// <param name="action">The action.</param>
		/// <param name="instructions">The instructions for the action view.</param>
		/// <param name="data">The data.</param>
		public ActionViewData(ControllerViewData controllerViewData, string action, string instructions, object data)
		{
			//set loose params
			this.Action = action;
			this.Data = data;
			this.Instructions = instructions;

			//set structured params
			this.Controller = controllerViewData.ControllerName;

			this.NestedView = controllerViewData.ModelViewPath + action + ".cshtml";

			this.Title = controllerViewData.ControllerName + " - " + action;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionViewData" /> class.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="action">The action.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="nestedViewPath">The nested view path.</param>
		/// <param name="data">The data.</param>
		public ActionViewData(string title, string action, string controller, string nestedViewPath, object data)
		{
			this.Title = title;
			this.Action = action;
			this.Controller = controller;
			this.NestedView = nestedViewPath;
			this.Data = data;
		}

		/// <summary>
		/// Gets or sets the title.
		/// Convention is: <see cref="ActionViewData.Controller"/> - <see cref="Action"/>
		/// </summary>
		/// <value>
		/// The title.
		/// </value>
		public string Title { get; set; }

		public string Action { get; set; }

		public string Instructions { get; set; }

		public string Controller { get; set; }

		public string FormHttpMethod { get; set; } = "Post";

		public char QueryDelinator { get; set; } = '?';

		/// <summary>
		/// Gets or sets the nested view with it's puesdo fully qualified Name.
		/// </summary>
		/// <value>
		/// The nested view.
		/// </value>
		public string NestedView { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// The data being the actual stuff from the database to display.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
		public object Data { get; set; }
	}
}