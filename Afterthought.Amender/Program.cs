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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel; 
using Afterthought;
using System.Reflection;

namespace Afterthought.Amender
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				var targetAssembly = args[0];
				var amendmentAssemblies = args.Skip(1).Where(p => File.Exists(p)).ToArray();
				var referencePaths = args.Skip(1).Where(p => Directory.Exists(p)).ToArray();

				if (amendmentAssemblies.Length + referencePaths.Length + 1 != args.Length)
					throw new ArgumentException("Invalid file or directory");

				Amend(targetAssembly, amendmentAssemblies, referencePaths);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw;
			}
			return 0;
		} 

		/// <summary>
		/// Amends the specified target assembly, optionally using assembly amendments defined in the 
		/// specified amendment assemblies.
		/// </summary>
		/// <param name="targetAssembly"></param>
		/// <param name="amendmentAssemblies"></param>
		internal static void Amend(string targetAssembly, string[] amendmentAssemblies, string[] referenceAssemblies)
		{
			// Verify the target assembly exists
			targetAssembly = Path.GetFullPath(targetAssembly);
			if (!File.Exists(targetAssembly))
				throw new ArgumentException("The specified target assembly, " + targetAssembly + ", does not exist.");

			// Verify the amendment assemblies exist
			if (amendmentAssemblies == null)
				amendmentAssemblies = new string[0];
			for (int i = 0; i < amendmentAssemblies.Length; i++)
			{
				var path = amendmentAssemblies[i] = Path.GetFullPath(amendmentAssemblies[i]);
				if (!File.Exists(path))
					throw new ArgumentException("The specified amendment assembly, " + path + ", does not exist.");
			}

			// Verify that the target has not already been amended
			var afterthoughtTracker = targetAssembly + ".afterthought";
			if (File.Exists(afterthoughtTracker) && File.GetLastWriteTime(targetAssembly) == File.GetLastWriteTime(afterthoughtTracker))
				return;

			// Determine the set of target directories and backup locations
			var targetWriteTime = File.GetLastWriteTime(targetAssembly);
			var backupTargetAssembly = targetAssembly + ".backup";
			var targetDirectory = Path.GetDirectoryName(targetAssembly);
			File.Delete(backupTargetAssembly);
			File.Move(targetAssembly, backupTargetAssembly);

			// Build up a set of paths with resolving assemblies
			var referencePaths = new Dictionary<string, string>();
			foreach (string path in amendmentAssemblies
				.Union(referenceAssemblies)
				.Union(Directory.GetFiles(targetDirectory).Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && p != targetAssembly)))
				referencePaths[Path.GetFileName(path)] = path;

			// Register an assembly resolver to look in assembly directories when resolving assemblies
			AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
			{
				var assemblyName = new System.Reflection.AssemblyName(e.Name).Name + ".dll";
				string referencePath;
				if (referencePaths.TryGetValue(assemblyName, out referencePath))
					return System.Reflection.Assembly.LoadFrom(referencePath);

				return null;
			};

			// Get the set of amendments to apply from all of the specified assemblies
			var assemblies = new System.Reflection.Assembly[] { System.Reflection.Assembly.LoadFrom(backupTargetAssembly) }.Union(amendmentAssemblies.Select(a => System.Reflection.Assembly.LoadFrom(a)));
			var amendments = AmendmentAttribute.GetAmendments(assemblies.First(), assemblies.Skip(1).ToArray()).ToList();

			// Exit immediately if there are no amendments in the target assemblies
			if (amendments.Count == 0)
			    return;

			// Amend the target assembly
			Console.Write("Amending " + Path.GetFileName(targetAssembly));
			var start = DateTime.Now;

			using (var host = new PeReader.DefaultHost())
			{
				// Load the target assembly
				IModule module = host.LoadUnitFrom(backupTargetAssembly) as IModule;
				if (module == null || module == Dummy.Module || module == Dummy.Assembly)
					throw new ArgumentException(backupTargetAssembly + " is not a PE file containing a CLR assembly, or an error occurred when loading it.");

				// Copy the assembly to enable it to be mutated
				module = new MetadataDeepCopier(host).Copy(module);

				// Load the debug file if it exists
				PdbReader pdbReader = null;
				var pdbFile = Path.Combine(targetDirectory, Path.GetFileNameWithoutExtension(targetAssembly) + ".pdb");
				var backupPdbFile = pdbFile + ".backup";
				if (File.Exists(pdbFile))
				{
					File.Delete(backupPdbFile);
					File.Move(pdbFile, backupPdbFile);
					using (var pdbStream = File.OpenRead(backupPdbFile))
					{
						pdbReader = new PdbReader(pdbStream, host);
					}
				}
					
				// Amend and persist the target assembly
				using (pdbReader)
				{
					// Create and execute a new assembly amender
					AssemblyAmender amender = new AssemblyAmender(host, pdbReader, amendments, assemblies);
					amender.TargetRuntimeVersion = module.TargetRuntimeVersion;
					module = amender.Visit(module);

					// Save the amended assembly back to the original directory
					var localScopeProvider = pdbReader == null ? null : new ILGenerator.LocalScopeProvider(pdbReader);
					using (var pdbWriter = pdbReader != null ? new PdbWriter(pdbFile, pdbReader) : null)
					{
						using (var dllStream = File.Create(targetAssembly))
						{
							PeWriter.WritePeToStream(module, host, dllStream, pdbReader, localScopeProvider, pdbWriter);
						}
					}
				}

				File.SetLastWriteTime(targetAssembly, targetWriteTime);
				if (pdbReader != null)
					File.SetLastWriteTime(pdbFile, targetWriteTime);
			}

			Console.WriteLine(" (" + DateTime.Now.Subtract(start).TotalSeconds.ToString("0.000") + " seconds)");

			// Set the last write time of the afterthought tracker to match the amended assembly to prevent accidental reamending
			File.WriteAllText(afterthoughtTracker, "");
			File.SetLastWriteTime(afterthoughtTracker, File.GetLastWriteTime(targetAssembly));
		}
	}
}
