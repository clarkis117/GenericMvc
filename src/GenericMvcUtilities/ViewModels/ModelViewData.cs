using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels
{
	public class ModelViewData
	{
		public ModelViewData()
		{

		}

		public string Title { get; set; }

		public string Action { get; set; }

		public string Controller { get; set; }

		public string HttpMethod { get; set; } 

		public char QueryDelinator { get; set; } = '?';

		public string NestedViewPath { get; set; }


	}
}
