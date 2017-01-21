using GenericMvc.Repositories;
using GenericMvc.Test.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericMvc.Test.App.Repositories
{
	public class PersonRepository : BaseEntityRepository<Person>
	{
		public PersonRepository(PersonDbContext dbContext) : base(dbContext)
		{
		}
	}
}
