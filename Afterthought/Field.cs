//-----------------------------------------------------------------------------
//
// Copyright (c) VC3, Inc. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Afterthought
{
	#region Amendment.Field

	/// <summary>
	/// Abstract base class for concrete <see cref="Amendment<TType, TAmended>"/> which supports amending code into 
	/// a specific <see cref="Type"/> during compilation.
	/// </summary>
	public abstract partial class Amendment 
	{
		public abstract class Field : Member, IFieldAmendment
		{
			internal Field(string name)
				: base(name)
			{ }

			internal Field(FieldInfo field)
				: base(field.Name)
			{
				this.FieldInfo = field;
			}

			public FieldInfo FieldInfo { get; private set; }

			public abstract Type Type { get; }

			public override bool IsAmended
			{
				get
				{
					return base.IsAmended || InitializerMethod != null;
				}
			}

			MethodInfo IFieldAmendment.Initializer
			{
				get
				{
					return InitializerMethod;
				}
			}

			internal MethodInfo InitializerMethod { get; set; }
		}
	}

	#endregion

	#region Amendment<TAmended>.Field<F>

	public partial class Amendment<TAmended> : Amendment
	{
		public class Field<TField> : Field
		{
			public Field(string name)
				: base(name)
			{ }

			internal Field(FieldInfo field)
				: base(field)
			{ }

			public override Type Type
			{
				get
				{
					return typeof(TField);
				}
			}

			public delegate TField FieldInitializer(TAmended instance, string fieldName);

			public Field<TField> Initialize(FieldInitializer initializer)
			{
				base.InitializerMethod = initializer.Method;
				return this;
			}
		}
	}

	#endregion
}
