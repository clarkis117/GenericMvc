using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Tests
{
	public interface IRepositoryTests
	{
		Task Any();

		Task GetAll();

		Task Get();

		Task GetWithData();

		Task GetMany();

		Task GetManyWithData();

		Task Create();

		Task CreateRange();

		Task Update();

		Task UpdateRange();

		Task Delete();

		Task DeleteRange();
	}
}
