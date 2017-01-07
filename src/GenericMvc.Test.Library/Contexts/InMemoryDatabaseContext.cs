using GenericMvc.Repositories;
using GenericMvc.Test.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.Lib.Contexts
{
	/// <summary>
	/// In memory Database context for testing EF Repos
	/// </summary>
	public class InMemoryDbContext : DbContext
	{
		public static DbContextOptions CreateNewContextOptions()
		{
			// Create a fresh service provider, and therefore a fresh
			// InMemory database instance.
			var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();

			// Create a new options instance telling the context to use an
			// InMemory database and the new service provider.
			var builder = new DbContextOptionsBuilder<InMemoryDbContext>();
			builder.UseInMemoryDatabase()
				   .UseInternalServiceProvider(serviceProvider);

			return builder.Options;
		}

		public InMemoryDbContext(DbContextOptions options) : base(options)
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;");
			}
		}
	}

	public class BlogDbContext : InMemoryDbContext
	{
		public BlogDbContext(DbContextOptions options) : base(options)
		{

		}

		public DbSet<Blog> Blogs { get; set; }

		public DbSet<BlogPost> BlogPosts { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Comment> Comments { get; set; }
	}
}

