using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenericMvc.ViewModels.Basic
{
	public enum MessageType : byte { Success, Warning, Danger, Info }

	public class MessageViewModel
	{
		public MessageViewModel()
		{

		}

		public MessageViewModel(MessageType type, string text)
		{
			MessageType = type;

			Style = getStyle(type);

			Text = text;
		}

		public MessageViewModel(MessageType type, string text, bool isDismissable)
		{
			MessageType = type;

			Style = getStyle(type);

			IsDismissable = isDismissable;

			Text = text;
		}

		public string Style { get; } = "alert-danger";

		public bool IsDismissable { get; } = true;

		public MessageType MessageType { get; }

		public string Text { get; }

		private static string getStyle(MessageType type)
		{
			switch (type)
			{
				case MessageType.Success:
					return "alert-success";

				case MessageType.Warning:
					return "alert-warning";

				case MessageType.Danger:
					return "alert-danger";

				case MessageType.Info:
					return "alert-info";

				default:
					return "alert-danger";
			}
		}
	}

	public static class MessageExtensions
	{
		public static string GetAlertStyleClasses(this MessageViewModel message)
		{
			if (message.IsDismissable)
			{
				return $"alert alert-dismissible {message.Style}";
			}
			else
			{
				return $"alert {message.Style}";
			}
		}

		public static Task<IHtmlContent> RenderAsync(this MessageViewModel message, IHtmlHelper HTML)
		{
			return HtmlHelperPartialExtensions.PartialAsync(HTML, "/Views/Shared/BasicMvc/AlertMessage.cshtml", message);
		}
	}
}
