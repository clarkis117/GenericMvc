using GenericMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericMvc.Repositories
{
	public enum ExpressionType : byte { QueryByName, QueryByContentType, QueryByData }

	public class FileSystemVisitor : ExpressionVisitor
	{
		private static readonly Type typeOfIFile = typeof(IFile);

		private static readonly Type typeOfString = typeof(string);

		private const string Name = "Name";

		private const string ContentType = "ContentType";

		private const string Data = "Data";

		private static readonly IReadOnlyList<string> PropNames = new string[] { Name, ContentType, Data };

		//resulting types
		public bool ParameterIsIFile { get; protected set; }

		public bool IsEqualityOp { get; protected set; }

		public string MemberName { get; protected set; }

		public Type MemberType { get; protected set; }

		public object MemberValue { get; protected set; }

		public MemberInfo RightHandPropInfo { get; protected set; }

		public Type RighHandParentType { get; protected set; }

		public bool CanQueryByName()
		{
			if (MemberName != null && MemberName == Name
				&& MemberType != null && MemberType == typeOfString
				&& MemberValue != null && MemberValue is string
				&& IsEqualityOp)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			ParameterIsIFile = false;

			IsEqualityOp = false;

			MemberName = null;

			MemberType = null;

			RightHandPropInfo = null;

			RighHandParentType = null;
		}


		protected override Expression VisitBinary(BinaryExpression node)
		{
			//determine the type of method used
			//i.e if something like equals for contains
			//get enough info to call method later on
			//if op_Equality
			if (node.Method.Name.Contains("Equality"))
			{
				IsEqualityOp = true;
			}

			return base.VisitBinary(node);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			//determine type of parameter and if it is valid IFile
			//node.type will be type of X in lambda
			if (typeOfIFile.IsAssignableFrom(node.Type))
			{
				ParameterIsIFile = true;
			}

			return base.VisitParameter(node);
		}

		//several cases for getting value from expression
		//value is a literal 
		//value belongs to an object graph
		//	in this case we must get the type of the graph
		// then traverse the tree until we can find the type
		// then invoke property and get value from it
		protected override Expression VisitMember(MemberExpression node)
		{
			// If we've ended up with a constant, and it's a property or a field,
			// we can simplify ourselves to a constant

			//get the IFile property (Type and Name) that is being accessed in the expression
			if (node.NodeType == System.Linq.Expressions.ExpressionType.MemberAccess
			&& node.Member.MemberType == MemberTypes.Property
			&& PropNames.Contains(node.Member.Name))
			{
				//determine type of expression
				MemberName = node.Member.Name;

				MemberType = node.Type;

				return base.VisitMember(node);
			}
			else if (MemberName != null
				&& MemberType != null
				&& node.Type == MemberType) //get property access expression for right hand side
			{
				//get parent type
				RighHandParentType = node.Member.DeclaringType;

				RightHandPropInfo = node.Member;

				//RightHandDeclaringType = node.Expression.Type;

				//Expression expression = Visit(node.Expression);

				/*
				do
				{
					// Recurse down to see if we can simplify...
					expression = Visit(node.Expression);


					if (expression is ConstantExpression)
					{
						break;
					}
				}
				while (expression != null && !(expression.NodeType == System.Linq.Expressions.ExpressionType.MemberAccess));
				*/
			}

			return base.VisitMember(node);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (node.Type == MemberType)
			{
				MemberValue = node.Value;
			}
			else
			{
				FieldInfo field = null;

				foreach (var type in node.Value.GetType().GetFields())
				{
					if (type.FieldType == RighHandParentType)
					{
						field = type;
					}
				}

				if (field != null)
				{
					object container = field.GetValue(node.Value);

					if (RightHandPropInfo is PropertyInfo)
					{
						MemberValue = ((PropertyInfo)RightHandPropInfo).GetValue(container, null);
					}
					else if (RightHandPropInfo is FieldInfo)
					{
						MemberValue = ((FieldInfo)RightHandPropInfo).GetValue(container);
					}
				}
			}

			return base.VisitConstant(node);
		}
	}
}
