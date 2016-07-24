using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.SinglePageApp
{
	public class Page
	{
		//--side list
		//<li class="withripple" title="stuff" data-target="#about">About</li>
		//<li class="withripple" data-target="Id">Name</li>

		//-- Page section
		//<div class="well page" id="about">About</div>
		//<div class="well page" id="Id">@View goes here</div>

		/// <summary>
		/// value for HTML Id
		/// </summary>
		public virtual string Id { get; set; }

		/// <summary>
		/// File Name
		/// </summary>
		public virtual string Name { get; set; }

		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		public virtual string DisplayName { get; set; }

		public virtual string ViewPath
		{
			get { return ($"~/Views/{ContainingFolder}/{ViewName}.cshtml"); }
		}

		public virtual string ContainingFolder { get; set; }

		public virtual string ViewName { get; set; }

		public object Data { get; set; }

		public Page()
		{

		}

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
		public BasicPage()
		{
			//this.Folder = ViewName;
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

		public string Folder { get; set; }

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
	public class BasicEditor : Page
	{
		public BasicEditor()
		{
			//Much wow, I so cheat
			this.Data = this;
		}

		public BasicPage ParentForm { get; set; }

		public List<BasicPage> ChildForms { get; set; }

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

			set
			{
				ParentForm.Name = value;
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
