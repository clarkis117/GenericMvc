using GenericMvc.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenFu;
using Xunit;
using GenericMvc.Test.Lib.Fixtures;
using GenericMvc.Test.Lib.Models;
using GenericMvc.Test.Lib.Contexts;
using Microsoft.EntityFrameworkCore;
using GenericMvc.Models;

namespace GenericMvc.Test.Lib.Repository
{
	public abstract class IRepositoryTests<TEntity, TKey, TRepository, TContext, TFixture>
		where TRepository : IRepository<TEntity>
		where TEntity : IModel<TKey>, new()
		where TContext : DbContext
		where TFixture : DataBaseFixture<TContext>, new()
		where TKey : IEquatable<TKey>
	{
		private static readonly int[] _ranges = new int[] { 1, 2, 5, 10, 25, 50 };

		private TFixture _fixture;

		//protected List<TEntity> DataToCleanUp;

		protected TRepository Repo;

		public virtual TRepository NewRepo(TContext context)
		{
			return (TRepository)Activator.CreateInstance(typeof(TRepository), context);
		}

		public IRepositoryTests()
		{
			_fixture = new TFixture() ?? throw new NullReferenceException($"{nameof(TFixture)}:{typeof(TFixture)}");

			Repo = NewRepo(_fixture.DbContext);
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

			//SantitizeData(data);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			foreach (var item in data)
			{
				var exists = await Repo.Any(x => item.Id.Equals(x.Id));

				Assert.True(exists);
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
				var datum = await Repo.Get(x => x.Id.Equals(item.Id));

				Assert.NotNull(datum);

				//Assert.Same(item, datum);
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
		public abstract Task GetMany();

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

				//todo this should be seperate
				Assert.Same(mutated, updated);

				updatedList.Add(updated);
			}

			Assert.True(data.Count() == updatedList.Count());

			//_DataToCleanUp.AddRange(updatedList);
		}

		[Fact]
		public virtual Task UpdateRange()
		{
			foreach (var range in _ranges)
			{
				var rangeX = A.ListOf<TEntity>(range);

				Assert.NotNull(rangeX);

				Assert.NotEmpty(rangeX);

				//var createdRangeX = await Repo.CreateRange(rangeX);

				//Assert.NotNull(createdRangeX);

				//Assert.NotEmpty(createdRangeX);

				//Assert.Same(rangeX, createdRangeX);

				//var deleteResult = await Repo.DeleteRange(createdRangeX);

				//Assert.True(deleteResult);

				//_DataToCleanUp.AddRange(createdRangeX);
			}

			return Task.FromResult<object>(null);
		}





		//[Fact]
		public Task<bool> DeleteChild(object child)
		{
			return Task.FromResult(true);
			//throw new NotImplementedException();
		}

	}
}
