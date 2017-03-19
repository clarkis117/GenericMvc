using GenericMvc.Api;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericMvc.SinglePage
{
	public interface ISinglePageController<T, TKey> : IGraphApi<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		IActionResult Index();
	}
}
