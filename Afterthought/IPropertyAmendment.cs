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
using System.Reflection;

namespace Afterthought
{
	public interface IPropertyAmendment : IMemberAmendment
	{
		Type Type { get; }

		PropertyInfo PropertyInfo { get; }

		PropertyInfo Implements { get; }

		MethodInfo LazyInitializer { get; }

		MethodInfo Initializer { get; }

		MethodInfo Getter { get; }

		MethodInfo Setter { get; }

		MethodInfo BeforeGet { get; }

		MethodInfo AfterGet { get; }

		MethodInfo BeforeSet { get; }

		MethodInfo AfterSet { get; }

	    bool IsExplicitImplementation { get; }
	}
}
