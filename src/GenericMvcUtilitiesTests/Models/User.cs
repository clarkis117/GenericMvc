using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Models
{
	public class User
	{
		public Guid Id { get; set; }

		public string FirstName { get; set; }

		public char MiddleInitial { get; set; }

		public string LastName { get; set; }

		public int Age { get; set; }

		public Blog Blog { get; set; }

		public IList<Comment> Comments { get; set; }
	}
}
