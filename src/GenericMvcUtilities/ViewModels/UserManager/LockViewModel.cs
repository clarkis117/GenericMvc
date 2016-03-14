using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GenericMvcUtilities.ViewModels.UserManager
{
	//todo: check for constancy with other parts of lockout process 
	public class LockViewModel
	{
		[Required]
		public object Id { get; set; }

		public DateTimeOffset? LockOutTime { get; set; }

		public bool RemovePassword { get; set; }
	}
}
