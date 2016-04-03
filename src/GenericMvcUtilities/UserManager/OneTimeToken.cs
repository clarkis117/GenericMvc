using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text;

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

		/// <summary>
		/// Initializes a new instance of the <see cref="OneTimeToken"/> class.
		/// Creates a new token for Verification 
		/// </summary>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
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
		/// Recreates a previously issued token for verification
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

		//todo: eval for removal
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

		private Task<bool> VerifyRawToken(byte[] token)
		{
			return Task.Run(() => {

				if(token == null || token.Length <= 0)
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

		public async Task<bool> VerifyToken(string utf8Token, DateTimeOffset expiration)
		{
			//if expired return false
			if (IsTokenExpried(expiration))
			{
				return false;
			}

			//if not null check token
			if (utf8Token != null)
			{
				if (Value != null && Value.Length > 0)
				{
					return (System.Text.Encoding.UTF8.GetString(Value) == utf8Token);
				}
				else
				{
					return (utf8Token == (await GenerateToken()));
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(utf8Token));
			}
		}

		//todo: maybe this can be done more efficiently 
		private Task<byte[]> GenerateRawToken()
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

				//todo: make this into a unit test
				//var value2 = _hmacSha.ComputeHash(merge.ToArray());
				//var ver = VerifyToken(value2).Result;

				merge.Dispose();

				return Value;
			});
		}


		public async Task<string> GenerateToken()
		{
			var rawToken = GenerateRawToken();

			return System.Text.Encoding.UTF8.GetString(await rawToken);
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
