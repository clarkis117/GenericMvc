using MimeDetective;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
	/// <summary>
	/// Byte Encodings for files
	///
	/// 1. RawBytes: Means raw exact bytes from Disk
	/// 2. Base64: Just let Json.NET serialize RawBytes to Base64
	///		-underlying data structure should still contain raw bytes, not base64 bytes
	///		-use case: is to inform that something will trans-code the bytes to base64 later on
	///
	///		-see Json.NET Serialization Guide: http://www.newtonsoft.com/json/help/html/SerializationGuide.htm
	///
	///	3. NonNewtonsoftBase64: i.e. serialize it to base64 yourself and handle whatever special case you need
	///
	/// </summary>
	public enum EncodingType : byte { RawBytes, Base64, NonNewtonsoftBase64 };

	/// <summary>
	/// This somewhat follows the data objects pattern
	/// where objects know how to get their own data
	///
	/// For File Retrieval: this is basically a lazy wrapper around file info
	/// For File Creation/Update: This is a data object not a wrapper, file repo assumes it's correct
	/// </summary>
	public class DataFile : IModel<string>
	{
		[JsonIgnore]
		public System.IO.FileInfo _fileInfo;

		[JsonIgnore]
		public MimeDetective.FileType _fileType;

		public DataFile()
		{
		}

		/*
		//todo rethink this
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
		*/

		/// <summary>
		///
		/// </summary>
		/// <param name="fileInfo"></param>
		public DataFile(System.IO.FileInfo fileInfo)
		{
			if (fileInfo != null && fileInfo.Exists)
			{
				_fileInfo = fileInfo;
			}
			else
			{
				throw new ArgumentNullException(nameof(fileInfo));
			}
		}

		/*
		public byte[] getData()
		{
			return System.IO.File.ReadAllBytes(this.getFullyQualifiedPath());
		}
		*/

		/// <summary>
		/// Does not load complete file from disk
		/// Initializes as file facade
		/// </summary>
		/// <returns></returns>
		public virtual DataFile Initialize(string path = "")
		{
			//default constructor handling
			if (_fileInfo == null)
			{
				if (path != "" && this.Name != null)
				{
					var filePath = System.IO.Path.Combine(path, this.Name);

					this._fileInfo = new System.IO.FileInfo(filePath);
				}
				else if (Id != null)
				{
					this._fileInfo = new System.IO.FileInfo(this.Id);
				}
				else
				{
					throw new InvalidOperationException("Object does not have require data to complete transaction");
				}
			}

			//set properties here
			Id = _fileInfo.FullName;

			Name = _fileInfo.Name;

			//ContainingFolder = _fileInfo.Directory.FullName;

			return this;
		}


		/// <summary>
		/// Assumes file has already been created
		/// Does not load complete file from disk
		/// Initializes as file facade
		/// </summary>
		/// <returns></returns>
		public async virtual Task<DataFile> InitializeWithMime()
		{
			this.Initialize(path: "");

			if (!_fileInfo.Exists)
			{
				throw new System.IO.FileNotFoundException("File not found after getting file info", _fileInfo.Name);
			}

			//set properties
			_fileType = await _fileInfo.GetFileTypeAsync();

			//ie this mean we couldn't find the file type
			if (_fileType.Mime == null)
			{
				ContentType = "";
			}
			else
			{
				ContentType = _fileType.Mime;
			}

			return this;
		}

		//todo maybe throw because base64 could use more bytes than raw encoding
		/// <summary>
		/// Initializes file and reads file from disk with specified encoding
		/// </summary>
		/// <param name="loadFile"></param>
		/// <param name="encodingType"></param>
		/// <returns></returns>
		public virtual async Task<DataFile> Initialize(bool loadFile = false, EncodingType encodingType = EncodingType.RawBytes)
		{
			await InitializeWithMime();

			if (loadFile)
			{
				using (var stream = _fileInfo.OpenRead())
				{
					if (stream.CanRead && stream.Length <= int.MaxValue)
					{
						byte[] rawData = new byte[Convert.ToInt32(stream.Length)];

						await stream.ReadAsync(rawData, 0, Convert.ToInt32(stream.Length));

						this.EncodingType = encodingType;

						if (encodingType == EncodingType.NonNewtonsoftBase64)
						{
							this.Data = System.Text.Encoding.ASCII.GetBytes(Convert.ToBase64String(rawData));
						}
						else
						{
							//basically: if (encodingType == Encoding.Base64 || encodingType == Encoding.RawBytes)
							this.Data = rawData;
						}
					}
					else
					{
						throw new System.IO.IOException("Stream too Large to copy to buffer");
					}
				}
			}

			return this;
		}
	
		/// <summary>
		/// path to file plus name and extension
		/// wrapper for FileInfo.FullName
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// filename.ext
		/// wrapper for FileInfo.Name
		/// </summary>
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// mime content type
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// encoding type
		/// needed to decode data from over the wire transmissions
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public EncodingType EncodingType { get; set; } = EncodingType.Base64;

		/// <summary>
		/// if file facade this will be null
		/// </summary>
		public byte[] Data { get; set; }
	}
}