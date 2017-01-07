using GenericMvc.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	/// <summary>
	/// currently does not allow modifications to folder structure
	/// </summary>
	public class FileRepository : IRepository<DataFile>
	{
		private readonly string RootFolder;

		private readonly System.IO.DirectoryInfo _directoryInfo;

		private System.IO.SearchOption _searchOption = System.IO.SearchOption.TopDirectoryOnly;

		private bool _includeNestedDriectories;

		/// <summary>
		///
		/// </summary>
		public bool InludedNestedDirectories
		{
			get { return _includeNestedDriectories; }
			set
			{
				if (value)
				{
					_searchOption = System.IO.SearchOption.AllDirectories;
				}
				else
				{
					_searchOption = System.IO.SearchOption.TopDirectoryOnly;
				}

				_includeNestedDriectories = value;
			}
		}

		public enum FileLoading { JustFileInfo, WithMime, WithMimeAndData };

		public EncodingType FileEncodingType { get; set; } = EncodingType.Base64;

		public FileLoading DefaultLoadingSettings { get; set; } = FileLoading.WithMime;

		public Type TypeOfEntity
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ParameterExpression EntityExpression
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// probably should have some file size limit here
		/// </summary>
		/// <param name="rootFolder"></param>
		public FileRepository(string rootFolder, bool includeNestedDirectories = false)
		{
			if (rootFolder != null)
			{
				//check if folder exists here
				if (System.IO.Directory.Exists(rootFolder))
				{
					this.RootFolder = rootFolder;

					this._directoryInfo = new System.IO.DirectoryInfo(rootFolder);
				}
				else
				{
					throw new ArgumentException(rootFolder);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(rootFolder));
			}

			this.InludedNestedDirectories = includeNestedDirectories;

			//this.Enumerator = this.EnumerateFiles().GetEnumerator();
			//this.DirectorySeparator = System.IO.Path.DirectorySeparatorChar;
		}

		private Task<DataFile> FileInitSwitch(DataFile entity)
		{
			switch (DefaultLoadingSettings)
			{
				case FileLoading.JustFileInfo:
					return Task.FromResult(entity.Initialize(path:""));

				case FileLoading.WithMime:
					return entity.Initialize(false, FileEncodingType);

				case FileLoading.WithMimeAndData:
					return entity.Initialize(true, FileEncodingType);

				default:
					return Task.FromResult(entity.Initialize(path:""));
			}
		}

		private IObservable<DataFile> GetFilesObservable(Func<DataFile, bool> matchFunc, bool completeOnFirst = false)
		{
			return Observable.Create<DataFile>(async obs =>
			{
				using (var enumerator = EnumerateFiles().GetEnumerator())
				{
					while (await enumerator.MoveNext())
					{
						//do work
						var file = new DataFile(enumerator.Current);

						if (matchFunc(await FileInitSwitch(file)))
						{
							obs.OnNext(file);

							if (completeOnFirst)
							{
								obs.OnCompleted();
								return;
							}
						}
					}
				}

				//if(!completeOnFirst)
				obs.OnCompleted();
			});
		}

		private IObservable<DataFile> GetAllFilesObservable()
		{
			return Observable.Create<DataFile>(async obs =>
			{
				using (var enumerator = EnumerateFiles().GetEnumerator())
				{
					while (await enumerator.MoveNext())
					{
						obs.OnNext(await FileInitSwitch(new DataFile(enumerator.Current)));
					}

					obs.OnCompleted();
				}
			});
		}

		/// <summary>
		/// Check and reduce
		/// then compile and return the delegate produced form the lambda
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		private Func<DataFile, bool> checkAndCompilePredicate(Expression<Func<DataFile, bool>> predicate)
		{
			if (predicate != null)
			{
				if (predicate.CanReduce)
				{
					Expression<Func<DataFile, bool>> reducedPredicate = predicate;

					while (reducedPredicate.CanReduce)
					{
						reducedPredicate = (Expression<Func<DataFile, bool>>)reducedPredicate.ReduceAndCheck();
					}

					return reducedPredicate.Compile();
				}

				return predicate.Compile();
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		/// <summary>
		/// File should not exist on disk
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		private Exception checkFileForCreation(DataFile file)
		{
			if (file.Data != null)
			{
				//var dirInfo = new System.IO.DirectoryInfo(file.ContainingFolder);

				if (_directoryInfo.GetFiles(file.Name).Count() == 0)
				{
					return null;
				}
				else
				{
					return new System.IO.IOException("File Already Exists");
				}
			}
			else
			{
				return new ArgumentNullException(nameof(file), "File Data is Null");
			}
		}

		/// <summary>
		/// Creates File on Disk
		///
		/// File does not have to be initialized
		/// File should not exist on disk
		/// </summary>
		/// <returns></returns>
		private async Task createFile(DataFile entity)
		{
			if (entity._fileInfo == null)
			{
				entity.Initialize(path: this._directoryInfo.FullName);
			}

			//check data, if both path and data valid make file
			//if (System.IO.Directory.Exists(ContainingFolder) && !this._fileInfo.Exists)

			byte[] data;

			//check encoding it must be set and if data needs trans-coded
			if (entity.EncodingType == EncodingType.NonNewtonsoftBase64)
			{
				data = Convert.FromBase64String(System.Text.Encoding.ASCII.GetString(entity.Data));
			}
			else
			{
				data = entity.Data;
			}

			using (var fileStream = entity._fileInfo.Create())
			{
				if (fileStream.CanWrite)
				{
					await fileStream.WriteAsync(data, 0, data.Length);

					await fileStream.FlushAsync();
				}
				else
				{
					throw new System.IO.IOException("Cannot write to file: " + entity.Name);
				}
			}
		}

		private Exception checkFileForUpdate(DataFile file)
		{
			if (file.Data != null)
			{
				//Make sure directory exists
				//var dirInfo = new System.IO.DirectoryInfo(file.ContainingFolder);

				if (_directoryInfo.GetFiles(file.Name).Count() == 1)
				{
					return null;
				}
				else
				{
					return new System.IO.IOException("File does not Exist");
				}
			}
			else
			{
				return new ArgumentNullException(nameof(file), "File Data is Null");
			}
		}

		private async Task updateFile(DataFile entity)
		{
			if (entity._fileInfo == null)
			{
				entity.Initialize(path:"");
			}

			//if (System.IO.Directory.Exists(ContainingFolder) && this._fileInfo.Exists)

			byte[] data;

			//check encoding it must be set and if data needs trans-coded
			if (entity.EncodingType == EncodingType.NonNewtonsoftBase64)
			{
				data = Convert.FromBase64String(System.Text.Encoding.ASCII.GetString(entity.Data));
			}
			else
			{
				data = entity.Data;
			}

			using (var fileStream = entity._fileInfo.OpenWrite())
			{
				if (fileStream.CanWrite)
				{
					await fileStream.WriteAsync(data, 0, data.Length);

					await fileStream.FlushAsync();
				}
				else
				{
					throw new System.IO.IOException("Cannot write to file: " + entity.Name);
				}
			}
		}

		private IAsyncEnumerable<System.IO.FileInfo> EnumerateFiles()
		{
			return this._directoryInfo.EnumerateFiles("*", _searchOption).ToAsyncEnumerable();
		}

		public async Task<bool> Any(Expression<Func<DataFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch).Any();
		}

		public Task<IEnumerable<DataFile>> GetAll()
		{
			return Task.FromResult(GetAllFilesObservable().ToEnumerable());
		}

		//todo use while loops
		/// <summary>
		/// Takes a lambda expression as argument and compiles it
		/// then enumerates files in the root folder
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns>First Match Found</returns>
		public async Task<DataFile> Get(Expression<Func<DataFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch, true).FirstAsync();

			//return new File(await this.EnumerateFiles().First(async x => IsMatch( (await(new File(x).Initialize())) ) == true));
		}

		public Task<DataFile> Get(Expression<Func<DataFile, bool>> predicate, bool WithNestedData = false)
		{
			if (WithNestedData)
			{
				return GetCompleteItem(predicate);
			}
			else
			{
				return Get(predicate);
			}
		}

		public async Task<IList<DataFile>> GetMany(Expression<Func<DataFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch).ToList();
		}

		public async Task<IList<DataFile>> GetMany(Expression<Func<DataFile, bool>> predicate, bool WithNestedData = false)
		{
			var IsMatch = checkAndCompilePredicate(predicate);

			if (WithNestedData)
			{
				var observable = GetFilesObservable(IsMatch);

				var taskForeach = observable.ForEachAsync(async x => await x.Initialize(true, FileEncodingType));

				/*
				foreach (var item in files)
				{
					await item.Initialize(true, FileEncodingType);
				}
				*/

				await taskForeach;

				return await observable.ToList();
			}
			else
			{
				return await GetMany(predicate);
			}
		}

		private async Task<DataFile> GetCompleteItem(Expression<Func<DataFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			var file = await GetFilesObservable(IsMatch, true).FirstAsync();

			return await file.Initialize(true, this.FileEncodingType);
		}

		/// <summary>
		/// File Should have already been Initialized and should have data
		/// Containing folder should match folder repository is attached to or subdirectories
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>

		public async Task<DataFile> Create(DataFile entity)
		{
			if (entity != null)
			{
				try
				{
					var checkFile = checkFileForCreation(entity);

					if (checkFile != null)
						throw checkFile;
					//throw checkFile;

					await createFile(entity);

					return entity;
				}
				catch (Exception e)
				{
					throw e;
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		/// <summary>
		/// Files Should have already been Initialized and should have data
		/// ContainingFolders should match folder repository is attached to or subdirectories
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		/// <exception cref="AggregateException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task<IEnumerable<DataFile>> CreateRange(IEnumerable<DataFile> entities)
		{
			if (entities != null)
			{
				var exceptionLazyList = new Lazy<List<Exception>>();

				foreach (var entity in entities)
				{
					try
					{
						var result = await Create(entity);

						/*
						if (result == null)
						{
							entities.Remove(entity);
						}
						*/
					}
					catch (Exception e)
					{
						if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
						{
							exceptionLazyList.Value.Add(e);
						}

						//entities.Remove(entity);
					}
				}

				if (exceptionLazyList.Value.Count > 0)
				{
					throw new AggregateException(exceptionLazyList.Value);
				}

				return entities;
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
			}
		}

		public async Task<DataFile> Update(DataFile entity)
		{
			if (entity != null)
			{
				try
				{
					var checkFile = checkFileForUpdate(entity);

					if (checkFile != null)
						throw checkFile;
					//throw checkFile;

					await updateFile(entity);

					return entity;
				}
				catch (Exception e)
				{
					throw e;
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		public async Task<IEnumerable<DataFile>> UpdateRange(IEnumerable<DataFile> entities)
		{
			if (entities != null)
			{
				var exceptionLazyList = new Lazy<List<Exception>>();

				foreach (var entity in entities)
				{
					try
					{
						var result = await Update(entity);

						/*
						if (result == null)
						{
							entities.Remove(entity);
						}
						*/
					}
					catch (Exception e)
					{
						if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
						{
							exceptionLazyList.Value.Add(e);
						}

						//entities.Remove(entity);
					}
				}

				if (exceptionLazyList.Value.Count > 0)
				{
					throw new AggregateException(exceptionLazyList.Value);
				}

				return entities;
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
			}
		}

		public Task<bool> Delete(DataFile entity)
		{
			if (entity != null)
			{
				try
				{
					if (entity._fileInfo == null)
					{
						entity.Initialize(path:"");
					}

					entity._fileInfo.Delete();

					return Task.FromResult(true);
				}
				catch (Exception)
				{
					return Task.FromResult(false);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		public async Task<bool> DeleteRange(IEnumerable<DataFile> entities)
		{
			var entityCount = entities.Count();

			if (entities != null)
			{
				var exceptionLazyList = new Lazy<List<Exception>>();

				foreach (var entity in entities)
				{
					try
					{
						var result = await Delete(entity);

						/*
						if (result == false)
						{
							entities.Remove(entity);
						}
						*/
					}
					catch (Exception e)
					{
						if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
						{
							exceptionLazyList.Value.Add(e);
						}

						/*
						entities.Remove(entity);
						*/
					}
				}

				if (exceptionLazyList.Value.Count > 0)
				{
					throw new AggregateException(exceptionLazyList.Value);
				}

				if (entities.Count() == entityCount)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entities));
			}
		}

		public IEnumerator<DataFile> GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}

		public Task<long> Count()
		{
			return Task.FromResult(_directoryInfo.GetFiles().LongCount());
		}
	}
}