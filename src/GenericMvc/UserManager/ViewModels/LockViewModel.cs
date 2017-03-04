using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GenericMvc.ViewModels.UserManager
{
	//todo: check for constancy with other parts of lockout process 
	public class LockViewModel
	{
		[Required]
		public string Id { get; set; }

		[Required]
		[Display(Name = "Select Lockout End time")]
		public DateTimeOffset? LockOutTime { get; set; }

		[Required]
		[Display(Name = "UTC Time Offset")]
		public int UTCTimeOffset { get; set; } = -5;

		public LockViewModel()
		{

		}

		public LockViewModel(object userId)
		{
			Id = userId.ToString();
		}

		public bool IsValid
		{
			get
			{
				if (Id != null && LockOutTime != null)
				{
					if (DateTimeOffset.Now < LockOutTime)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
		}

		public DateTimeOffset ConvertToLockoutTime()
		{
			return new DateTimeOffset(LockOutTime.Value.UtcDateTime, new TimeSpan(UTCTimeOffset, 0, 0));
		}
	}
}
