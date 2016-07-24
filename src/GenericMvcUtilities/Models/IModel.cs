using System;

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
	
}