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

	
	public interface IModel2 //<TKey> where TKey : class
	{
		ValueType Id { get; set; }

		//string JsonType { get; }
	}
	
}