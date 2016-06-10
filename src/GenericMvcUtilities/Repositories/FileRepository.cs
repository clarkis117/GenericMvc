using GenericMvcUtilities.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
{
	public class FileRepository : IRepository<File>
	{
		private char DirectorySeparator;

		private string RootFolder;

		private System.IO.DirectoryInfo DirInfo;

		private System.IO.SearchOption _searchOption = System.IO.SearchOption.TopDirectoryOnly;

		private bool _includeNestedDriectories;

		private IAsyncEnumerator<System.IO.FileInfo> Enumerator;

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

		/// <summary>
		/// probably should have some file size limit here
		/// </summary>
		/// <param name="pathToRootFolder"></param>
		public FileRepository(string pathToRootFolder, bool includeNestedDirectories = false)
		{
			if (pathToRootFolder != null)
			{
				//check if folder exists here

				//var a = new FileRepository("dfadfad");

				//a.Insert(new File());

				if (System.IO.Directory.Exists(pathToRootFolder))
				{
					this.RootFolder = pathToRootFolder;

					this.DirInfo = new System.IO.DirectoryInfo(pathToRootFolder);
				}
				else
				{
					throw new ArgumentException(pathToRootFolder);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(pathToRootFolder));
			}

			this.InludedNestedDirectories = includeNestedDirectories;

			this.DirectorySeparator = System.IO.Path.DirectorySeparatorChar;
		}

		public IObservable<File> GetFilesFromFileInfo()
		{
			return Observable.Create<File>(async obs => {

				if(await this.Enumerator.MoveNext())
				{
					//do work
					var file = new File(this.Enumerator.Current);

					obs.OnNext(await file.Initialize());
				}
				else
				{
					obs.OnCompleted();

					this.Enumerator.Dispose();

					this.Enumerator = this.EnumerateFiles().GetEnumerator();
				}
		   });
		}

		/// <summary>
		/// Check and reduce
		/// then compile and return the delegate produced form the lambda
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		private Func<File, bool> checkAndCompilePredicate(Expression<Func<File, bool>> predicate)
		{
			if (predicate != null)
			{
				return predicate.Compile();
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		private IAsyncEnumerable<System.IO.FileInfo> EnumerateFiles()
		{
			return this.DirInfo.EnumerateFiles("*", _searchOption).ToAsyncEnumerable();
		}

		public Task<bool> Delete(File entity)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Exists(Expression<Func<File, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Takes a lambda expression as argument and compiles it
		/// then enumerates files in the root folder
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns>First Match Found</returns>
		public async Task<File> Get(Expression<Func<File, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			return new File(await this.EnumerateFiles().First(x => IsMatch(new File(x)) == true)) ?? null;
		}

		public Task<IEnumerable<File>> GetAll()
		{
			return Task.Run(() => GetFilesFromFileInfo().ToEnumerable());
		}

		public Task<File> GetCompleteItem(Expression<Func<File, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<File>> GetMultiple(Expression<Func<File, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Insert(File entity)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Inserts(ICollection<File> entities)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Update(File entity)
		{
			throw new NotImplementedException();
		}
	}
}