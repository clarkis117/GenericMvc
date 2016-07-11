using GenericMvcUtilities.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Test.Lib.Models
{
    public class BlogRepo : BaseEntityFrameworkRepository<Blog>
    {
		public BlogRepo(DbContext context) : base(context)
		{

		}

    }
}
