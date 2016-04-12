using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.SinglePageApp
{
	public class Page
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public virtual string ViewPath
		{
			get { return ($"~/Views/.cshtml"); }
		}

		public virtual string View { get; set; }

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
