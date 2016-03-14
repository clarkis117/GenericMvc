using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class TableViewModel : GenericViewModel
	{
		public string Title { get; set; }

		public string Action { get; set; }

		public string Description { get; set; }

		public bool CreateButton { get; set; }

	}
}
