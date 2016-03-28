﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Linq.Expressions;
using GenericMvcUtilities.Models;

namespace GenericMvcUtilities.Controllers
{
	/// <summary>
	/// Base Interface for all Web API nodes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBaseApiController<T, TKey>
		where T : class
		where TKey : IEquatable<TKey>
	{
		//todo: maybe a count method?

		//[HttpGet("{id:int}")]
		Task<T> Get(TKey id);

		//[HttpGet]
		Task<IEnumerable<T>> GetAll();

		//[HttpPost]
		Task<IActionResult> Create([FromBody] T item);

		//todo: see if this should return IEnumerable<T>
		//[HttpPost]
		Task<IActionResult> Creates([FromBody] T[] items);

		//[HttpPut("{id:int}")]
		Task<IActionResult> Update(TKey id, [FromBody] T item);

		//[HttpDelete("{id:int}")]
		Task<IActionResult> Delete(TKey id);
	}
}
