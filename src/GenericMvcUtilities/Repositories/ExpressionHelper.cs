using GenericMvcUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
{
	public static class ExpressionHelper
	{
		//static readonly fields for expressions
		//private readonly static ParameterExpression expressioOfT = Expression.Parameter(typeofT);

		private readonly static Type typeOfString = typeof(string);

		private readonly static MethodInfo containsMethodInfo = typeOfString.GetMethod(nameof(string.Contains), new[] { typeOfString });

		/// <summary>
		///
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		/// <returns></returns>
		public static Expression<Func<TEntity, bool>> SearchExpression<TEntity>(this IRepository<TEntity> repository, string propertyName, object propertyValue)
		{
			if (propertyValue.GetType() == typeOfString)
			{
				//see this SO Answer: http://stackoverflow.com/questions/278684/how-do-i-create-an-expression-tree-to-represent-string-containsterm-in-c

				var propertyExp = Expression.Property(repository.EntityExpression, propertyName);
				var someValue = Expression.Constant(propertyValue, typeOfString);
				var containsMethodExp = Expression.Call(propertyExp, containsMethodInfo, someValue);

				return Expression.Lambda<Func<TEntity, bool>>(containsMethodExp, repository.EntityExpression);
			}
			else
			{
				return IsMatchedExpression<TEntity>(repository, propertyName, propertyValue);
			}
		}

		/// <summary>
		/// Determines whether [is matched expression] [the specified property name].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <returns></returns>
		public static Expression<Func<T, bool>> IsMatchedExpression<T>(this IRepository<T> repository, string propertyName, object propertyValue)
		{
			var propertyOrField = Expression.PropertyOrField(repository.EntityExpression, propertyName);

			//todo is this needed? can it be made conditional
			var typeConversion = Expression.Convert(propertyOrField, propertyValue.GetType());

			var binaryExpression = Expression.Equal(typeConversion, Expression.Constant(propertyValue));
			return Expression.Lambda<Func<T, bool>>(binaryExpression, repository.EntityExpression);
		}

		/// <summary>
		/// Matches the by identifier expression.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public static Expression<Func<TEntity, bool>> MatchByIdExpression<TEntity,TKey>(this IRepository<TEntity> repository, TKey id)
			where TEntity : IModel<TKey>
			where TKey : IEquatable<TKey>
		{
			var propertyOrField = Expression.PropertyOrField(repository.EntityExpression, nameof(IModel<TKey>.Id));
			var binaryExpression = Expression.Equal(propertyOrField, Expression.Constant(id));
			return Expression.Lambda<Func<TEntity, bool>>(binaryExpression, repository.EntityExpression);
		}
	}
}
