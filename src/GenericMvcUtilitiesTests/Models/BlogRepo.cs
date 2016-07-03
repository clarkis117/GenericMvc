using GenericMvcUtilities.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Models
{
    public class BlogRepo : BaseEntityFrameworkRepository<Blog>
    {
		public BlogRepo(DbContext context) : base(context)
		{

		}

    }
}
