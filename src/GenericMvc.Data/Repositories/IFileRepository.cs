using GenericMvc.Models;
using MimeDetective;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	/// <summary>
	/// currently does not allow modifications to folder structure
	/// </summary>
	//todo clean this up
	public class IFileRepository : IRepository<IFile>
	{
		private readonly FileSystemVisitor _visitor = new FileSystemVisitor();

		private readonly System.IO.DirectoryInfo _directoryInfo;

		private System.IO.SearchOption _searchOption = System.IO.SearchOption.TopDirectoryOnly;

		private bool _includeNestedDriectories;

		private static readonly Type typeofT = typeof(IFile);

		public Type TypeOfEntity { get { return typeofT; } }

		private static readonly ParameterExpression expressionOfT = Expression.Parameter(typeofT);

		public ParameterExpression EntityExpression { get { return expressionOfT; } }

		public bool InludedNesteDirectories
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

		public enum FileLoading : byte { JustFileInfo, WithMime, WithMimeAndData };

		public EncodingType FileEncodingType { get; set; } = EncodingType.Base64;

		public FileLoading DefaultLoadingSettings { get; set; } = FileLoading.WithMime;

		/// <summary>
		/// probably should have some file size limit here
		/// </summary>
		/// <param name="rootFolder"></param>
		public IFileRepository(string rootFolder, bool includeNestedDirectories = false)
		{
			if (rootFolder == null)
				throw new ArgumentNullException(nameof(rootFolder));

			//check if folder exists here
			if (System.IO.Directory.Exists(rootFolder))
			{
				this._directoryInfo = new System.IO.DirectoryInfo(rootFolder);
			}
			else
			{
				throw new ArgumentException(rootFolder);
			}

			this.InludedNesteDirectories = includeNestedDirectories;
		}

		#region FileInit

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte[] decodeData(IFile file)
		{
			//check encoding it must be set and if data needs trans-coded
			if (file.EncodingType != EncodingType.NonNewtonsoftBase64)
			{
				return file.Data;
			}
			else
			{
				//else it needs trans-coded
				return Convert.FromBase64String(System.Text.Encoding.ASCII.GetString(file.Data));
			}
		}

		/// <summary>
		/// Initializes the specified file.
		/// Can do both File Backed And Data Backed Files
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		/// <exception cref="System.IO.IOException">Invalid Path</exception>
		protected static IFile Initialize(IFile file, string path = "")
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
		protected static async Task<IFile> InitializeWithMime(IFile file)
		{
			var fileInit = Initialize(file, path: "");

			if (!fileInit.FileInfo.Exists)
			{
				throw new System.IO.FileNotFoundException("File not found after getting file info", file.Name);
			}

			//set properties
			FileType type = await file.FileInfo.GetFileTypeAsync();

			//ie this mean we couldn't find the file type
			if (type.Mime == null)
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
		protected static async Task<IFile> Initialize(IFile file, bool loadFile = false, EncodingType encodingType = EncodingType.RawBytes)
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

		private Task<IFile> FileInitSwitch(IFile entity)
		{
			switch (DefaultLoadingSettings)
			{
				case FileLoading.JustFileInfo:
					return Task.FromResult(Initialize(entity, path: ""));

				case FileLoading.WithMime:
					return Initialize(entity, loadFile: false, encodingType: FileEncodingType);

				case FileLoading.WithMimeAndData:
					return Initialize(entity, loadFile: true, encodingType: FileEncodingType);

				default:
					return Task.FromResult(Initialize(entity, path: ""));
			}
		}

		#endregion FileInit

		#region FileObservables

		private IObservable<IFile> GetFilesObservable(Func<IFile, bool> matchFunc, bool completeOnFirst = false)
		{
			return Observable.Create<IFile>(async obs =>
			{
				using (var enumerator = _directoryInfo.EnumerateFiles("*", _searchOption).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var file = new FileStruct(enumerator.Current);

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

				obs.OnCompleted();
			});
		}

		private IObservable<IFile> GetAllFilesObservable()
		{
			return Observable.Create<IFile>(async obs =>
			{
				using (var enumerator = _directoryInfo.EnumerateFiles("*", _searchOption).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var file = new FileStruct(enumerator.Current);

						obs.OnNext(await FileInitSwitch(file));
					}

					obs.OnCompleted();
				}
			});
		}

		#endregion FileObservables

		/// <summary>
		/// Check and reduce
		/// then compile and return the delegate produced form the lambda
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Expression<Func<IFile, bool>> checkAndReducePredicate(Expression<Func<IFile, bool>> predicate)
		{
			while (predicate.CanReduce)
			{
				predicate = (Expression<Func<IFile, bool>>)predicate.ReduceAndCheck();
			}

			return predicate;
		}

		public Task<IEnumerable<IFile>> GetAll()
		{
			return Task.FromResult(GetAllFilesObservable().ToEnumerable());
		}

		protected IFile GetFileByName(string Name)
		{
			var resultFiles = _directoryInfo.GetFiles(Name, _searchOption);

			if (resultFiles.Length >= 1)
			{
				return new FileStruct(resultFiles[0]);
			}
			else
			{
				return null;
			}
		}

		public async Task<bool> Any(Expression<Func<IFile, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			var checkedPredicate = checkAndReducePredicate(predicate);

			_visitor.Reset();

			_visitor.Visit(checkedPredicate);

			//check to see if we can query by file name
			if (_visitor.CanQueryByName())
			{
				var file = GetFileByName((string)_visitor.MemberValue);

				if (file != null)
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
				return await GetFilesObservable(checkedPredicate.Compile(), true).Any();
			}
		}

		/// <summary>
		/// Takes a lambda expression as argument and compiles it
		/// then enumerates files in the root folder
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns>First Match Found</returns>
		public async Task<IFile> Get(Expression<Func<IFile, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			var checkedPredicate = checkAndReducePredicate(predicate);

			_visitor.Reset();

			_visitor.Visit(checkedPredicate);

			//check to see if we can query by file name
			if (_visitor.CanQueryByName())
			{
				var file = GetFileByName((string)_visitor.MemberValue);

				return await FileInitSwitch(file);
			}
			else
			{
				return await GetFilesObservable(checkedPredicate.Compile(), true).FirstOrDefaultAsync();
			}
		}

		public async Task<IFile> Get(Expression<Func<IFile, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
			{
				var checkedPredicate = checkAndReducePredicate(predicate);

				_visitor.Reset();

				_visitor.Visit(checkedPredicate);

				IFile file;

				//check to see if we can query by file name
				if (_visitor.CanQueryByName())
				{
					file = GetFileByName((string)_visitor.MemberValue);
				}
				else
				{
					file = await GetFilesObservable(checkedPredicate.Compile(), true).FirstAsync();
				}

				return await Initialize(file, loadFile: true, encodingType: FileEncodingType);
			}
			else
			{
				return await Get(predicate);
			}
		}

		public async Task<IList<IFile>> GetMany(Expression<Func<IFile, bool>> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			return await GetFilesObservable(checkAndReducePredicate(predicate).Compile()).ToList();
		}

		public async Task<IList<IFile>> GetMany(Expression<Func<IFile, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			if (WithNestedData)
			{
				var observable = GetFilesObservable(checkAndReducePredicate(predicate).Compile());

				var taskForeach = observable.ForEachAsync(async x => await Initialize(x, true, FileEncodingType));

				await taskForeach;

				return await observable.ToList();
			}
			else
			{
				return await GetMany(predicate);
			}
		}

		/// <summary>
		/// File Should have already been Initialized and should have data
		/// Containing folder should match folder repository is attached to or subdirectories
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task<IFile> Create(IFile entity)
		{
			if (entity == null || entity.Data == null)
				throw new ArgumentNullException($"{nameof(entity)} or {nameof(entity.Data)} is Null");

			if (_directoryInfo.GetFiles(entity.Name, _searchOption).Count() != 0)
				throw new System.IO.IOException("File Already Exists");

			//init file info backing
			if (entity.FileInfo == null)
				Initialize(entity, path: this._directoryInfo.FullName);

			var data = decodeData(entity);

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

			return entity;
		}

		/// <summary>
		/// Files Should have already been Initialized and should have data
		/// ContainingFolders should match folder repository is attached to or subdirectories
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		/// <exception cref="AggregateException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task<IEnumerable<IFile>> CreateRange(IEnumerable<IFile> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			var exceptionLazyList = new Lazy<List<Exception>>();

			foreach (var entity in entities)
			{
				try
				{
					var result = await Create(entity);
				}
				catch (Exception e)
				{
					if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
					{
						exceptionLazyList.Value.Add(e);
					}
				}
			}

			if (exceptionLazyList.Value.Count > 0)
			{
				throw new AggregateException(exceptionLazyList.Value);
			}

			return entities;
		}

		public async Task<IFile> Update(IFile entity)
		{
			if (entity == null && entity.Data == null)
				throw new ArgumentNullException($"{nameof(entity)} or {nameof(entity.Data)} is Null");

			//Make sure file exists
			if (!(_directoryInfo.GetFiles(entity.Name, _searchOption).Count() == 1))
				throw new System.IO.IOException($"File: {entity.Name} does not Exist");

			//init file backing
			if (entity.FileInfo == null)
				Initialize(entity, path: "");

			//decode data if encoded
			var data = decodeData(entity);

			//if file has changed create new file and delete old one
			if (entity.FileInfo.Length != data.Length)
			{
				//delete existing file and then write a new one
				if (entity.FileInfo.IsReadOnly)
					throw new System.IO.IOException($"File {entity.Name} is Read Only");

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

			return entity;
		}

		//we should treat files as immutable and just delete them
		//this way if the underlying file's bytes change we can just make a new one and rewrite the bytes to disk

		public async Task<IEnumerable<IFile>> UpdateRange(IEnumerable<IFile> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			var exceptionLazyList = new Lazy<List<Exception>>();

			foreach (var entity in entities)
			{
				try
				{
					var result = await Update(entity);
				}
				catch (Exception e)
				{
					if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
					{
						exceptionLazyList.Value.Add(e);
					}
				}
			}

			if (exceptionLazyList.Value.Count > 0)
			{
				throw new AggregateException(exceptionLazyList.Value);
			}

			return entities;
		}

		//todo add check to see if file exists
		//if file does not exists return true
		public Task<bool> Delete(IFile entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

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
			catch (Exception)
			{
				return Task.FromResult(false);
			}
		}

		public async Task<bool> DeleteRange(IEnumerable<IFile> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			var entityCount = entities.Count();

			var exceptionLazyList = new Lazy<List<Exception>>();

			foreach (var entity in entities)
			{
				try
				{
					var result = await Delete(entity);
				}
				catch (Exception e)
				{
					if (exceptionLazyList.Value != null && exceptionLazyList.IsValueCreated)
					{
						exceptionLazyList.Value.Add(e);
					}
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

		public IEnumerator<IFile> GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetAllFilesObservable().ToEnumerable().GetEnumerator();
		}

		public Task<long> Count()
		{
			return Task.FromResult(_directoryInfo.GetFiles("*", _searchOption).LongCount());
		}
	}
}