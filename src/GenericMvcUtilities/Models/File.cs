using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{

	/// <summary>
	/// This somewhat follows the data objects pattern
	/// where objects know how to get their own data
	/// 
	/// this is basically a lazy wrapper around file info
	/// </summary>
	public class File : IModel<string>
	{
		public System.IO.FileInfo _FileInfo;

		public File()
		{

		}

		public File(string fileName, string containingFolder, bool getData)
		{
			if (fileName != null)
			{
				this.Name = fileName;
			}
			else
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			if (fileName != null)
			{
				this.Name = fileName;
			}
			else
			{
				throw new ArgumentNullException(nameof(containingFolder));
			}

			if (getData)
			{
				this.Data = this.getData();
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileInfo"></param>
		public File(System.IO.FileInfo fileInfo)
		{
			if (fileInfo != null)
			{
				_FileInfo = fileInfo;
			}
			else
			{
				throw new ArgumentNullException(nameof(fileInfo));
			}
		}

		public byte[] getData()
		{
			return System.IO.File.ReadAllBytes(this.getFullyQualifiedPath());
		}

		public virtual Task<File> Initialize()
		{
			return Task.Run( () =>
			{
				if (!_FileInfo.Exists)
				{
					throw new System.IO.FileNotFoundException("File not found after getting file info", _FileInfo.Name);
				}

				Id = _FileInfo.FullName;

				ContainingFolder = _FileInfo.Directory.Name;

				ContentType = _FileInfo;

				return this;
			});
		}

		public virtual async Task Initialize(bool WithData = false)
		{
			await Initialize();

			if (WithData)
			{
				var open = _FileInfo.OpenRead();

				if (open.c)
				{

				}
			}
		}

		/// <summary>
		/// Containing folder + separator + filename
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// filename.ext
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// delimited by separator
		/// </summary>
		public string ContainingFolder { get; set; }

		/// <summary>
		/// mime content type
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// encoding type
		/// needed to decode data from over the wire transmissions
		/// </summary>
		public string EncodingType { get; set; }

		/// <summary>
		/// if file facade this will be null
		/// </summary>
		public byte[] Data { get; set; }
	}
}
