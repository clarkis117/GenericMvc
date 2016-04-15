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

		public string Id { get; set; }

		public string Name { get; set; }

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

	}

	public class Editor
	{

	}

	public class GraphEditor
	{

	}
}
