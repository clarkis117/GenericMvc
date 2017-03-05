using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeDetective;

namespace GenericMvc.Repositories
{
	public static class Utilities
	{
		public struct ValidationResult
		{
			public string ErrorMessage;

			public bool IsValid;

			public ValidationResult(bool isValid, string errorMessage)
			{
				IsValid = isValid;
				ErrorMessage = errorMessage;
			}
		}

		public static string GetSpecialCharString()
		{
			return Path.GetInvalidFileNameChars().ToString();
		}

		public static ValidationResult IsValidFileName(string filename)
		{
			if (!string.IsNullOrEmpty(filename))
			{
				ValidationResult result = new ValidationResult();

				var InValidChars = Path.GetInvalidFileNameChars();

				foreach (char charc in InValidChars)
				{
					if (filename.Contains(charc))
					{
						result.IsValid = false;
						result.ErrorMessage = "Name is not a Valid Name for a File, please remove any special characters: " + Utilities.GetSpecialCharString();
						return result;
					}

				}

				result.IsValid = true;
				result.ErrorMessage = "";
				return result;
			}
			else
			{
				return new ValidationResult(false, $"{nameof(filename)} is null or empty");
			}
		}

		public async static Task<ValidationResult> IsFileNameUnique<T>(string filename, IRepository<T> repo)
			where T : class, IFile
		{
			if (!string.IsNullOrEmpty(filename))
			{
				ValidationResult result = new ValidationResult();

				//see if any name matches
				if (await repo.Any(x => x.Name == filename))
				{
					result.IsValid = false;
					result.ErrorMessage = $"A file with the Name: {filename} already Exists, if you want to upload this file change the Name";
					return result;
				}
				else
				{
					result.IsValid = true;
					result.ErrorMessage = "";
					return result;
				}
			}
			else
			{
				return new ValidationResult(false,"File Name is null or empty");
			}
		}

		/// <summary>
		/// checks content type string against white list
		/// </summary>
		/// <param name="contenttype"></param>
		/// <param name="whiteList"></param>
		/// <returns></returns>
		public static ValidationResult IsWhiteListedContentType(string contenttype, string[] whiteList)
		{
			if (!string.IsNullOrEmpty(contenttype))
			{
				var result = new ValidationResult();

				if (whiteList.Any(x => x == contenttype))
				{
					result.IsValid = true;
					result.ErrorMessage = "";
					return result;
				}
				else
				{
					result.IsValid = false;
					result.ErrorMessage = $"ContentType: {contenttype} is not supported at this time";
					return result;
				}
			}
			else
			{
				return new ValidationResult(false, "Content Type is null or empty");
			}
		}

		/// <summary>
		/// take this check with a grain of salt
		/// checks content type against byte array content
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="model"></param>
		/// <param name="whiteList"></param>
		/// <returns></returns>
		public static ValidationResult IsWhiteListedContentType<T>(T model, string[] whiteList)
			where T : IFile
		{
			if (model != null && model.Data != null)
			{
				var result = new ValidationResult(false,"");

				var filetype = model.Data.GetFileType();

				if ((filetype.Mime != null && whiteList.Any(x => x == filetype.Mime))
					|| (model.ContentType != null && whiteList.Any(x => x == model.ContentType)))
				{
					result.IsValid = true;
					result.ErrorMessage = "";
					return result;
				}
				else
				{
					result.IsValid = false;
					result.ErrorMessage = $"ContentType: {filetype.Mime ?? "undefined"} is not supported at this time";
					return result;
				}
			}
			else
			{
				return new ValidationResult(false, "File and its Content cannot be null");
			}
		}


	}
}
