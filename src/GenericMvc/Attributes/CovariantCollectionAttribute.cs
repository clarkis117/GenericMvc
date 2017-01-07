using System;

namespace GenericMvc.Models
{
	public class CovariantCollectionAttribute : Attribute
	{
		public CovariantCollectionAttribute(params Type[] types)
		{
			Types = types;
		}

		public Type[] Types { get; set; }
	}
}