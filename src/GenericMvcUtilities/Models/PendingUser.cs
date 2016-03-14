using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Models
{
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


		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
