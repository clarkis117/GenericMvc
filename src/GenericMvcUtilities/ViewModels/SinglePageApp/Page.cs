using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.SinglePageApp
{
	public class Page
	{
		public Page(string name)
		{
			Name = name;
		}

		public Page(string name, object data)
		{
			Name = name;
			Data = data;
		}

		public Page(string displayName, string name)
		{
			DisplayName = displayName;
			Name = name;
		}

		public Page(string displayName, string name, object data)
		{
			DisplayName = displayName;
			Name = name;
			Data = data;
		}

		//--side list
		//<li class="withripple" title="stuff" data-target="#about">About</li>
		//<li class="withripple" data-target="Id">Name</li>

		//-- Page section
		//<div class="well page" id="about">About</div>
		//<div class="well page" id="Id">@View goes here</div>

		/// <summary>
		/// value for HTML Id
		/// </summary>
		public virtual string Id { get; }

		/// <summary>
		/// File Name
		/// </summary>
		public virtual string Name { get; }

		public virtual object Data { get; }

		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		public string DisplayName { get; }

		public virtual string ViewPath
		{
			get { return ($"~/Views/{ContainingFolder}/{ViewName}.cshtml"); }
		}

		public virtual string ContainingFolder { get; }

		public virtual string ViewName { get; }

		public string GetDisplayName()
		{
			if (this.DisplayName != null)
			{
				return this.DisplayName;
			}
			else
			{
				return this.Name;
			}
		}
	}

	//page with conventions
	public class BasicPage : Page
	{
		public BasicPage(string displayName) : base(displayName: displayName, name: null)
		{

		}

		public BasicPage(string name, string folder) : base(name)
		{
			Folder = folder;
		}

		public BasicPage(string name, object data) : base(name, data)
		{

		}

		public BasicPage(string name, object data, string folder) : base(name, data)
		{
			Folder = folder;
		}

		public BasicPage(string displayName, string name, object data) : base(displayName, name, data)
		{

		}

		public BasicPage(string displayName, string name, string folder) : base(displayName, name)
		{
			Folder = folder;
		}

		public BasicPage(string displayName, string name, object data, string folder) : base(displayName, name, data)
		{
			Folder = folder;
		}

		public override string Id
		{
			get
			{
				return Name;
			}
		}

		public override string ContainingFolder
		{
			get
			{
				return "ModelViews";
			}
		}

		public override string ViewName
		{
			get
			{
				return this.Name;
			}
		}

		public string Folder { get; }

		public override string ViewPath
		{
			get
			{
				if (Folder != null)
				{
					return $"~/Views/{ContainingFolder}/{Folder}/{ViewName}.cshtml";
				}

				return $"~/Views/{ContainingFolder}/{ViewName}/{ViewName}.cshtml";
			}
		}
	}

	/// <summary>
	/// This is used to layout the collapsing forms 
	/// </summary>
	public class BasicEditor : BasicPage
	{
		public BasicEditor(string displayName, BasicPage parentForm, IEnumerable<BasicPage> childForms) : base(displayName: displayName)
		{
			ParentForm = parentForm;
			ChildForms = childForms;
		}

		public BasicPage ParentForm { get; }

		public IEnumerable<BasicPage> ChildForms { get; }

		public override string Id
		{
			get
			{
				return ParentForm.Name;
			}
		}

		public override string Name
		{
			get
			{
				return ParentForm.Name;
			}
		}

		public override object Data
		{
			get
			{
				return this;
			}
		}

		public override string ContainingFolder
		{
			get
			{
				return "Shared";
			}
		}

		public override string ViewName
		{
			get
			{
				return "BasicEditor";
			}
		}

		public override string ViewPath
		{
			get
			{
				return $"~/Views/{ContainingFolder}/{ViewName}.cshtml";
			}
		}


	}


}
