using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenericMvcUtilities.ViewModels.Basic
{
	public enum MessageType { Success, Warning, Danger, Info }

	public class MessageViewModel
	{
		public string Style { get; set; } = "alert-danger";

		public bool IsAlertDismissable { get; set; } = true;

		private MessageType _messageType;

		public MessageType MessageType
		{
			get
			{
				return _messageType;
			}
			set
			{
				switch (value)
				{
					case MessageType.Success:
						Style = "alert-success";
						break;
					case MessageType.Warning:
						Style = "alert-warning";
						break;
					case MessageType.Danger:
						Style = "alert-danger";
						break;
					case MessageType.Info:
						Style = "alert-info";
						break;
					default:
						Style = "alert-danger";
						break;
				}

				_messageType = value;
			}
		}

		public string GetAlertStyleClasses()
		{
			if (IsAlertDismissable)
			{
				return $"alert alert-dismissible {Style}";
			}
			else
			{
				return $"alert {Style}";
			}
		}

		public string Text { get; set; }


		public Task<IHtmlContent> RenderAsync(IHtmlHelper HTML)
		{
			return HtmlHelperPartialExtensions.PartialAsync(HTML, "/Views/Shared/BasicMvc/AlertMessage.cshtml", this);
		}
	}
}
