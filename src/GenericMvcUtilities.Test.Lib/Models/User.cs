using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenericMvcUtilities.Test.Lib.Models
{
	public class User
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public string FirstName { get; set; }

		public char MiddleInitial { get; set; }

		public string LastName { get; set; }

		public int Age { get; set; }

		public Blog Blog { get; set; }

		public IList<Comment> Comments { get; set; }
	}
}
