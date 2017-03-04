using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GenericMvc.Test.App.Models
{
	public class PersonDbContext : DbContext
	{
		public PersonDbContext(DbContextOptions options) : base(options)
		{

		}

		/*
		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;");
			}
		}
		*/

		public DbSet<Person> People { get; set; }
	}
}
