using GenericMvcUtilities.Models;
using MimeDetective;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
{
	/// <summary>
	/// currently does not allow modifications to folder structure
	/// </summary>
	public class IFileRepository<TFile, TKey> : IRepository<TFile> 
		where TFile : class, IFile<TKey>, new()
		where TKey : IEquatable<TKey>
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

		/// <summary>
		/// probably should have some file size limit here
		/// </summary>
		/// <param name="rootFolder"></param>
		public IFileRepository(string rootFolder, bool includeNestedDirectories = false)
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

		/// <summary>
		/// Initializes the specified file.
		/// Can do both File Backed And Data Backed Files
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		/// <exception cref="System.IO.IOException">Invalid Path</exception>
		public static TFile Initialize(TFile file, string path = "")
		{
			FileInfo fileInfo;

			//File Backed File
			if (file.IsFileBacked())
			{
				return file;
			}
			else if (file.Path != null && file.Path != "" && file.Name != null && file.Name != "")
			{
				var filePath = System.IO.Path.Combine(file.Path, file.Name);

				fileInfo = new System.IO.FileInfo(filePath);
			}
			else if (path != null && path != "" && file.Name != null && file.Name != "") //Data Backed File
			{
				var filePath = System.IO.Path.Combine(path, file.Name);

				fileInfo = new System.IO.FileInfo(filePath);
			}
			else
			{
				throw new System.IO.IOException("Invalid Path");
			}

			//set properties here
			file.Path = fileInfo.FullName;

			file.FileInfo = fileInfo;

			return file;
		}


		/// <summary>
		/// Assumes file has already been created
		/// Does not load complete file from disk
		/// Initializes as file facade
		/// Can only do File Backed Files
		/// </summary>
		/// <returns></returns>
		public static async Task<TFile> InitializeWithMime(TFile file)
		{
			var fileInit = Initialize(file, path: "");

			if (!fileInit.FileInfo.Exists)
			{
				throw new System.IO.FileNotFoundException("File not found after getting file info", file.Name);
			}

			//set properties
			FileType type = await file.FileInfo.GetFileTypeAsync();

			//ie this mean we couldn't find the file type
			if (type == null)
			{
				file.ContentType = "";
			}
			else
			{
				file.ContentType = type.Mime;
			}

			return file;
		}

		//todo maybe throw because base64 could use more bytes than raw encoding
		/// <summary>
		/// Initializes file and reads file from disk with specified encoding
		/// </summary>
		/// <param name="loadFile"></param>
		/// <param name="encodingType"></param>
		/// <returns></returns>
		public static async Task<TFile> Initialize(TFile file, bool loadFile = false, EncodingType encodingType = EncodingType.RawBytes)
		{
			var fileInit = await InitializeWithMime(file);

			if (loadFile)
			{
				using (var stream = file.FileInfo.OpenRead())
				{
					if (stream.CanRead && stream.Length <= int.MaxValue)
					{
						byte[] rawData = new byte[Convert.ToInt32(stream.Length)];

						await stream.ReadAsync(rawData, 0, Convert.ToInt32(stream.Length));

						file.EncodingType = encodingType;

						if (encodingType == EncodingType.NonNewtonsoftBase64)
						{
							file.Data = System.Text.Encoding.ASCII.GetBytes(Convert.ToBase64String(rawData));
						}
						else
						{
							//basically: if (encodingType == Encoding.Base64 || encodingType == Encoding.RawBytes)
							file.Data = rawData;
						}
					}
					else
					{
						throw new System.IO.IOException("Stream too Large to copy to buffer");
					}
				}
			}

			return file;
		}


		private Task<TFile> FileInitSwitch(TFile entity)
		{
			switch (DefaultLoadingSettings)
			{
				case FileLoading.JustFileInfo:
					return Task.FromResult(Initialize(entity,path:""));

				case FileLoading.WithMime:
					return Initialize(entity, loadFile: false, encodingType: FileEncodingType);

				case FileLoading.WithMimeAndData:
					return Initialize(entity, loadFile: true, encodingType: FileEncodingType);

				default:
					return Task.FromResult(Initialize(entity, path:""));
			}
		}

		private IObservable<TFile> GetFilesObservable(Func<TFile, bool> matchFunc, bool completeOnFirst = false)
		{
			return Observable.Create<TFile>(async obs =>
			{
				using (var enumerator = EnumerateFiles().GetEnumerator())
				{
					while (await enumerator.MoveNext())
					{
						//do work
						var file = new TFile()
						{
							Name = enumerator.Current.Name,
							Path = enumerator.Current.DirectoryName,
							FileInfo = enumerator.Current
						};

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

		private IObservable<TFile> GetAllFilesObservable()
		{
			return Observable.Create<TFile>(async obs =>
			{
				using (var enumerator = EnumerateFiles().GetEnumerator())
				{
					while (await enumerator.MoveNext())
					{
						var file = new TFile()
						{
							Name = enumerator.Current.Name,
							Path = enumerator.Current.DirectoryName,
							FileInfo = enumerator.Current
						};

						obs.OnNext(await FileInitSwitch(file));
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
		private Func<TFile, bool> checkAndCompilePredicate(Expression<Func<TFile, bool>> predicate)
		{
			if (predicate != null)
			{
				if (predicate.CanReduce)
				{
					Expression<Func<TFile, bool>> reducedPredicate = predicate;

					while (reducedPredicate.CanReduce)
					{
						reducedPredicate = (Expression<Func<TFile, bool>>)reducedPredicate.ReduceAndCheck();
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
		private Exception checkFileForCreation(TFile file)
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
		private async Task createFile(TFile entity)
		{
			if (entity.FileInfo == null)
			{
				Initialize(entity, path: this._directoryInfo.FullName);
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

			using (var fileStream = entity.FileInfo.Create())
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

		private byte[] decodeData(TFile file)
		{
			byte[] data;

			//check encoding it must be set and if data needs trans-coded
			if (file.EncodingType == EncodingType.NonNewtonsoftBase64)
			{
				return data = Convert.FromBase64String(System.Text.Encoding.ASCII.GetString(file.Data));
			}
			else //newtonsoft handled or just raw bytes
			{
				//data = file.Data;
				return data = file.Data;
			}
		}

		private IAsyncEnumerable<System.IO.FileInfo> EnumerateFiles()
		{
			return this._directoryInfo.EnumerateFiles("*", _searchOption).ToAsyncEnumerable();
		}

		public async Task<bool> Any(Expression<Func<TFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch).Any();
		}

		public Task<IEnumerable<TFile>> GetAll()
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
		public async Task<TFile> Get(Expression<Func<TFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch, true).FirstAsync();

			//return new File(await this.EnumerateFiles().First(async x => IsMatch( (await(new File(x).Initialize())) ) == true));
		}

		public Task<TFile> Get(Expression<Func<TFile, bool>> predicate, bool WithNestedData = false)
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

		public async Task<IList<TFile>> GetMany(Expression<Func<TFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return await GetFilesObservable(IsMatch).ToList();
		}

		public async Task<IList<TFile>> GetMany(Expression<Func<TFile, bool>> predicate, bool WithNestedData = false)
		{
			var IsMatch = checkAndCompilePredicate(predicate);

			if (WithNestedData)
			{
				var observable = GetFilesObservable(IsMatch);

				var taskForeach = observable.ForEachAsync(async x => await Initialize(x, true, FileEncodingType));

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

		private async Task<TFile> GetCompleteItem(Expression<Func<TFile, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			var file = await GetFilesObservable(IsMatch, true).FirstAsync();

			return await Initialize(file, loadFile: true, encodingType: FileEncodingType);
		}

		/// <summary>
		/// File Should have already been Initialized and should have data
		/// Containing folder should match folder repository is attached to or subdirectories
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>

		public async Task<TFile> Create(TFile entity)
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
		public async Task<IEnumerable<TFile>> CreateRange(IEnumerable<TFile> entities)
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

		//This checks to make sure the file exists on disk
		private Exception checkFileForUpdate(TFile file)
		{
			if (file.Data != null)
			{
				//Make sure file exists
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

		public async Task<TFile> Update(TFile entity)
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

		//we should treat files as immutable and just delete them 
		//this way if the underlying file's bytes change we can just make a new one and rewrite the bytes to disk
		private async Task updateFile(TFile entity)
		{
			if (entity.FileInfo == null)
			{
				//path should already have been set
				//todo re evaluate this
				Initialize(entity,path:"");
			}



			//decode data if encoded
			var data  = decodeData(entity);

			if (entity.FileInfo.Length != data.Length)
			{
				//delete existing file and then write a new one
				if (!entity.FileInfo.IsReadOnly)
				{
					//delete file
					entity.FileInfo.Delete();

					//create file
					using (var fileStream = entity.FileInfo.Create())
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
				else
				{
					throw new System.IO.IOException("File is readonly");
				}
			}
			else
			{
				//else file hasn't change, do nothing
				return;
			}
		}

		public async Task<IEnumerable<TFile>> UpdateRange(IEnumerable<TFile> entities)
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

		//todo add check to see if file exists
		//if file does not exists return true
		public Task<bool> Delete(TFile entity)
		{
			if (entity != null)
			{
				try
				{
					//make it file backed
					if (entity.FileInfo == null)
					{
						if (entity.Path == null || entity.Path == "")
						{
							Initialize(entity, path: this._directoryInfo.FullName);
						}
						else
						{
							Initialize(entity, path: "");
						}
					}

					entity.FileInfo.Delete();

					return Task.FromResult(true);
				}
				catch (Exception e)
				{
					return Task.FromResult(false);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(entity));
			}
		}

		public async Task<bool> DeleteRange(IEnumerable<TFile> entities)
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

		public IEnumerator<TFile> GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}
	}
}