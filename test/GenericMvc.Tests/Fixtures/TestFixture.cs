using GenericMvc.Test.Lib.Contexts;
using GenericMvc.Test.Lib.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericMvc.Tests.Fixtures
{
    public class TestFixture : DataBaseFixture<InMemoryDbContext>
    {
        public TestFixture() : base()
        {
        }
    }
}
