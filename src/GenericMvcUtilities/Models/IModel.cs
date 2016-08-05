using System;
using System.IO;

namespace GenericMvcUtilities.Models
{
	/// <summary>
	/// Interface for all models
	/// </summary>
	public interface IModel<TKey> where TKey : IEquatable<TKey> //IComparable<TKey> //,no this is for arrays IComparable<TKey>
	{
		TKey Id { get; set; }

		//string JsonType { get; }
	}

	
	public interface IModelWithFile<TKey> : IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		DataFile File { get; set; }
	}

	public interface IModelWithFilename<TKey> : IModel<TKey>
	where TKey : IEquatable<TKey>
	{
		[AutoMapper.Configuration.Conventions.MapTo("Name")]
		string Filename { get; set; }
	}


	/// <summary>
	/// todo use this to replace datafile
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <seealso cref="GenericMvcUtilities.Models.IModel{TKey}" />
	public interface IFile<TKey> : IModel<TKey> where TKey : IEquatable<TKey>
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

	public interface IRelatedFile<TKey> : IFile<TKey>
		where TKey : IEquatable<TKey>
	{
		TKey ParentObjectId { get; set; }
	}
}