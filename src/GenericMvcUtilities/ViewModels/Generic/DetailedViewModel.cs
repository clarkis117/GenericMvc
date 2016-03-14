using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.ViewModels.Generic
{
	public class DetailedViewModel : GenericViewModel
	{

		/// <summary>
		/// Gets or sets a value indicating whether this instance is edit view.
		/// If true, no details or delete button
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is edit view; otherwise, <c>false</c>.
		/// </value>
		public bool IsEditView { get; set; }
	}
}
