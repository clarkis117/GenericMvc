using GenericMvcUtilities.Test.Lib.Contexts;
using GenericMvcUtilities.Test.Lib.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilitiesTests.Fixtures
{
    public class TestFixture : DataBaseFixture<InMemoryDbContext>
    {
    }
}
