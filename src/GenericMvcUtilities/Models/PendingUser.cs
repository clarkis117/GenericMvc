using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
	//todo: find a way to do key while maintaining pending user model
	public interface IPendingUser
	{
		object Id { get; set; }

		[Required]
		string FirstName { get; set; }

		[Required]
		string LastName { get; set; }

		[Required]
		[Phone]
		[DataType(DataType.PhoneNumber)]
		string PhoneNumber { get; set; }

		[Required]
		string RequestedRole { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		DateTime DateRegistered { get; set; }

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		string Email { get; set; }

		string HashedPassword { get; set; }

		bool HasUserBeenAdded { get; set; }
	}

	//todo: figure out password handling
	/// <summary>
	/// Class for approving users before they can register their account
	/// </summary>
	public class PendingUser<TKey> : IUserConstraints
		where TKey : IEquatable<TKey>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public TKey Id { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		[Phone]
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }

		//todo: fix handling of this through user manager
		[Required]
		public string RequestedRole { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime DateRegistered { get; set; }

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		public string HashedPassword { get; set; }

		public bool HasUserBeenAdded { get; set; } = false;
	}
}
