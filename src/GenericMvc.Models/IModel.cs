using System;
using System.IO;

namespace GenericMvc.Models
{
	/// <summary>
	/// Interface for all models
	/// </summary>
	public interface IModel<TKey> where TKey : IEquatable<TKey>
	{
		TKey Id { get; set; }
	}
}