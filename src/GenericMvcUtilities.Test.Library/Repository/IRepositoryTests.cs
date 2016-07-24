using GenericMvcUtilities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenFu;
using Xunit;
using GenericMvcUtilities.Test.Lib.Fixtures;
using GenericMvcUtilities.Test.Lib.Models;
using GenericMvcUtilities.Test.Lib.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GenericMvcUtilities.Test.Lib.Repository
{
	public abstract class IRepositoryTests<TEntity, TRepository, TContext, TFixture> ///:Lib.EntityFrameworkRepository
		where TRepository : IRepository<TEntity>
		where TEntity : class, new()
		where TContext : DbContext
		where TFixture : DataBaseFixture<TContext>, new()
	{
		private static readonly int[] _ranges = new int[] { 1, 2, 5, 10, 25, 50 };

		private TFixture Fixture;

		//protected List<TEntity> DataToCleanUp;

		protected TRepository Repo;

		public virtual TRepository NewRepo(TContext context)
		{
			return (TRepository)Activator.CreateInstance(typeof(TRepository), context);
		}

		public IRepositoryTests()
		{
			var fixture = new TFixture();

			if (fixture != null)
			{
				Fixture = fixture;

				Repo = NewRepo(fixture.DbContext);

				//_DataToCleanUp = new List<TEntity>();

				//create a test Item
				var entityOne = A.New<TEntity>();

				//(entityOne as Object).MemberwiseClone()
			}
			else
			{
				throw new ArgumentNullException(nameof(fixture));
			}
		}

		/// <summary>
		/// Creates the object graph with sanitized Id/Key Data
		/// </summary>
		/// <returns></returns>
		protected abstract TEntity CreateObjectGraph(int n);

		/// <summary>
		/// Creates the list of object with sanitized Id/Key Data
		/// </summary>
		/// <param name="n">The number of objects to create.</param>
		/// <returns></returns>
		protected abstract IEnumerable<TEntity> CreateListofGraphs(int numberOfObjects, int numberOfSubObjects); 

		protected abstract TEntity Mutator(TEntity entity);

		protected abstract System.Linq.Expressions.Expression<Func<TEntity, bool>> GetQuery();

		protected abstract System.Linq.Expressions.Expression<Func<TEntity, bool>> GetManyQuery();

		#region Utility Methods
		private Task<IEnumerable<TEntity>> CreateDataInDataBase(int n)
		{
			var list = CreateListofGraphs(n, n);

			return Repo.CreateRange(list);
		}

		protected abstract IEnumerable<TEntity> SantitizeData(IEnumerable<TEntity> collection);

		#endregion

		[Fact]
		public async virtual Task Any()
		{
			var data = await CreateDataInDataBase(10);

			SantitizeData(data);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			foreach (var item in data)
			{
				var createdDatum = await Repo.Any(x => x.Equals(item));

				Assert.NotNull(createdDatum);

				Assert.Same(item, createdDatum);
			}
		}

		[Fact]
		public async virtual Task Create()
		{
			var data = A.ListOf<TEntity>(10);

			SantitizeData(data);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			foreach (var item in data)
			{
				Assert.NotNull(item);

				var createdItem = await Repo.Create(item);

				Assert.NotNull(createdItem);

				Assert.Same(item, createdItem);

				//_DataToCleanUp.Add(item);
			}


		}

		[Fact]
		public virtual async Task CreateRange()
		{
			foreach (var range in _ranges)
			{
				var rangeX = A.ListOf<TEntity>(range);

				SantitizeData(rangeX);

				Assert.NotNull(rangeX);

				Assert.NotEmpty(rangeX);

				var createdRangeX = await Repo.CreateRange(rangeX);

				Assert.NotNull(createdRangeX);

				Assert.NotEmpty(createdRangeX);

				Assert.Same(rangeX, createdRangeX);

				//_DataToCleanUp.AddRange(createdRangeX);
			}
		}

		[Fact]
		public virtual async Task Delete()
		{
			var data = A.ListOf<TEntity>(10);

			SantitizeData(data);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			var createdData = new List<TEntity>();

			foreach (var item in data)
			{
				createdData.Add(await Repo.Create(item));
			}

			Assert.NotNull(createdData);

			Assert.NotEmpty(createdData);

			foreach (var item in createdData)
			{
				var result = await Repo.Delete(item);

				Assert.True(result);
			}
		}

		[Fact]
		public virtual async Task DeleteRange()
		{
			foreach (var range in _ranges)
			{
				var rangeX = A.ListOf<TEntity>(range);

				SantitizeData(rangeX);

				Assert.NotNull(rangeX);

				Assert.NotEmpty(rangeX);

				var createdRangeX = await Repo.CreateRange(rangeX);

				Assert.NotNull(createdRangeX);

				Assert.NotEmpty(createdRangeX);

				Assert.Same(rangeX, createdRangeX);

				var deleteResult = await Repo.DeleteRange(createdRangeX);

				Assert.True(deleteResult);

				//_DataToCleanUp.AddRange(createdRangeX);
			}
		}

		[Fact]
		public virtual async Task Get()
		{
			var data = await CreateDataInDataBase(10);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			foreach (var item in data)
			{
				var createdDatum = await Repo.Get(x => x.Equals(item));

				Assert.NotNull(createdDatum);

				Assert.Same(item, createdDatum);
			}

			//_DataToCleanUp.AddRange(data);
		}

		[Fact]
		public abstract Task GetWithData();

		[Fact]
		public virtual async Task GetAll()
		{
			var data = await CreateDataInDataBase(25);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			var retrivedData = await Repo.GetAll();

			Assert.NotNull(retrivedData);

			Assert.NotEmpty(retrivedData);

			Assert.True(data.Count() == retrivedData.Count());

			//_DataToCleanUp.AddRange(data);
		}

		[Fact]
		public virtual Task GetMany()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public abstract Task GetManyWithData();

		[Fact]
		public virtual async Task Update()
		{
			var data = await CreateDataInDataBase(10);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			var updatedList = new List<TEntity>();

			foreach (var item in data)
			{
				var mutated = Mutator(item);

				var updated = await Repo.Update(mutated);

				Assert.NotNull(updated);

				Assert.NotSame(mutated, updated);

				updatedList.Add(updated);
			}

			Assert.True(data.Count() == updatedList.Count());

			//_DataToCleanUp.AddRange(updatedList);
		}

		[Fact]
		public virtual async Task UpdateRange()
		{
			foreach (var range in _ranges)
			{
				var rangeX = A.ListOf<TEntity>(range);

				Assert.NotNull(rangeX);

				Assert.NotEmpty(rangeX);

				var createdRangeX = await Repo.CreateRange(rangeX);

				Assert.NotNull(createdRangeX);

				Assert.NotEmpty(createdRangeX);

				Assert.Same(rangeX, createdRangeX);

				var deleteResult = await Repo.DeleteRange(createdRangeX);

				Assert.True(deleteResult);

				//_DataToCleanUp.AddRange(createdRangeX);
			}
		}

		/* to do place in decendant class
		[Fact]
		public void GetDataContext()
		{
			var context = Repo.DataContext;

			Assert.NotNull(context);
		}

		[Fact]
		public void ContextSet()
		{
			var contextSet = Repo.ContextSet;

			Assert.NotNull(contextSet);
		}

		[Fact]
		public void GetEntityTypes()
		{
			var types = Repo.DataContext.Model.GetEntityTypes();

			Assert.NotNull(types);

			Assert.NotEmpty(types);
		}
		*/

		[Fact]
		public Task<bool> DeleteChild(object child)
		{
			throw new NotImplementedException();
		}

	}
}
