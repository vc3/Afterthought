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

using Microsoft.Build;
using Microsoft.Build.Framework;
using System;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Afterthought.Amender
{
	public sealed class Amender : AppDomainIsolatedTask
	{
		public override bool Execute()
		{
			DateTime start = DateTime.Now;
			Log.LogMessage(MessageImportance.High, "Amending {0}", TargetAssembly.Select(a => a.ItemSpec).ToArray());
			Program.Amend(TargetAssembly.First().ItemSpec, AmendmentAssemblies.Select(a => a.ItemSpec).ToArray(), ReferenceAssemblies.Select(a => a.ItemSpec).ToArray());
			Log.LogMessage(MessageImportance.High, "Amending Complete ({0:0.000} seconds)", DateTime.Now.Subtract(start).TotalSeconds);
			return true;
		}

		[Required]
		public ITaskItem[] TargetAssembly { get; set; }

		public ITaskItem[] AmendmentAssemblies { get; set; }

		public ITaskItem[] ReferenceAssemblies { get; set; }
	}
}