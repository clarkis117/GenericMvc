using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
	/// <summary>
	/// Class for approving users before they can register their account
	/// </summary>
	public class PendingUser<TKey> where TKey : IEquatable<TKey>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public TKey Id { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime DateRegistered { get; set; }

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
	}
}
