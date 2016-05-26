using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
	public class File : IModel<string>
	{
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

		public byte[] getData()
		{
			return System.IO.File.ReadAllBytes(this.getFullyQualifiedPath());
		}

		public string getFullyQualifiedPath()
		{
			return System.IO.Path.Combine(this.ContainingFolder, this.Name);
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
		/// </summary>
		public string EncodingType { get; set; }

		/// <summary>
		/// if file facade this will be null
		/// </summary>
		public byte[] Data { get; set; }
	}
}
