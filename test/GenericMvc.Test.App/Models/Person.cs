using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.App.Models
{
	public class Person : IModel<int>
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}
}
