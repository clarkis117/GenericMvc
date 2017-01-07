using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Models
{
	/// <summary>
	/// </summary>
	public interface IFile
	{
		//replaces Id in File... this is the path to the file
		//equal directory name
		[AutoMapper.IgnoreMap]
		string Path { get; set; }

		//file name as it appears on disk
		[AutoMapper.Configuration.Conventions.MapTo("Filename")]
		string Name { get; set; }

		/// <summary>
		/// mime content type
		/// </summary>
		[AutoMapper.IgnoreMap]
		string ContentType { get; set; }

		/// <summary>
		/// encoding type
		/// needed to decode data from over the wire transmissions
		/// </summary>
		[AutoMapper.IgnoreMap]
		EncodingType EncodingType { get; set; }

		/// <summary>
		/// if file facade this will be null
		/// </summary>
		[AutoMapper.IgnoreMap]
		byte[] Data { get; set; }

		[AutoMapper.IgnoreMap]
		FileInfo FileInfo { get; set; }

		bool IsFileBacked();

		//FileInfo GetFileInfo();
	}

	public struct FileStruct : IFile
	{
		public string Path { get; set; }

		public string Name { get; set; }

		public string ContentType { get; set; }

		public EncodingType EncodingType { get; set; }

		public byte[] Data { get; set; }

		public FileInfo FileInfo { get; set; }

		public bool IsFileBacked()
		{
			if (this.FileInfo != null && this.Path != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public FileStruct(FileInfo fileinfo)
		{
			Name = fileinfo.Name;
			Path = fileinfo.DirectoryName;
			ContentType = null;
			EncodingType = EncodingType.Base64;
			Data = null;
			FileInfo = fileinfo;
		}

		public FileStruct(string path, string name, string contentType, EncodingType type, byte[] data, FileInfo fileinfo)
		{
			Path = path;
			Name = name;
			ContentType = contentType;
			EncodingType = type;
			Data = data;
			FileInfo = fileinfo;
		}
	}

	public static class IFileExtensions
	{
		/// <summary>
		/// This replaces map file portion functions
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="source"></param>
		public static void MapFileProperties(this IFile destination, IFile source)
		{
			destination.Name = source.Name;
			destination.ContentType = source.ContentType;
			destination.EncodingType = source.EncodingType;
			destination.Path = source.Path;

			if (source.Data != null)
			{
				destination.Data = source.Data;
			}
		}
	}
}
