using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.Lib.Models
{
	public class Blog : IModel<int>
	{
		public Blog()
		{

		}

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string Name { get; set; }

		public Guid OwnerId { get; set; }

		public User Owner { get; set; }

		public DateTime DateCreated { get; set; }

		public IList<BlogPost> Posts { get; set; }
	}

	public class BlogPost
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public Blog Blog { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public DateTime DatePublished { get; set; }

		public IList<Comment> Comments { get; set; }
	}
}
