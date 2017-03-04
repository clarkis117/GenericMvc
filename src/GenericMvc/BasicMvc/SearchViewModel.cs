using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.ViewModels.Basic
{
	public class SearchViewModel
	{
		[Required]
		public string PropertyName { get; set; }

		[Required]
		public string Value { get; set; }

		public IEnumerable<SelectListItem> SelectPropertyList { get; set; }
	}
}
