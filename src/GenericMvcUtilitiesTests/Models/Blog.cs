using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Models
{
	public class Blog
	{
		public Blog()
		{

		}

		public int Id { get; set; }

		public string Name { get; set; }

		public User Owner { get; set; }

		public DateTime DateCreated { get; set; }

		public IList<BlogPost> Posts { get; set; }
	}

	public class BlogPost
	{
		public int Id { get; set; }

		public Blog Blog { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public DateTime DatePublished { get; set; }

		public IList<Comment> Comments { get; set; }
	}
}
