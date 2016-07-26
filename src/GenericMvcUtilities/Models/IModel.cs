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
		string Path { get; set; }

		//file name as it appears on disk
		string Name { get; set; }

		/// <summary>
		/// mime content type
		/// </summary>
		string ContentType { get; set; }

		/// <summary>
		/// encoding type
		/// needed to decode data from over the wire transmissions
		/// </summary>
		EncodingType EncodingType { get; set; }

		/// <summary>
		/// if file facade this will be null
		/// </summary>
		byte[] Data { get; set; }


		FileInfo FileInfo { get; set; }

		bool IsFileBacked();

		//FileInfo GetFileInfo();
	}
}