using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Cci.MutableCodeModel;
using Microsoft.Cci;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;

namespace ExoGraph.Injector
{
	/// <summary>
	/// Injects a graph instance parasite and property notification eventing into eligible classes.
	/// </summary>
	public class GraphTypeInjector : ILMutator
	{
		IModule exoGraphAssembly;
		ITypeDefinition graphInstance;
		ITypeDefinition exoGraphAttribute;

		public GraphTypeInjector(IMetadataHost host, PdbReader pdbReader)
			: base(host, pdbReader)
		{ }

		public override IAssembly Visit(IAssembly assembly)
		{
			string exoGraphPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "exograph.dll");
			exoGraphAssembly = (IModule)host.LoadUnitFrom(exoGraphPath);
			graphInstance = UnitHelper.FindType(host.NameTable, exoGraphAssembly, "ExoGraph.GraphInstance");
			exoGraphAttribute = UnitHelper.FindType(host.NameTable, exoGraphAssembly, "ExoGraph.ExoGraphAttribute");
			return base.Visit(assembly);
		}

		/// <summary>
		/// Create private GraphInstance __graphInstance backing field.
		/// </summary>
		/// <param name="namespaceTypeDefinition"></param>
		/// <returns></returns>
		public override NamespaceTypeDefinition Mutate(NamespaceTypeDefinition namespaceTypeDefinition)
		{
			// Only inject types marked with GraphTypeAttribute
			if (namespaceTypeDefinition.Attributes.Where(a => a.Type.ToString() == "ExoGraph.GraphTypeAttribute").Any())
			{
				// Add __graphInstance private field
				//var graphInstanceField = new FieldDefinition()
				//{
				//    Name = host.NameTable.GetNameFor("__graphInstance"),
				//    Type = graphInstance,
				//    InternFactory = host.InternFactory,

				//};
				//namespaceTypeDefinition.Fields.Add(graphInstanceField);

				//// Add IGraphInstance interface to type definition
				//namespaceTypeDefinition.Interfaces.Add(UnitHelper.FindType(host.NameTable, exoGraphAssembly, "ExoGraph.IGraphInstance"));

				//// Add ExoGraph.IGraphInstance.get_Instance getter
				//var get_Instance = new MethodDefinition
				//{
				//    ContainingTypeDefinition = namespaceTypeDefinition,
				//    Type = graphInstance,
				//    Name = host.NameTable.GetNameFor("ExoGraph.IGraphInstance.get_Instance"),
				//    IsSpecialName = true,
				//    IsHiddenBySignature = true,
				//    InternFactory = host.InternFactory,
				//    Body = new MethodBody()
				//    {
				//        Operations = new List<IOperation>() 
				//        {
				//            // Load this pointer onto stack
				//            new Operation() { OperationCode = OperationCode.Ldarg_0	},

				//            // Load graph instance from local field
				//            new Operation()	{ OperationCode = OperationCode.Ldfld, Value = graphInstanceField },

				//            // return the graph instance
				//            new Operation()	{ OperationCode = OperationCode.Ret },
				//        }
				//    }
				//};
				//namespaceTypeDefinition.Methods.Add(get_Instance);

				//// Add IGraphInstance.Instance property
				//namespaceTypeDefinition.Properties.Add(new PropertyDefinition()
				//{
				//    Accessors = new List<IMethodReference>() { get_Instance },
				//    ContainingTypeDefinition = namespaceTypeDefinition,
				//    Visibility = TypeMemberVisibility.Public,
				//    Name = host.NameTable.GetNameFor("ExoGraph.IGraphInstance.Instance"),
				//    Type = graphInstance,
				//    Getter = get_Instance
				//});
			}
			return base.Mutate(namespaceTypeDefinition);
		}

