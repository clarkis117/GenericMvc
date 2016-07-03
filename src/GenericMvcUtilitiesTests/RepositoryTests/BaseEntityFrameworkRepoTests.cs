using GenericMvcUtilities.Repositories;
using GenericMvcUtilities.Tests;
using GenericMvcUtilitiesTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenFu;
using Xunit;

namespace GenericMvcUtilitiesTests.RepositoryTests
{
	public abstract class BaseEntityFrameworkRepoTests<TEntity> : IRepositoryTests where TEntity : class, new()
	{
		private static readonly int[] _ranges = new int[] { 1, 2, 5, 10, 25, 50, 100 };

		private IDbTestFixture<TEntity> Fixture;

		private List<TEntity> _DataToCleanUp;

		protected BaseEntityFrameworkRepository<TEntity> Repo;

		public BaseEntityFrameworkRepoTests(IDbTestFixture<TEntity> fixture)
		{
			if (fixture != null)
			{
				Fixture = fixture;

				Repo = fixture.Repository;

				_DataToCleanUp = new List<TEntity>();

				//create a test Item
				var entityOne = A.New<TEntity>();

				//(entityOne as Object).MemberwiseClone()
			}
			else
			{
				throw new ArgumentNullException(nameof(fixture));
			}
		}

		protected abstract TEntity Mutator(TEntity entity);

		protected abstract System.Linq.Expressions.Expression<Func<TEntity, bool>> GetQuery();

		protected abstract System.Linq.Expressions.Expression<Func<TEntity, bool>> GetManyQuery();

		#region Utility Methods
		private Task<IEnumerable<TEntity>> CreateDataInDataBase(int n)
		{
			var data = A.ListOf<TEntity>(n);

			return Repo.CreateRange(data);
		}

		#endregion

		public virtual Task Any()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public async virtual Task Create()
		{
			var data = A.ListOf<TEntity>(10);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			foreach (var item in data)
			{
				Assert.NotNull(item);

				var createdItem = await Repo.Create(item);

				Assert.NotNull(createdItem);

				Assert.Same(item, createdItem);

				_DataToCleanUp.Add(item);
			}


		}

		[Fact]
		public virtual async Task CreateRange()
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

				_DataToCleanUp.AddRange(createdRangeX);
			}
		}

		public virtual async Task Delete()
		{
			var data = A.ListOf<TEntity>(10);

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

		public virtual async Task DeleteRange()
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

		public virtual async Task Get()
		{
			var data = await CreateDataInDataBase(10);

			Assert.NotNull(data);

			Assert.NotEmpty(data);

			Repo.Get(x => x.Equals(x));

		}

		public Task GetAll()
		{
			throw new NotImplementedException();
		}

		public Task GetMany()
		{
			throw new NotImplementedException();
		}

		public abstract Task GetManyWithData();

		public abstract Task GetWithData();

		public Task Update()
		{
			throw new NotImplementedException();
		}

		public Task UpdateRange()
		{
			throw new NotImplementedException();
		}
	}
}
