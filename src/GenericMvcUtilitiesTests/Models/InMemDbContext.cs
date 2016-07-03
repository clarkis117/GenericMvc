using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Models
{
	/// <summary>
	/// In memory Database context for testing EF Repos
	/// </summary>
	public class InMemDbContext : DbContext
	{
		public InMemDbContext()
		{

		}

		public InMemDbContext(DbContextOptions options) : base(options)
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;");
			}
		}

		public DbSet<Blog> Blogs { get; set; }

		public DbSet<BlogPost> BlogPosts { get; set; }

		public DbSet<User>	Users { get; set; }

		public DbSet<Comment> Comments { get; set; }
	}
}
