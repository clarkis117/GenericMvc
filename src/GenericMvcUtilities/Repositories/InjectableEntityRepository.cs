using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
{
	public class InjectableEntityRepo<T, TContext> : BaseEntityRepository<T>
		where T : class
		where TContext : DbContext
	{
		public InjectableEntityRepo(TContext context) : base(context)
		{
		}
	}
}
