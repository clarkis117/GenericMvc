using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Models
{
	public class Comment
	{
		public int Id { get; set; }

		public DateTime DatePosted { get; set; }

		public BlogPost Post { get; set; }

		public User User { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public IList<Comment> Replies { get; set; }
	}
}
