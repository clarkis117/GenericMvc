using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericMvc.Repositories;
using GenericMvc.Models;
using Xunit;
using GenericMvc.Tests;
using GenericMvc.Files.Models;
using GenericMvc.Files.Repositories;

namespace GenericMvcTests
{
	//todo change test2 name to path.randomfile name
	//todo use finally for test clean up
	/// <summary>
	/// All tests should be conducted as if they're coming in from over the wire, when calling file repo
	/// -this means that fileInfo should be null
	/// </summary>
	public class FileRepoTests : IDisposable //, IRepositoryTests
	{
		public const string TestFolder = "./Data";

		public const string TestName = "test.png";

		public static readonly FileRepository FileRepo = new FileRepository(TestFolder, false);

		public List<string> filesToCleanup = new List<string>();

		public FileRepoTests()
		{

		}

		private void TestInitializedFile(DataFile file)
		{
			Assert.NotNull(file);

			Assert.NotNull(file._fileInfo);

			Assert.NotNull(file._fileType);

			Assert.Null(file.Data);

			Assert.True(TestName == file.Name);
		}

		private void TestInitializedFileWithData(DataFile file)
		{
			Assert.NotNull(file);

			Assert.NotNull(file._fileInfo);

			Assert.NotNull(file._fileType);

			Assert.NotNull(file.Data);

			//Assert.True(TestName == file.Name);
		}

		private void TestMultipleFiles(IEnumerable<DataFile> files)
		{
			Assert.NotEmpty(files);

			Assert.True(files.Count() > 1);

			foreach (var item in files)
			{
				Assert.NotNull(item);

				Assert.NotNull(item._fileType);

				//this should be null
				Assert.Null(item.Data);
			}
		}

		[Fact]
		public void HasTestData()
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(TestFolder);

			var files = dir.EnumerateFiles();

			Assert.NotEmpty(files);
		}

		[Fact]
		public void IEnumerableTests()
		{
			var IEnumerator = FileRepo.GetEnumerator();

			Assert.NotNull(IEnumerator);

			var file = FileRepo.First(x => x.Name == TestName);

			Assert.NotNull(file);

			TestInitializedFile(file);
		}

		[Fact]
		public async Task GetAll()
		{
			var files = await FileRepo.GetAll();

			TestMultipleFiles(files);
		}

		[Fact]
		public async Task Get()
		{
			var file = await FileRepo.Get(x => x.Name == TestName);

			TestInitializedFile(file);
		}

		[Fact]
		public async Task Any()
		{
			var fileExists = await FileRepo.Any(x => x.Name == TestName);

			Assert.True(fileExists);

			var madeUpName = await FileRepo.Any(x => x.Name == "ThisShouldntExist.png");

			Assert.False(madeUpName);
		}

		[Fact]
		public async Task GetWithData()
		{
			var file = await FileRepo.Get(x => x.Name == TestName, true);

			TestInitializedFileWithData(file);
		}

		[Fact]
		public async Task GetMany()
		{
			var files = await FileRepo.GetMany(x => System.IO.Path.GetFileNameWithoutExtension(x._fileInfo.FullName) == "test");

			TestMultipleFiles(files);
		}

		private async Task testAndCreatedFile(DataFile file)
		{
			TestInitializedFileWithData(file);

			file.Name = "test2" + file._fileInfo.Extension;

			file.Id = System.IO.Path.Combine(file.Name);

			file._fileInfo = null;

			var result = await FileRepo.Create(file);

			Assert.NotNull(result);

			Assert.True(System.IO.File.Exists(file.Id));

			filesToCleanup.Add(file.Id);
		}

		[Fact]
		public async Task Create()
		{
			var file = await FileRepo.Get(x => x.Name == TestName,true);

			await testAndCreatedFile(file);
		}

		[Fact]
		public async Task CreateRange()
		{
			var files = await FileRepo.GetMany(x => System.IO.Path.GetFileNameWithoutExtension(x._fileInfo.FullName) == "test");

			foreach (var file in files)
			{
				await file.Initialize(true, EncodingType.Base64);

				await testAndCreatedFile(file);
			}
		}

		[Fact]
		public async Task Update()
		{
			var file = await FileRepo.Get(x => x.Name == TestName,true);

			await testAndCreatedFile(file);

			for (int i = 0; i < file.Data.Length; i++)
			{
				file.Data[i] += 1;
			}

			var result = await FileRepo.Update(file);
		}

		[Fact]
		public async Task Delete()
		{
			var file = await FileRepo.Get(x => x.Name == TestName,true);

			TestInitializedFileWithData(file);

			file.Name = "test2" + file._fileInfo.Extension;

			file.Id = System.IO.Path.Combine(file.Name);

			file._fileInfo = null;

			var result = await FileRepo.Create(file);

			Assert.NotNull(result);

			Assert.True(System.IO.File.Exists(file.Id));

			file._fileInfo = null;

			var deleteResult = await FileRepo.Delete(file);

			Assert.True(deleteResult);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects).
				}

				foreach (var item in filesToCleanup)
				{
					System.IO.File.Delete(item);
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
