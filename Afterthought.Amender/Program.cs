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
				Amend(args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw;
			}
			return 0;
		} 

		internal static void Amend(params string[] targets)
		{
			// Ensure that at least one target assembly was specified
			if (targets == null || targets.Length == 0)
				throw new ArgumentException("At least one target assembly must be specified.");

			// Ensure that the target assemblies exist
			for (int i = 0; i < targets.Length; i++)
			{
				var path = targets[i] = Path.GetFullPath(targets[i]);
				if (!File.Exists(path))
					throw new ArgumentException("The specified target assembly, " + path + ", does not exist.");
			}

			// Determine the set of target directories and backup locations
			var directories = targets
				.Select(path => Path.GetDirectoryName(path).ToLower())
				.Distinct()
				.Select(directory => new { SourcePath = directory, BackupPath = Directory.CreateDirectory(Path.Combine(directory, "Backup")).FullName });

			// Determine the set of dlls, pdbs, and backup files
			var assemblies = targets
				.Select(dllPath => new
				{
					DllPath = dllPath,
					DllBackupPath = Path.Combine(Path.Combine(Path.GetDirectoryName(dllPath), "Backup"), Path.GetFileName(dllPath)),
					PdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb"),
					PdbBackupPath = Path.Combine(Path.Combine(Path.GetDirectoryName(dllPath), "Backup"), Path.GetFileNameWithoutExtension(dllPath) + ".pdb")
				});

			// Backup the directories containing the targeted dll and pdb files
			foreach (var directory in directories)
			{
				foreach (var file in Directory.GetFiles(directory.SourcePath))
				{
					if (file.ToLower().EndsWith("exe") || file.ToLower().EndsWith("dll") || file.ToLower().EndsWith("pdb"))
						File.Copy(file, Path.Combine(directory.BackupPath, Path.GetFileName(file)), true);
				}
			}

			// Register an assembly resolver to look in backup folders when resolving assemblies
			AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
			{
				foreach (var directory in directories)
				{
					var dependency = Path.Combine(directory.BackupPath, e.Name.Substring(0, e.Name.IndexOf(',')) + ".dll");
					if (File.Exists(dependency))
						return System.Reflection.Assembly.LoadFrom(dependency);
				}
				return null;
			};

			// Get the set of amendments to apply from all of the specified assemblies
			var amendments = AmendmentAttribute.GetAmendments(assemblies.Select(a => System.Reflection.Assembly.LoadFrom(a.DllBackupPath)).ToArray()).ToList();

			// Exit immediately if there are no amendments in the target assemblies
			if (amendments.Count == 0)
				return;

			// Process each target assembly individually
			foreach (var assembly in assemblies)
			{
				var assemblyAmendments = amendments.Where(a => a.Type.Assembly.Location == assembly.DllBackupPath).ToArray();
				if (assemblyAmendments.Length == 0)
					continue;

				Console.Write("Amending " + Path.GetFileName(assembly.DllPath));
				var start = DateTime.Now;

				using (var host = new PeReader.DefaultHost())
				{
					// Load the target assembly
					IModule module = host.LoadUnitFrom(assembly.DllBackupPath) as IModule;
					if (module == null || module == Dummy.Module || module == Dummy.Assembly)
						throw new ArgumentException(assembly.DllBackupPath + " is not a PE file containing a CLR assembly, or an error occurred when loading it.");

					// Copy the assembly to enable it to be mutated
					module = new MetadataDeepCopier(host).Copy(module);

					// Load the debug file if it exists
					PdbReader pdbReader = null;
					if (File.Exists(assembly.PdbBackupPath))
					{
						using (var pdbStream = File.OpenRead(assembly.PdbBackupPath))
						{
							pdbReader = new PdbReader(pdbStream, host);
						}
					}
					
					// Amend and persist the target assembly
					using (pdbReader)
					{
						// Create and execute a new assembly amender
						AssemblyAmender amender = new AssemblyAmender(host, pdbReader, assemblyAmendments);
						amender.TargetRuntimeVersion = module.TargetRuntimeVersion;
						module = amender.Visit(module);

						// Save the amended assembly back to the original directory
						using (var pdbWriter = pdbReader != null ? new PdbWriter(assembly.PdbPath, pdbReader) : null)
						{
							using (var dllStream = File.Create(assembly.DllPath))
							{
								PeWriter.WritePeToStream(module, host, dllStream, pdbReader, null, pdbWriter);
							}
						}
					}
				}
				Console.WriteLine(" (" + DateTime.Now.Subtract(start).TotalSeconds.ToString("0.000") + " seconds)");
			}
		}
	}
}
