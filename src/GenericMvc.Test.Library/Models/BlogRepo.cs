using GenericMvc.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.Lib.Models
{
    public class BlogRepo : EntityRepository<Blog>
    {
		public BlogRepo(DbContext context) : base(context)
		{

		}

    }
}
