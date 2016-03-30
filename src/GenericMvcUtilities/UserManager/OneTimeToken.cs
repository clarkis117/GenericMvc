using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace GenericMvcUtilities.UserManager
{
	/// <summary>
	/// One time server generated auth token
	/// 
	/// HMAC Based One Time Authentication Token
	/// Key:
	/// Where Key is secret like a password or password hash value
	/// 
	/// Hash Value = Expiration Date + Secure RNG
	/// </summary>
	public class OneTimeToken : IDisposable
	{
		private HMACSHA256 _hmacSha;

		private RandomNumberGenerator _rng;

		private DateTimeOffset _issueTime;

		public byte[] TokenStamp { get; set; }

		public DateTimeOffset ExpirationDate {
			get { return _issueTime.AddHours(1); }
		}

		public byte[] Value { get; set; }

		public OneTimeToken(byte[] key)
		{
			if (key != null || key.Length <= 0)
			{
				_hmacSha = new HMACSHA256(key);

				TokenStamp = new byte[128];

				_rng = System.Security.Cryptography.RandomNumberGenerator.Create();

				_rng.GetBytes(TokenStamp);

				_issueTime = DateTimeOffset.UtcNow;
			}
			else
			{
				throw new ArgumentNullException(nameof(key));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OneTimeToken"/> class.
		/// This method is the constructor for recreating another token to compare against
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="tokenStamp">The token stamp.</param>
		/// <param name="expiration">The expiration.</param>
		/// <exception cref="ArgumentNullException">
		/// </exception>
		public OneTimeToken(byte[] key, byte[] tokenStamp, DateTimeOffset expiration)
		{
			if (key != null || key.Length <= 0)
			{
				_hmacSha = new HMACSHA256(key);

				if (tokenStamp != null || tokenStamp.Length <= 0)
				{
					TokenStamp = tokenStamp;
				}
				else
				{
					throw new ArgumentNullException(nameof(tokenStamp));
				}

				_rng = System.Security.Cryptography.RandomNumberGenerator.Create();

				_issueTime = expiration.AddHours(-1);
			}
			else
			{
				throw new ArgumentNullException(nameof(key));
			}
		}

		public static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		//todo: test this
		private static bool IsTokenExpried(DateTimeOffset tokenTime)
		{
			var result = DateTimeOffset.Compare(tokenTime, DateTimeOffset.UtcNow);

			if (result <= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		public Task<bool> VerifyToken(byte[] token)
		{
			return Task.Run(() => {

				if(token != null || token.Length <= 0)
				{
					throw new ArgumentNullException(nameof(token));
				}

				if (Value == null || Value.Length <= 0)
				{
					GenerateToken().Wait();
				}

				return Value.SequenceEqual(token);
			});
		}

		//todo: maybe this can be done more efficiently 
		public Task<byte[]> GenerateToken()
		{
			return Task.Run(() => {
				
				//todo: maybe utf 8 bytes instead?
				var counter = GetBytes(ExpirationDate.ToString());
				
				int mergeSize = counter.Length + TokenStamp.Length;

				//set up memory stream for array merge
				MemoryStream merge = new MemoryStream(new byte[mergeSize], 0, mergeSize, true, true);

				merge.Write(TokenStamp, 0, TokenStamp.Length);

				merge.Write(counter, 0, counter.Length);

				Value = _hmacSha.ComputeHash(merge.ToArray());

				merge.Dispose();

				return Value;
			});
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects).
					_rng.Dispose();

					_hmacSha.Dispose();
				}

				// free unmanaged resources (unmanaged objects) and override a finalizer below.
				// set large fields to null.
	
				Value = null;

				TokenStamp = null;

				disposedValue = true;
			}
		}

		// override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~OneTimeToken() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
