using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Test.Lib.Models
{
	public class Comment
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public DateTime DatePosted { get; set; }

		public BlogPost Post { get; set; }

		public User User { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public IList<Comment> Replies { get; set; }
	}
}
