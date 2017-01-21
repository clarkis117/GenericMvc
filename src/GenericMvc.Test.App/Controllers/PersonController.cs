using GenericMvc.Controllers;
using GenericMvc.Test.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvc.Repositories;
using Microsoft.Extensions.Logging;

namespace GenericMvc.Test.App.Controllers
{
	public class PersonController : BasicController<int, Person>
	{
		public PersonController(IEntityRepository<Person> repository, ILogger<Person> logger) : base(repository, logger)
		{
		}
	}
}
