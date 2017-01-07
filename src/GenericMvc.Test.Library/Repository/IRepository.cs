using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Test.Lib
{
	public interface IRepository
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

	public interface EntityFrameworkRepository
	{
		void GetDataContext();

		void ContextSet();

		void GetEntityTypes();

		Task<bool> DeleteChild(object child);

	}
}
