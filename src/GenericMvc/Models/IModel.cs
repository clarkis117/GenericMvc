using System;
using System.IO;

namespace GenericMvc.Models
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

	public interface IModelFile<TKey> : IFile, IModel<TKey>
		where TKey : IEquatable<TKey>
	{

	}

	public interface IRelatedFile<TKey> : IModelFile<TKey>
		where TKey : IEquatable<TKey>
	{
		TKey ParentObjectId { get; set; }
	}
}