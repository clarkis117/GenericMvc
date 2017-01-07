using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Models
{
	/// <summary>
	/// Class for approving users before they can register their account
	/// </summary>
	public class PendingUser<TKey> : IPrivilegedUserConstraints, IModel<TKey>
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

		//Authentication attributes for pending user
		public string HashedPassword { get; set; }

		public bool HasUserBeenAdded { get; set; } = false;

		public byte[] SecurityStamp { get; set; }

		public DateTimeOffset StampExpiration { get; set; }
	}
}