		/// <summary>
		/// Initialize __graphInstance backing field with a new <see cref="GraphInstance"/>.
		/// </summary>
		/// <param name="methodBody"></param>
		/// <returns></returns>
		public override MethodBody Mutate(MethodBody methodBody)
		{
			// Get the graph instance field for the class to determine if the current type is injectable
			var graphInstanceField = GetCurrentType().Fields.Where(f => f.Name.Value == "__graphInstance").FirstOrDefault();

			// Skip types that were not injected with a graph instance
			if (graphInstanceField == null)
				return base.Mutate(methodBody);

			// Inject GraphInstance initializer into each constructor
			if (methodBody.MethodDefinition.IsConstructor)
			{
				// Skip the constructor if it calls another constructor in the same class definition
				if (methodBody.Operations.Where(o => o.OperationCode == OperationCode.Call && ((IMethodReference)o.Value).ResolvedMethod.IsConstructor && ((IMethodReference)o.Value).ResolvedMethod.ContainingType == GetCurrentType()).Any())
					return base.Mutate(methodBody);

				// Load this pointer onto stack
				methodBody.Operations.Insert(0, new Operation()
				{
					OperationCode = OperationCode.Ldarg_0
				});

				// Load this pointer onto stack again
				methodBody.Operations.Insert(1, new Operation()
				{
					OperationCode = OperationCode.Ldarg_0
				});

				// Invoke GraphInstance constructor, passing current instance from stack
				methodBody.Operations.Insert(2, new Operation()
				{
					OperationCode = OperationCode.Newobj,
					Value = graphInstance
							.Methods
							.Where(m => m.IsConstructor && m.ParameterCount == 1)
							.FirstOrDefault()
				});

				// Store new graph instance in local field
				methodBody.Operations.Insert(3, new Operation()
				{
					OperationCode = OperationCode.Stfld,
					Value = graphInstanceField
				});
			}

			// Process method bodies for public properties
			else if (methodBody.MethodDefinition.IsHiddenBySignature && methodBody.MethodDefinition.IsSpecialName && (methodBody.MethodDefinition.Name.Value.StartsWith("get_") || methodBody.MethodDefinition.Name.Value.StartsWith("set_")))
			{
				// Determine which property is being injected
				IPropertyDefinition property = GetCurrentType().Properties
					.Where(p => p.Name.Value == methodBody.MethodDefinition.Name.Value.Substring(4) && p.Visibility == TypeMemberVisibility.Public)
					.FirstOrDefault();

				// Only process eligible properties
				if (property != null && property.Getter != null & property.Setter != null)
				{
					// Inject the getter
					if (methodBody.MethodDefinition.Name.Value.StartsWith("get_"))
					{
						// Load this pointer onto stack
						methodBody.Operations.Insert(0, new Operation()
						{
							OperationCode = OperationCode.Ldarg_0
						});

						// Load graph instance from local field
						methodBody.Operations.Insert(1, new Operation()
						{
							OperationCode = OperationCode.Ldfld,
							Value = graphInstanceField
						});

						// Load property name onto stack
						methodBody.Operations.Insert(2, new Operation()
						{
							OperationCode = OperationCode.Ldstr,
							Value = property.Name.Value
						});

						// Call OnPropertyGet
						methodBody.Operations.Insert(3, new Operation()
						{
							OperationCode = OperationCode.Callvirt,
							Value = graphInstance.Methods.Where(m => m.Name.Value == "OnPropertyGet").FirstOrDefault()
						});
					}

					// Inject the setter
					else // if (false)
					{
						methodBody.LocalVariables.Add(new LocalDefinition()
						{
							Name = host.NameTable.GetNameFor("__originalValue"),
							Type = property.Type
						});

						// Load this pointer onto stack
						methodBody.Operations.Insert(0, new Operation()
						{
							OperationCode = OperationCode.Ldarg_0
						});

						// Call the property getter to get the original value
						methodBody.Operations.Insert(1, new Operation()
						{
							OperationCode = property.Getter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call,
							Value = property.Getter.ResolvedMethod
						});

						// Store the original value in a local variable
						methodBody.Operations.Insert(2, new Operation()
						{
							OperationCode = OperationCode.Stloc,
							Value = methodBody.LocalVariables.Last()
						});

						// Load this pointer onto stack
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Ldarg_0
						});

						// Load graph instance from local field
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Ldfld,
							Value = graphInstanceField
						});

						// Load property name onto stack
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Ldstr,
							Value = property.Name.Value
						});

						// Load the original value onto stack
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Ldloc,
							Value = methodBody.LocalVariables.Last()
						});

						// Box value types
						if (property.Type.IsValueType)
						{
							methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
							{
								OperationCode = OperationCode.Box,
								Value = property.Type
							});
						}

						// Load this pointer onto stack
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Ldarg_0
						});

						// Call the property getter to get the original value
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = property.Getter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call,
							Value = property.Getter.ResolvedMethod
						});

						// Box value types
						if (property.Type.IsValueType)
						{
							methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
							{
								OperationCode = OperationCode.Box,
								Value = property.Type
							});
						}

						// Call OnPropertySet
						methodBody.Operations.Insert(methodBody.Operations.Count - 1, new Operation()
						{
							OperationCode = OperationCode.Callvirt,
							Value = graphInstance.Methods.Where(m => m.Name.Value == "OnPropertySet").FirstOrDefault()
						});
					}

					// Fix the operation offsets after injecting code
					return Process(methodBody);
				}
			}

			return base.Mutate(methodBody);
		}
	}
}
