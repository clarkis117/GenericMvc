using GenericMvcUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;


namespace GenericMvcUtilities.Repositories
{
	public class FileRepository ////epository<File>
	{
		private char DirectorySeparator;

		private string RootFolder;

		/// <summary>
		/// probably should have some file size limit here
		/// </summary>
		/// <param name="pathToRootFolder"></param>
		public FileRepository(string pathToRootFolder)
		{
			if (pathToRootFolder != null)
			{
				//check if folder exists here

				//var a = new FileRepository("dfadfad");

				//a.Insert(new File());

				if (System.IO.Directory.Exists(pathToRootFolder))
				{
					this.RootFolder = pathToRootFolder;
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

			this.DirectorySeparator = System.IO.Path.DirectorySeparatorChar;
		}

		public Task<bool> Delete(File entity)
		{
			throw new NotImplementedException();
		}

		private Func<File, bool> checkAndCompilePredicate(Expression<Func<File, bool>> predicate)
		{
			if (predicate !=null)
			{
				return predicate.Compile();
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}


		public Task<bool> Exists(Expression<Func<File, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		/*
		public Task<File> Get(Expression<Func<File, bool>> predicate)
		{
			var IsMatch = this.checkAndCompilePredicate(predicate);

			var files = System.IO.Directory.EnumerateFiles(RootFolder);

			return new File();

			foreach (var item in files)
			{
				
				if (IsMatch(new File( item)))
				{

				}
				
			}
		
		}
		*/

		public Task<IEnumerable<File>> GetAll()
		{
			throw new NotImplementedException();
		}

		public Task<File> GetCompleteItem(Expression<Func<File, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<File>> GetMulti(Expression<Func<File, bool>> predicate)
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
