using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Attributes
{
	public class ModelInfo : Attribute
	{
		public string DisplayName { get; set; }

		public string PluralizedName { get; set; }

		public string Description { get; set; }

		public ModelInfo()
		{

		}
	}
}
