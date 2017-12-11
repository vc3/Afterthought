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
using Microsoft.Cci.MutableCodeModel;
using Microsoft.Cci;
using Microsoft.Cci.Immutable;
using System.Collections;

namespace Afterthought.Amender
{
	/// <summary>
	/// Amends an assembly using the specified type amendments.
	/// </summary>
	internal class AssemblyAmender : MutatingVisitor
	{
		static System.Reflection.MethodInfo VoidMethod = new Action(() => { }).Method;
		static System.Reflection.MethodInfo NotImplementedMethod = new Action(() => { throw new NotImplementedException(); }).Method;
		static System.Reflection.MethodInfo DefaultGetterMethod = new Func<object>(() => { return new object(); }).Method;
		static System.Reflection.MethodInfo DefaultSetterMethod = new Action<object>((value) => { }).Method;

		Dictionary<string, ITypeAmendment> typeAmendments;
		ITypeDefinition iTypeAmendment;
		ITypeDefinition iAmendmentAttribute;
		Dictionary<System.Reflection.Assembly, IAssembly> resolvedAssemblies = new Dictionary<System.Reflection.Assembly, IAssembly>();
		Dictionary<Type, ITypeDefinition> resolvedTypes = new Dictionary<Type, ITypeDefinition>();
		Dictionary<ITypeDefinition, Type> resolvedTypeDefs = new Dictionary<ITypeDefinition, Type>();

		internal AssemblyAmender(IMetadataHost host, PdbReader pdbReader, IEnumerable<ITypeAmendment> typeAmendments, IEnumerable<System.Reflection.Assembly> assemblies)
			: base(host, true)
		{
			// Preload assemblies
			foreach (var assembly in assemblies)
			{
				var assemblyDef = ResolveAssembly(assembly);
				foreach (var reference in assemblyDef.AssemblyReferences.Select(a => a.ResolvedAssembly).Where(a => a.Name.Value != "" && !a.Name.Value.StartsWith("mscorlib")))
				{
					var assemblyRef = System.Reflection.Assembly.LoadWithPartialName(reference.Name.Value);
					if (assemblyRef != null)
						CacheAssembly(assemblyRef, reference);
				}
			}

			this.typeAmendments = typeAmendments.ToDictionary(w => w.Type.FullName);
			iTypeAmendment = ResolveType(typeof(ITypeAmendment));
			iAmendmentAttribute = ResolveType(typeof(IAmendmentAttribute));
		}

		bool IsAmending { get; set; }

		internal string TargetRuntimeVersion { get; set; }

		Dictionary<IFieldDefinition, IFieldAmendment> Fields { get; set; }

		Dictionary<IMethodDefinition, IConstructorAmendment> Constructors { get; set; }

		Dictionary<IPropertyDefinition, IPropertyAmendment> Properties { get; set; }

		Dictionary<IMethodDefinition, IMethodAmendment> Methods { get; set; }

		Dictionary<IEventDefinition, IEventAmendment> Events { get; set; }

		Dictionary<IPropertyDefinition, GenericMethod> PropertyInitializers { get; set; }

		/// <summary>
		/// Performs mutations on namespace-level types.
		/// </summary>
		/// <param name="namespaceTypeDefinition"></param>
		/// <returns></returns>
		public override NamespaceTypeDefinition Mutate(NamespaceTypeDefinition namespaceTypeDefinition)
		{
			return AmendType(namespaceTypeDefinition) ? base.Mutate(namespaceTypeDefinition) : namespaceTypeDefinition;
		}

		/// <summary>
		/// Remove references to Afterthought from the target assembly.
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public override Assembly Mutate(Assembly assembly)
		{
			if (assembly.AssemblyReferences != null)
				assembly.AssemblyReferences.RemoveAll(a => a.Name.Value == "Afterthought");
			return base.Mutate(assembly);
		}

		/// <summary>
		/// Remove references to Afterthought from the target assembly.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public override Module Mutate(Module module)
		{
			if (module.AssemblyReferences != null)
				module.AssemblyReferences.RemoveAll(a => a.Name.Value == "Afterthought");
			return base.Mutate(module);
		}

		public override EventDefinition Mutate(EventDefinition eventDefinition)
		{
			return base.Mutate(eventDefinition);
		}

		/// <summary>
		/// Performs mutations on nested types.
		/// </summary>
		/// <param name="nestedTypeDefinition"></param>
		/// <returns></returns>
		public override NestedTypeDefinition Mutate(NestedTypeDefinition nestedTypeDefinition)
		{
			return AmendType(nestedTypeDefinition) ? base.Mutate(nestedTypeDefinition) : nestedTypeDefinition;
		}

		/// <summary>
		/// Applies amendments to namespace and nested types.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>True if the type is amended, otherwise false</returns>
		bool AmendType(NamedTypeDefinition type)
		{
			IsAmending = false;

			// Remove all attributes implementing IAmendmentAttribute
			if (type.Attributes != null)
				type.Attributes.RemoveAll(attr => TypeHelper.Type1ImplementsType2(attr.Type.ResolvedType, iAmendmentAttribute));

			// Get the Amendment for the current type definition
			ITypeAmendment typeAmendment;
			typeAmendments.TryGetValue(GetTypeName(type), out typeAmendment);

			// Exit immediately if the type is not being amended
			if (typeAmendment == null)
			{
				// Indicate that the type implement ITypeAmendment and should be mutated
				if (TypeHelper.Type1ImplementsType2(type, iTypeAmendment))
					return true;

				// Indicate that this type is not being amended and should not be mutated
				else
					return false;
			}

			IsAmending = true;

			// Attributes
			type = AddAttributes(type, typeAmendment);

			// Add new fields
			Fields = new Dictionary<IFieldDefinition, IFieldAmendment>();
			foreach (var field in typeAmendment.Fields)
			{
				if (field.FieldInfo == null)
				{
					if (type.Fields == null)
						type.Fields = new List<IFieldDefinition>();
					type.Fields.Add(new FieldDefinition()
					{
						Name = host.NameTable.GetNameFor(field.Name),
						Type = ResolveType(field.Type),
						InternFactory = host.InternFactory,
					});
				}
				else
					Fields.Add(ResolveField(type, field.FieldInfo), field);
			}

			// Constructors
			Constructors = new Dictionary<IMethodDefinition, IConstructorAmendment>();
			foreach (var constructor in typeAmendment.Constructors)
			{
				// Add a new method
				if (constructor.ConstructorInfo == null)
					AddConstructor(type, constructor);

				// Track existing properties being amended
				else
					Constructors.Add(ResolveConstructor(type, constructor.ConstructorInfo), constructor);
			}

			// Properties
			Properties = new Dictionary<IPropertyDefinition, IPropertyAmendment>();
			PropertyInitializers = new Dictionary<IPropertyDefinition, GenericMethod>();
			foreach (var property in typeAmendment.Properties)
			{
				IPropertyDefinition propertyDef;

				// Add a new property
				if (property.PropertyInfo == null)
					propertyDef = AddProperty(type, property);

				// Track existing properties being amended
				else
				{
					propertyDef = ResolveProperty(type, property.PropertyInfo);
					Properties.Add(propertyDef, property);
				}

				// Track property initializers
				if (property.Initializer != null)
					PropertyInitializers.Add(propertyDef, ResolvePropertyDelegate(propertyDef, property.Initializer));
			}

			// Methods
			Methods = new Dictionary<IMethodDefinition, IMethodAmendment>();
			foreach (var method in typeAmendment.Methods)
			{
				// Add a new method
				if (method.MethodInfo == null)
					AddMethod(type, method);

				// Track existing properties being amended
				else
					Methods.Add(ResolveMethod(type, method.MethodInfo), method);
			}

			// Events
			Events = new Dictionary<IEventDefinition, IEventAmendment>();
			foreach (var @event in typeAmendment.Events)
			{
				IEventDefinition eventDef;

				// Add a new event
				if (@event.EventInfo == null)
					eventDef = AddEvent(type, @event);

				// Track existing events being amended
				else
				{
					eventDef = ResolveEvent(type, @event.EventInfo);
					Events.Add(eventDef, @event);
				}
			}

			// Implement interfaces
			foreach (var interfaceType in typeAmendment.Interfaces)
			{
				// Get the corresponding interface definition
				ITypeDefinition interfaceDef = ResolveType(interfaceType);

				// Process all interface properties
				foreach (var property in interfaceType.GetProperties())
				{
					// Get the corresponding property definition
					var propertyDef = ResolveProperty(interfaceDef, property);

					// Determine if the interface property is already implemented by the type
					var existingProperty = type.Properties == null ? null : type.Properties.Where(p => AreEquivalent(p, property) && p.Visibility == TypeMemberVisibility.Public).FirstOrDefault();

					// Mark the existing property as implementing the interface
					if (existingProperty != null)
					{
						// Notify the type that the interface is being implemented by this property
						if (existingProperty.Getter != null)
							Implement(type, propertyDef.Getter, existingProperty.Getter);

						if (existingProperty.Setter != null)
							Implement(type, propertyDef.Setter, existingProperty.Setter);
					}

					// Determine if the property needs to be implicitly implemented as an auto property
					else if (!Properties.Values.Any(p => p.Implements == property))
						AddProperty(type, Afterthought.Amendment.Property.Implement(typeAmendment.Type, property));
				}

				// Process all interface methods
				foreach (var method in interfaceType.GetMethods())
				{
					// Get the corresponding method definition
					var methodDef = ResolveMethod(interfaceDef, method);

					// Determine if the interface method is already implemented by the type
					var existingMethod = type.Methods == null ? null : type.Methods.Where(m => AreEquivalent(m, method) && m.Visibility == TypeMemberVisibility.Public).FirstOrDefault();

					// Mark the existing method as implementing the interface
					if (existingMethod != null)
						Implement(type, methodDef, existingMethod);

					// TODO: Implicitly implement missing interface methods by throwing a not implemented exception
					//// Determine if the method needs to be implicitly implemented
					//else if (!Methods.Values.Any(m => m.Implements == method))
					//    AddMethod(type, Afterthought.Amendment.Method.Implement(typeAmendment.Type, method));
				}

				// Process all interface events
				foreach (var eventInfo in interfaceType.GetEvents())
				{
					// Get the corresponding event definition
					var eventDef = ResolveEvent(interfaceDef, eventInfo);

					// Determine if the interface event is already implemented by the type
					var existingEvent = type.Events == null ? null : type.Events.Where(e => AreEquivalent(e, eventInfo) && e.Visibility == TypeMemberVisibility.Public).FirstOrDefault();

					// Mark the existing event as implementing the interface
					if (existingEvent != null)
					{
						// Notify the type that the interface is being implemented by this event
						if (existingEvent.Adder != null)
							Implement(type, eventDef.Adder, existingEvent.Adder);

						if (existingEvent.Remover != null)
							Implement(type, eventDef.Remover, existingEvent.Remover);
					}

					// Determine if the event needs to be implicitly implemented as an auto event
					else if (!Events.Values.Any(e => e.Implements == eventInfo))
					{
						var args = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters()
							.SkipWhile((p, i) => (i == 0 && p.ParameterType == typeof(object)) || (i == 1 && p.ParameterType == typeof(EventArgs)))
							.Select(p => p.ParameterType).ToArray();
						var evt = Afterthought.Amendment.Event.Implement(typeAmendment.Type, eventInfo);
						AddEvent(type, evt);
						//if (interfaceType.GetMethod("On" + eventInfo.Name, args) != null)
						//	AddMethod(type, evt.RaisedBy("On" + eventInfo.Name));
					}
				}

				// Mark the type as implementing the interface
				if (type.Interfaces == null)
					type.Interfaces = new List<ITypeReference>();
				type.Interfaces.Add(interfaceDef);
			}

			// Indicate that this type is being mutated
			return true;
		}

		/// <summary>
		/// Indicates that a method implements an interface on a type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="implemented"></param>
		/// <param name="implementing"></param>
		void Implement(NamedTypeDefinition type, IMethodReference implemented, IMethodReference implementing)
		{
			if (type.ExplicitImplementationOverrides == null)
				type.ExplicitImplementationOverrides = new List<IMethodImplementation>();
			type.ExplicitImplementationOverrides.Add(new MethodImplementation()
			{
				ContainingType = type,
				ImplementedMethod = implemented,
				ImplementingMethod = implementing
			});
			
		}

		/// <summary>
		/// Adds a new constructor to a type definition.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="constructor"></param>
		/// <returns></returns>
		MethodDefinition AddConstructor(NamedTypeDefinition type, IConstructorAmendment constructor)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds a new property to a type definition.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		PropertyDefinition AddProperty(NamedTypeDefinition type, IPropertyAmendment property)
		{
			var propertyType = ResolveType(property.Type);
			bool isInterface = property.Implements != null;

			// Add the property
			var propertyDef = new PropertyDefinition()
			{
				Name = host.NameTable.GetNameFor(property.Name),
				Type = propertyType,
				ContainingTypeDefinition = type,
				CallingConvention = CallingConvention.HasThis,
				Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
				Accessors = new List<IMethodReference>()
			};
			if (type.Properties == null)
				type.Properties = new List<IPropertyDefinition>();
			type.Properties.Add(propertyDef);

			// Optionally create a getter
			if (property.Getter != null || (isInterface && property.Implements.GetGetMethod() != null) || (!isInterface && property.Setter == null))
			{
				// Create the getter method
				var getter = new MethodDefinition
				{
					ContainingTypeDefinition = type,
					Type = propertyType,
					Name = host.NameTable.GetNameFor((isInterface ? property.Implements.DeclaringType.FullName + "." : "") +
							"get_" + (isInterface ? property.Implements.Name : property.Name)),
					IsSpecialName = true,
					IsHiddenBySignature = true,
					IsCil = true,
					IsNewSlot = isInterface,
					IsVirtual = isInterface,
					IsSealed = isInterface,
					Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
					CallingConvention = CallingConvention.HasThis,
					InternFactory = host.InternFactory,
					Body = new MethodBody()
				};

				// Association the getter with the property definition
				propertyDef.Getter = getter;
				propertyDef.Accessors.Add(getter);
				((MethodBody)getter.Body).MethodDefinition = getter;
				if (type.Methods == null)
					type.Methods = new List<IMethodDefinition>();
				type.Methods.Add(getter);
				if (isInterface)
					Implement(type, ResolveProperty(ResolveType(property.Implements.DeclaringType), property.Implements).Getter, getter);

			}

			// Optionally create a setter
			if (property.Setter != null || (isInterface && property.Implements.GetSetMethod() != null) || (!isInterface && property.Getter == null))
			{
				// Create the setter method
				var setter = new MethodDefinition
				{
					ContainingTypeDefinition = type,
					Type = host.PlatformType.SystemVoid,
					Name = host.NameTable.GetNameFor((isInterface ? property.Implements.DeclaringType.FullName + "." : "") +
							"set_" + (isInterface ? property.Implements.Name : property.Name)),
					IsSpecialName = true,
					IsHiddenBySignature = true,
					IsCil = true,
					IsNewSlot = isInterface,
					IsVirtual = isInterface,
					IsSealed = isInterface,
					Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
					CallingConvention = CallingConvention.HasThis,
					InternFactory = host.InternFactory,
					Body = new MethodBody(),
					Parameters = new List<IParameterDefinition>() { new ParameterDefinition() 
						{
							Index = 0,
							Name = host.NameTable.value,
							Type = propertyType
						}}
				};

				// Association the setter with the property definition
				propertyDef.Setter = setter;
				propertyDef.Accessors.Add(setter);
				((MethodBody)setter.Body).MethodDefinition = setter;
				if (type.Methods == null)
					type.Methods = new List<IMethodDefinition>();
				type.Methods.Add(setter);
				if (isInterface)
					Implement(type, ResolveProperty(ResolveType(property.Implements.DeclaringType), property.Implements).Setter, setter);
			}

			// Add a backing field for new properties that do not have a getter or setter defined
			if (property.PropertyInfo == null && property.Getter == null && property.Setter == null)
			{
				// Add the backing field
				var backingField = new FieldDefinition()
				{
					Name = host.NameTable.GetNameFor(GetBackingFieldName(property)),
					Type = propertyType,
					InternFactory = host.InternFactory,
				};
				if (type.Fields == null)
					type.Fields = new List<IFieldDefinition>();
				type.Fields.Add(backingField);
			}

			// Track the newly added property
			Properties.Add(propertyDef, property);

			// Return the new property definition
			return propertyDef;
		}

		/// <summary>
		/// Adds a new method to a type definition.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		MethodDefinition AddMethod(NamedTypeDefinition type, IMethodAmendment method)
		{
			bool isInterface = method.Implements != null;

			// Determine the method arguments
			var args =
				method.Implementation != null ?
				method.Implementation.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray() :
				(
					method.Overrides != null ?
					method.Overrides.GetParameters().Select(p => p.ParameterType) :
					method.Raises.Type.GetMethod("Invoke").GetParameters()
						.SkipWhile((p, i) => (i == 0 && p.ParameterType == typeof(object)) || (i == 1 && p.ParameterType == typeof(EventArgs)))
						.Select(p => p.ParameterType).ToArray()
				);

			// Add the method
			var methodDef = new MethodDefinition
			{
				ContainingTypeDefinition = type,
				Type = method.Raises != null ?
					(ITypeReference)host.PlatformType.SystemVoid :
					ResolveType((method.Implementation ?? method.Overrides).ReturnType),
				Name = host.NameTable.GetNameFor(method.Name),
				IsSpecialName = isInterface,
				IsHiddenBySignature = isInterface,
				IsCil = true,
				IsNewSlot = isInterface,
				IsVirtual = isInterface || method.Overrides != null,
				IsSealed = isInterface,
				Visibility =
					isInterface ? TypeMemberVisibility.Private :
					(method.Overrides != null && !method.Overrides.IsPublic ? TypeMemberVisibility.Family :
					TypeMemberVisibility.Public),
				CallingConvention = CallingConvention.HasThis,
				InternFactory = host.InternFactory,
				Body = new MethodBody(),
				Parameters = args.Select((parameterType, index) =>
						new ParameterDefinition()
						{
							Index = (ushort)index,
							Name = host.NameTable.GetNameFor("arg" + index),
							Type = ResolveType(parameterType)
						}
					)
					.Cast<IParameterDefinition>()
					.ToList<IParameterDefinition>()
			};

			((MethodBody)methodDef.Body).MethodDefinition = methodDef;
			if (type.Methods == null)
				type.Methods = new List<IMethodDefinition>();
			type.Methods.Add(methodDef);
			if (isInterface)
				Implement(type, ResolveMethod(ResolveType(method.Implements.DeclaringType), method.Implements), methodDef);

			// Track the newly added method
			Methods.Add(methodDef, method);

			// Return the new method definition
			return methodDef;
		}

		/// <summary>
		/// Adds a new event to a type definition.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="event"></param>
		/// <returns></returns>
		EventDefinition AddEvent(NamedTypeDefinition type, IEventAmendment @event)
		{
			var eventType = ResolveType(@event.Type);
			bool isInterface = @event.Implements != null;

			// Add the event
			var eventDef = new EventDefinition()
			{
				Name = host.NameTable.GetNameFor(@event.Name),
				Type = eventType,
				ContainingTypeDefinition = type,
				Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
				Accessors = new List<IMethodReference>()
			};
			if (type.Events == null)
				type.Events = new List<IEventDefinition>();
			type.Events.Add(eventDef);

			// Optionally create an adder
			if (@event.Adder != null || (isInterface && @event.Implements.GetAddMethod() != null) || (!isInterface && @event.Remover == null))
			{
				// Create the adder method
				var adder = new MethodDefinition
				{
					ContainingTypeDefinition = type,
					Type = host.PlatformType.SystemVoid,
					Name = host.NameTable.GetNameFor((isInterface ? @event.Implements.DeclaringType.FullName + "." : "") +
							"add_" + (isInterface ? @event.Implements.Name : @event.Name)),
					IsSpecialName = true,
					IsHiddenBySignature = true,
					IsCil = true,
					IsNewSlot = isInterface,
					IsVirtual = isInterface,
					IsSealed = isInterface,
					Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
					CallingConvention = CallingConvention.HasThis,
					InternFactory = host.InternFactory,
					Body = new MethodBody(),
					Parameters = new List<IParameterDefinition>() { new ParameterDefinition() 
						{
							Index = 0,
							Name = host.NameTable.value,
							Type = eventType
						}}
				};

				// Associate the adder with the event definition
				eventDef.Adder = adder;
				eventDef.Accessors.Add(adder);
				((MethodBody)adder.Body).MethodDefinition = adder;
				if (type.Methods == null)
					type.Methods = new List<IMethodDefinition>();
				type.Methods.Add(adder);
				if (isInterface)
					Implement(type, ResolveEvent(ResolveType(@event.Implements.DeclaringType), @event.Implements).Adder, adder);

			}

			// Optionally create a remover
			if (@event.Remover != null || (isInterface && @event.Implements.GetRemoveMethod() != null) || (!isInterface && @event.Adder == null))
			{
				// Create the remover method
				var remover = new MethodDefinition
				{
					ContainingTypeDefinition = type,
					Type = host.PlatformType.SystemVoid,
					Name = host.NameTable.GetNameFor((isInterface ? @event.Implements.DeclaringType.FullName + "." : "") +
							"remove_" + (isInterface ? @event.Implements.Name : @event.Name)),
					IsSpecialName = true,
					IsHiddenBySignature = true,
					IsCil = true,
					IsNewSlot = isInterface,
					IsVirtual = isInterface,
					IsSealed = isInterface,
					Visibility = isInterface ? TypeMemberVisibility.Private : TypeMemberVisibility.Public,
					CallingConvention = CallingConvention.HasThis,
					InternFactory = host.InternFactory,
					Body = new MethodBody(),
					Parameters = new List<IParameterDefinition>() { new ParameterDefinition() 
						{
							Index = 0,
							Name = host.NameTable.value,
							Type = eventType
						}}
				};

				// Associate the remover with the event definition
				eventDef.Remover = remover;
				eventDef.Accessors.Add(remover);
				((MethodBody)remover.Body).MethodDefinition = remover;
				if (type.Methods == null)
					type.Methods = new List<IMethodDefinition>(); type.Methods.Add(remover);
				if (isInterface)
					Implement(type, ResolveEvent(ResolveType(@event.Implements.DeclaringType), @event.Implements).Remover, remover);
			}

			// Add a backing field for new properties that do not have a adder or remover defined
			if (@event.EventInfo == null && @event.Adder == null && @event.Remover == null)
			{
				// Add the backing field
				var backingField = new FieldDefinition()
				{
					Name = host.NameTable.GetNameFor(@event.Name),
					Type = eventType,
					InternFactory = host.InternFactory,
				};
				if (type.Fields == null)
					type.Fields = new List<IFieldDefinition>();
				type.Fields.Add(backingField);
			}

			// Track the newly added event
			Events.Add(eventDef, @event);

			// Return the new event definition
			return eventDef;
		}

		/// <summary>
		/// Generates <see cref="IMetadataExpression"/> information that encapsulates a given type and value
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private IMetadataExpression GetMetadataExpression(Type type, object value)
		{
			var typeDef = ResolveType(type);

			if (value == null)
				return new MetadataConstant { Type = typeDef, Value = null };

			if (typeof(Type).IsAssignableFrom(type))
				return new MetadataTypeOf { Type = typeDef, TypeToGet = ResolveType((Type)value) };

			if (type.IsArray)
			{
				// Recursively generate initial state for each element of the array
				var initializers = (from e in ((IEnumerable)value).Cast<object>()
									select GetMetadataExpression(e.GetType(), e)).ToList();

				return new MetadataCreateArray { Type = typeDef, ElementType = ResolveType(type.GetElementType()), Initializers = initializers };
			}

			return new MetadataConstant { Type = typeDef, Value = value };
		}

		/// <summary>
		/// Adds attributes to a type definition
		/// </summary>
		/// <param name="typeDef"></param>
		/// <param name="member"></param>
		/// <returns></returns>
		private NamedTypeDefinition AddAttributes(NamedTypeDefinition typeDef, IMemberAmendment member)
		{
			foreach (var attribute in member.Attributes)
			{
				// Get the constructor for the attribute
				IMethodReference ctor = TypeHelper.GetMethod(
					ResolveType(attribute.Type),
					host.NameTable.GetNameFor(".ctor"),
					attribute.Constructor.GetParameters().Select(p => ResolveType(p.ParameterType)).ToArray()
				);

				// Create an IMetadataExpression list for all attributes being passed to the constructor
				List<IMetadataExpression> args = (from a in attribute.Arguments
												  select GetMetadataExpression(a.GetType(), a)).ToList();

				if (typeDef.Attributes == null)
					typeDef.Attributes = new List<ICustomAttribute>();
				typeDef.Attributes.Add(new CustomAttribute
				{
					Constructor = ctor,
					Arguments = args
				});
			}

			return typeDef;
		}

		/// <summary>
		/// Adds attributes to member definition from amendments
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="typeDefMember"></param>
		/// <param name="member"></param>
		/// <returns></returns>
		private T AddAttributes<T>(T typeDefMember, IMemberAmendment member)
			where T : TypeDefinitionMember
		{
			foreach (var attribute in member.Attributes)
			{
				// Get the constructor for the attribute
				IMethodReference ctor = TypeHelper.GetMethod(
					ResolveType(attribute.Type),
					host.NameTable.GetNameFor(".ctor"),
					attribute.Constructor.GetParameters().Select(p => ResolveType(p.ParameterType)).ToArray()
				);

				// Create an IMetadataExpression list for all attributes being passed to the constructor
				List<IMetadataExpression> args = (from a in attribute.Arguments
												  select GetMetadataExpression(a.GetType(), a)).ToList();

				if (typeDefMember.Attributes == null)
					typeDefMember.Attributes = new List<ICustomAttribute>();
				typeDefMember.Attributes.Add(new CustomAttribute
				{
					Constructor = ctor,
					Arguments = args
				});
			}

			return typeDefMember;
		}

		/// <summary>
		/// Appends any additional attributes
		/// </summary>
		/// <param name="propertyDefinition"></param>
		/// <returns></returns>
		public override PropertyDefinition Mutate(PropertyDefinition propertyDefinition)
		{
			if (!IsAmending)
				return propertyDefinition;

			IPropertyAmendment propertyAdmendment;

			if (!Properties.TryGetValue(propertyDefinition, out propertyAdmendment))
				return propertyDefinition;

			propertyDefinition = AddAttributes<PropertyDefinition>(propertyDefinition, propertyAdmendment);

			return base.Mutate(propertyDefinition);
		}

		/// <summary>
		/// Append any additional attributes
		/// </summary>
		/// <param name="fieldDefinition"></param>
		/// <returns></returns>
		public override FieldDefinition Mutate(FieldDefinition fieldDefinition)
		{
			// Add attributes
			if (IsAmending)
			{
				IFieldAmendment fieldAmendment;
				if (Fields.TryGetValue(fieldDefinition, out fieldAmendment))
					AddAttributes<FieldDefinition>(fieldDefinition, fieldAmendment);
			}

			return base.Mutate(fieldDefinition);
		}

		/// <summary>
		/// Changes visibility on static methods of Amendments to make them callable.
		/// </summary>
		/// <param name="methodDef"></param>
		/// <returns></returns>
		public override MethodDefinition Mutate(MethodDefinition methodDef)
		{
			// Automatically make all private static methods have internal scope
			if (TypeHelper.Type1ImplementsType2(GetCurrentType(), iTypeAmendment) && methodDef.IsStatic && methodDef.Visibility == TypeMemberVisibility.Private)
				methodDef.Visibility = TypeMemberVisibility.Assembly;

			if (IsAmending)
			{
				IMethodAmendment methodAmendment;

				if (methodDef.IsConstructor)
				{
					IConstructorAmendment ctorAmendment;
					if (!Constructors.TryGetValue(methodDef, out ctorAmendment))
						return base.Mutate(methodDef);

					methodDef = AddAttributes<MethodDefinition>(methodDef, ctorAmendment);
				}
				else
				{
					if (!Methods.TryGetValue(methodDef, out methodAmendment))
						return base.Mutate(methodDef);

					methodDef = AddAttributes<MethodDefinition>(methodDef, methodAmendment);
				}

				return base.Mutate(methodDef);
			}

			return methodDef;
		}

		/// <summary>
		/// Performs mutation on method bodies to amend the IL as requested for properties, methods and constructors.
		/// </summary>
		/// <param name="methodBody"></param>
		/// <returns></returns>
		public override MethodBody Mutate(MethodBody methodBody)
		{
			// Get the method and type definitions
			IMethodDefinition methodDef = methodBody.MethodDefinition;
			var typeDef = GetCurrentType();

			// Static methods are not supported
			if (methodDef.IsStatic)
				return methodBody;

			// Get the name of the interface and method
			int index = methodDef.Name.Value.LastIndexOf('.');
			string interfaceName = index < 0 ? "" : methodDef.Name.Value.Substring(0, index + 1);
			string methodName = index < 0 ? methodDef.Name.Value : methodDef.Name.Value.Substring(index + 1);

			// Constructors
			if (methodDef.IsConstructor)
			{
				// Get the constructor amendment
				IConstructorAmendment constructorAmendment;

				// Exit immediately if the constructor is not being amended
				if (!Constructors.TryGetValue(methodDef, out constructorAmendment) && PropertyInitializers.Count == 0)
					return methodBody;

				// Amends the constructor
				AmendConstructor(constructorAmendment, methodBody);
			}

			// Properties
			else if (methodDef.IsHiddenBySignature && methodDef.IsSpecialName && (methodName.StartsWith("get_") || methodName.StartsWith("set_")))
			{
				// Determine which property is being mutated
				IPropertyDefinition propertyDef = typeDef.Properties == null ? null : typeDef.Properties
					.Where(p => p.Name.Value == interfaceName + methodName.Substring(4))
					.FirstOrDefault();

				// Ignore methods that are generated to implement properties defined on base types
				if (propertyDef == null && interfaceName != "")
					return methodBody;

				// Get the property Amendment
				IPropertyAmendment propertyAmendment;

				// Exit immediately if the property is not being amended
				if (!Properties.TryGetValue(propertyDef, out propertyAmendment))
					return methodBody;

				// Amend the property
				AmendProperty(propertyDef, propertyAmendment, methodBody);
			}

			// Events
			else if (methodDef.IsHiddenBySignature && methodDef.IsSpecialName && (methodName.StartsWith("add_") || methodName.StartsWith("remove_")))
			{
				// Determine which event is being mutated
				IEventDefinition eventDef = typeDef.Events == null ? null : typeDef.Events
					.Where(e => e.Name.Value == interfaceName + methodName.Substring(methodName.StartsWith("a") ? 4 : 7))
					.FirstOrDefault();

				// Ignore methods that are generated to implement events defined on base types
				if (eventDef == null && interfaceName != "")
					return methodBody;

				// Get the event amendment
				IEventAmendment eventAmendment;

				// Exit immediately if the event is not being amended
				if (!Events.TryGetValue(eventDef, out eventAmendment))
					return methodBody;

				// Amend the event
				AmendEvent(eventDef, eventAmendment, methodBody);
			}

			// Methods
			else
			{
				// Get the method Amendment
				IMethodAmendment methodAmendment;

				// Exit immediately if the method is not being amended
				if (!Methods.TryGetValue(methodDef, out methodAmendment))
					return base.Mutate(methodBody);

				// Amends the method
				AmendMethod(methodAmendment, methodBody);
			}

			return methodBody;
		}

		/// <summary>
		/// Amends the constructor body with the specified constructor amendment.
		/// </summary>
		/// <param name="constructorAmendment"></param>
		/// <param name="methodBody"></param>
		void AmendConstructor(IConstructorAmendment constructorAmendment, MethodBody methodBody)
		{
			// Create an IL generator to amend the operations
			var il = new ILAmender(host, methodBody);

			// Determine if the constructor is being specifically amended or if there are just property initializers
			if (constructorAmendment != null)
			{
				// Before 
				LocalDefinition context = null;
				if (constructorAmendment.Before != null)
				{
					var indexOfFirstNew = 0;
					foreach (var op in il.Operations)
					{						
						if (op.OperationCode == OperationCode.Call )
							break;
						indexOfFirstNew++;
					}
					il.EmitOperations(indexOfFirstNew+1);
						
					
					CallMethodDelegate(methodBody, constructorAmendment.Before, true, il, null, MethodDelegateType.Before, null, null);

					// Track the result of context-based method amendments
					if (constructorAmendment.Before.ReturnType != typeof(void))
					{
						context = new LocalDefinition() { Name = host.NameTable.GetNameFor("_ctx_"), Type = ResolveType(constructorAmendment.Before.ReturnType) };
						if (methodBody.LocalVariables == null)
							methodBody.LocalVariables = new List<ILocalDefinition>();
						methodBody.LocalVariables.Add(context);
						il.Emit(OperationCode.Stloc, context);
					}
				}

				// Implementation
				if (constructorAmendment.Implementation != null)
				{
					// Clear the original method body
					il.Operations.Clear();
					CallMethodDelegate(methodBody, constructorAmendment.Implementation, false, il, null, MethodDelegateType.Implement, null, null);
				}

				// Emit the original method operations if the method implementation was not overriden
				if (constructorAmendment.ConstructorInfo != null && constructorAmendment.Implementation == null)
					il.EmitUntilReturn();

				// After 
				if (constructorAmendment.After != null)
				{
					il.GoToEndOfOperations();

					CallMethodDelegate(methodBody, constructorAmendment.After, false, il, null, MethodDelegateType.After, context, null);
				}
			}
			else
				il.EmitUntilReturn();

			// Emit property initializers if the current constructor calls a base constructor or inherits from System.Object
			var typeDef = GetCurrentType();
			if (typeDef.BaseClasses != null && typeDef.BaseClasses.Any(b => TypeHelper.TypesAreEquivalent(b, host.PlatformType.SystemObject)) ||
				(methodBody.Operations != null && methodBody.Operations.Any(o => o.OperationCode == OperationCode.Call && ((IMethodReference)o.Value).ResolvedMethod.IsConstructor && ((IMethodReference)o.Value).ResolvedMethod.ContainingType == typeDef)))
			{
				foreach (var initializer in PropertyInitializers)
				{
					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load this pointer onto stack again
					il.Emit(OperationCode.Ldarg_0);

					// Load property name onto stack
					il.Emit(OperationCode.Ldstr, initializer.Key.Name.Value);

					// Call LazyInitializer delegate
					il.Emit(OperationCode.Call, initializer.Value.Instance);

					// Set the property
					il.Emit(initializer.Key.Setter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call, initializer.Key.Setter);
				}
			}

			// Emit a return statement is no more operations are present
			if (il.Operations.Count == 0)
				il.Emit(OperationCode.Ret);

			// Update the method body
			il.UpdateMethodBody(6);
		}

		/// <summary>
		/// Amends a property definition using the specified property amendment.
		/// </summary>
		/// <param name="propertyDef"></param>
		/// <param name="propertyAmendment"></param>
		/// <param name="methodBody"></param>
		void AmendProperty(IPropertyDefinition propertyDef, IPropertyAmendment propertyAmendment, MethodBody methodBody)
		{
			// Create an IL generator to amend the operations
			var il = new ILAmender(host, methodBody);

			// Property Get
			if (methodBody.MethodDefinition.Name.Value.StartsWith("get_") || methodBody.MethodDefinition.Name.Value.Contains(".get_"))
			{
				// Getter for new or existing property
				if (propertyAmendment.Getter != null)
				{
					// Clear the original method body
					il.Operations.Clear();

					// Get the delegate to call 
					var getter = ResolvePropertyDelegate(propertyDef, propertyAmendment.Getter);

					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load property name onto stack
					il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

					// Call Getter delegate
					il.Emit(OperationCode.Call, getter.Instance);

					// Return
					il.Emit(OperationCode.Ret);
				}

				// New property without defined getter
				else if (propertyAmendment.PropertyInfo == null)
				{
					// Get the previously created backing field for this property
					var backingField = GetCurrentType().Fields.Where(f => f.Name.Value == GetBackingFieldName(propertyAmendment)).First();

					// Default getter
					if (propertyAmendment.LazyInitializer == null)
					{
						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load the backing field
						il.Emit(OperationCode.Ldfld, backingField);

						// Return
						il.Emit(OperationCode.Ret);
					}

					// Lazy initialized getter
					else
					{
						// Get the delegate to call before the property getter
						var lazyInitializer = ResolvePropertyDelegate(propertyDef, propertyAmendment.LazyInitializer);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load the backing field
						il.Emit(OperationCode.Ldfld, backingField);

						// Skip initialization if the backing field is already initialized
						var skipIfNotNull = new ILGeneratorLabel();
						il.Emit(OperationCode.Brtrue_S, skipIfNotNull);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load this pointer onto stack again
						il.Emit(OperationCode.Ldarg_0);

						// Load property name onto stack
						il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

						// Call LazyInitializer delegate
						il.Emit(OperationCode.Call, lazyInitializer.Instance);

						// Update the backing field
						il.Emit(OperationCode.Stfld, backingField);

						// End the skipping branch
						il.MarkLabel(skipIfNotNull);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load the backing field
						il.Emit(OperationCode.Ldfld, backingField);

						// Return
						il.Emit(OperationCode.Ret);
					}
				}

				// Amend the existing property
				else
				{
					// Before Get
					if (propertyAmendment.BeforeGet != null)
					{
						// Get the delegate to call before the property getter
						var beforeGet = ResolvePropertyDelegate(propertyDef, propertyAmendment.BeforeGet);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load property name onto stack
						il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

						// Call BeforeGet delegate
						il.Emit(OperationCode.Call, beforeGet.Instance);
					}

					// Lazy Initializer and/or After Get
					if (propertyAmendment.LazyInitializer != null || propertyAmendment.AfterGet != null)
					{
						// After Get
						if (propertyAmendment.AfterGet != null)
						{
							// Load this pointer onto stack
							il.Emit(OperationCode.Ldarg_0);

							// Load property name onto stack
							il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);
						}

						// Emit the original property getter instructions
						il.EmitUntilReturn();

						// Lazy Initializer
						if (propertyAmendment.LazyInitializer != null)
						{
							// Add local variable to store the current value of the property
							var currentValue = new LocalDefinition() { Name = host.NameTable.GetNameFor("_cv_"), Type = propertyDef.Type };

							if (methodBody.LocalVariables == null)
								methodBody.LocalVariables = new List<ILocalDefinition>();

							methodBody.LocalVariables.Add(currentValue);

							// Store the current value in a local variable
							il.Emit(OperationCode.Stloc, currentValue);

							// Load the current value
							il.Emit(OperationCode.Ldloc, currentValue);

							// Skip initialization if the backing field is already initialized
							var skipIfNotNull = new ILGeneratorLabel();
							il.Emit(OperationCode.Brtrue_S, skipIfNotNull);

							// Load this pointer onto stack
							il.Emit(OperationCode.Ldarg_0);

							// Load this pointer onto stack again
							il.Emit(OperationCode.Ldarg_0);

							// Load property name onto stack
							il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

							// Call LazyInitializer delegate
							il.Emit(OperationCode.Call, ResolvePropertyDelegate(propertyDef, propertyAmendment.LazyInitializer).Instance);

							// Store the initialized value
							il.Emit(OperationCode.Stloc, currentValue);

							// Load the initialized value
							il.Emit(OperationCode.Ldloc, currentValue);

							// Call the property setter
							il.Emit(propertyDef.Setter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call, propertyDef.Setter);

							// End the skipping branch
							il.MarkLabel(skipIfNotNull);

							// Load the current value
							il.Emit(OperationCode.Ldloc, currentValue);
						}

						// Call AfterGet delegate
						if (propertyAmendment.AfterGet != null)
							il.Emit(OperationCode.Call, ResolvePropertyDelegate(propertyDef, propertyAmendment.AfterGet).Instance);
					}

					// Emit the rest of the original method if not amending after get
					else
						il.EmitUntilReturn();
				}
			}

			// Property Set
			else
			{
				// Setter for new or existing property
				if (propertyAmendment.Setter != null)
				{
					// Clear the original method body
					il.Operations.Clear();

					// Get the delegate to call 
					var setter = ResolvePropertyDelegate(propertyDef, propertyAmendment.Setter);

					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load property name onto stack
					il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

					// Load value argument onto stack
					il.Emit(OperationCode.Ldarg_1);

					// Call Setter delegate
					il.Emit(OperationCode.Call, setter.Instance);

					// Return
					il.Emit(OperationCode.Ret);
				}

				// New property without setter
				else if (propertyAmendment.PropertyInfo == null)
				{
					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load value argument onto stack
					il.Emit(OperationCode.Ldarg_1);

					// Save to backing field
					il.Emit(OperationCode.Stfld, GetCurrentType().Fields.Where(f => f.Name.Value == GetBackingFieldName(propertyAmendment)).First());

					// Return
					il.Emit(OperationCode.Ret);
				}

				// Amend the existing property setter
				else if (propertyAmendment.BeforeSet != null || propertyAmendment.AfterSet != null)
				{
					// Get the before and after set delegate implementations
					var beforeSet = propertyAmendment.BeforeSet != null ? ResolvePropertyDelegate(propertyDef, propertyAmendment.BeforeSet) : null;
					var afterSet = propertyAmendment.AfterSet != null ? ResolvePropertyDelegate(propertyDef, propertyAmendment.AfterSet) : null;

					// Determine if the old value is used by the amended delegates
					bool useOriginalValue = beforeSet.ReferencesArgument(2) || afterSet.ReferencesArgument(2);

					// Emit a local variable and store the initial property value by calling the property getter
					if (useOriginalValue)
					{
						// Add local variable to store the old value of the property
						if (methodBody.LocalVariables == null)
							methodBody.LocalVariables = new List<ILocalDefinition>();
						methodBody.LocalVariables.Add(new LocalDefinition() { Name = host.NameTable.GetNameFor("_ov_"), Type = propertyDef.Type });

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Call the property getter to get the old value
						il.Emit(propertyDef.Getter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call, propertyDef.Getter.ResolvedMethod);

						// Store the old value in a local variable
						il.Emit(OperationCode.Stloc, methodBody.LocalVariables.Last());
					}

					// Before Set
					if (beforeSet != null)
					{
						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load property name onto stack
						il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

						// Optionally load the old value onto stack
						if (useOriginalValue)
							il.Emit(OperationCode.Ldloc, methodBody.LocalVariables.Last());
						else
							il.Emit(OperationCode.Ldarg_1);

						// Load value argument pointer
						il.Emit(OperationCode.Ldarg_1);

						// Call BeforeSet delegate
						il.Emit(OperationCode.Call, beforeSet.Instance);

						// Store the result as the new value for the setter
						il.Emit(OperationCode.Starg, methodBody.MethodDefinition.Parameters.First());
					}

					// Emit the old property setter instructions
					il.EmitUntilReturn();

					// After Set
					if (afterSet != null)
					{
						il.GoToEndOfOperations();

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load property name onto stack
						il.Emit(OperationCode.Ldstr, propertyDef.Name.Value);

						// Optionally load the old value onto stack
						if (useOriginalValue)
							il.Emit(OperationCode.Ldloc, methodBody.LocalVariables.Last());
						else
							il.Emit(OperationCode.Ldarg_1);

						// Load value argument onto stack
						il.Emit(OperationCode.Ldarg_1);

						// Optionally get the new property value if used
						if (afterSet.ReferencesArgument(4))
						{
							// Load this pointer onto stack
							il.Emit(OperationCode.Ldarg_0);

							// Call property getter
							il.Emit(propertyDef.Getter.ResolvedMethod.IsVirtual ? OperationCode.Callvirt : OperationCode.Call, propertyDef.Getter.ResolvedMethod);
						}
						else
							il.Emit(OperationCode.Ldarg_1);

						// Call AfterSet delegate
						il.Emit(OperationCode.Call, afterSet.Instance);

						il.Emit(OperationCode.Ret);
					}
				}
			}

			// Update the method body
			il.UpdateMethodBody(6);
		}

		/// <summary>
		/// Amends the method body with the specified method amendment.
		/// </summary>
		/// <param name="methodAmendment"></param>
		/// <param name="methodBody"></param>
		void AmendMethod(IMethodAmendment methodAmendment, MethodBody methodBody)
		{

			/**
			 * The method implementation is as follows
			 * Try for finally block if exists
			 * Try for catch block if exists
			 * catch block if exists
			 * Finally block if exits
			 * 
			 * */

			bool hasReturnValue = !TypeHelper.TypesAreEquivalent(methodBody.MethodDefinition.Type, host.PlatformType.SystemVoid);
			bool isRetLastStatement =methodBody.Operations!= null && methodBody.Operations.Count() > 0 && methodBody.Operations.Last().OperationCode == OperationCode.Ret;
			// Create an IL generator to amend the operations
			var il = new ILAmender(host, methodBody);
			// Implementation
			Action implement = () =>
			{
				ImplementMethod(methodAmendment, methodBody, il);
			};

			/**
			 * The entities under modifications are the methodBody,  Operations variable in the amender.
			 * The emit method is adding operations to the method operations.
			 * */
			

			// Before Method
			LocalDefinition context = null;
			if (methodAmendment.Before != null)
			{
				CallMethodDelegate(methodBody, methodAmendment.Before, true, il, null, MethodDelegateType.Before, null, null);

				// Track the result of context-based method amendments
				if (methodAmendment.Before.ReturnType != typeof(void))
				{
					context = new LocalDefinition() { Name = host.NameTable.GetNameFor("_ctx_"), Type = ResolveType(methodAmendment.Before.ReturnType) };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>(); 
					methodBody.LocalVariables.Add(context);
					il.Emit(OperationCode.Stloc, context);
				}
			}

			// Optionally start try catch finally block
			if (methodAmendment.Finally != null)
			{
				il.BeginTryBody();
			}
			if (methodAmendment.Catch != null)
			{
				il.BeginTryBody();				
			}
						
			// Implement
			if (methodAmendment.After == null || methodAmendment.After.ReturnType == typeof(void))
			{
				implement();
				implement = null;
			}

			// After Method
			if (methodAmendment.After != null)
			{
				il.GoToEndOfOperations();

				CallMethodDelegate(methodBody, methodAmendment.After, false, il, implement, MethodDelegateType.After, context, null);
				//if (methodAmendment.Catch == null && methodAmendment.Finally == null)
				//{					
				//	il.Emit(OperationCode.Ret);					
				//}
				//else
				//{

				//}
			}

			// Emit the remaining operations
			il.EmitUntilReturn();

			// Catch/Finally
			if (methodAmendment.Catch != null || methodAmendment.Finally != null)
			{
				
				ILocalDefinition result = null;

				// Store the result before starting the catch/finally blocks
				if (hasReturnValue)
				{
					result = new LocalDefinition() { Name = host.NameTable.GetNameFor("_result_"), Type = methodBody.MethodDefinition.Type };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>();
					methodBody.LocalVariables.Add(result);
					il.Emit(OperationCode.Stloc, result);
				}
				

				// Skip to the return statement
				var exit = new ILGeneratorLabel();
				il.Emit(OperationCode.Leave_S, exit);

				// Catch
				if (methodAmendment.Catch != null)
				{
					// Determine the type of delegate
					var delegateType = MethodDelegateType.Catch;
					if (context != null)
						delegateType |= MethodDelegateType.WithContext;
					ResolveMethodDelegate(methodBody.MethodDefinition.ContainingType, methodAmendment.Catch, methodBody.MethodDefinition, ref delegateType);

					// Determine the type of exception being caught
					var exceptionType = ResolveType(methodAmendment.Catch.GetParameters()[(delegateType.HasFlag(MethodDelegateType.ArraySyntax) ? 1 : 0) + (context != null ? 2 : 1)].ParameterType);

					// Start the catch block
					il.BeginCatchBlock(exceptionType);

					// Store exception in local variable
					var exception = new LocalDefinition() { Name = host.NameTable.GetNameFor("_exception_"), Type = exceptionType };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>(); 
					methodBody.LocalVariables.Add(exception);
					il.Emit(OperationCode.Stloc, exception);

					// Call the catch delegate
					CallMethodDelegate(methodBody, methodAmendment.Catch, false, il, null, MethodDelegateType.Catch, context, exception);

					// Store the new result if the method has a return value
					if (hasReturnValue)
						il.Emit(OperationCode.Stloc, result);

					// Exit the catch block
					il.Emit(OperationCode.Leave_S, exit);

					// End nested try block if necessary
					if (methodAmendment.Finally != null)
						il.EndTryBody();
				}

				// Finally
				if (methodAmendment.Finally != null)
				{
					// Start the finally block
					il.BeginFinallyBlock();

					// Call the finally delegate
					CallMethodDelegate(methodBody, methodAmendment.Finally, false, il, null, MethodDelegateType.Finally, context, null);

					// End the finally block
					il.Emit(OperationCode.Endfinally);
				}

				// End the try clause
				il.MarkLabel(exit);
				il.EndTryBody();

				// Load the result
				if (hasReturnValue)
				{
					il.Emit(OperationCode.Ldloc, result);
                    
				}
				
			}

			// Emit a return for new/overriden methods
			if (methodAmendment.Implementation != null || methodAmendment.Overrides != null || methodAmendment.Raises != null)
				//|| methodAmendment.Catch != null || methodAmendment.Finally != null)
			{
				il.Emit(OperationCode.Ret);
			}
			//if the catch or finally block is implemented, the method body will change, and even if it doesn't have a ret cmd it should have now
			// for methods that doesn't have a return type, it is normal that they don't have ret
			// some methods end with throw statement instead of ret
			else if (methodAmendment.Catch != null || methodAmendment.Finally != null)
			{
				if (!isRetLastStatement)
				{
					il.Emit(OperationCode.Ret);
				}
			}
            //else if (il.EmittedOperations.Count == 0)
            //{
            //    il.Emit(OperationCode.Ret);
            //}
            //else if ( il.EmittedOperations.Last().OperationCode != OperationCode.Ret)
            //{
            //    if (il.EmittedOperations.Last().OperationCode != OperationCode.Ret)
            //        il.Emit(OperationCode.Ret);
            //}
            //else
            //{
            //}

			// Update the method body
			il.UpdateMethodBody(6);

           

           
        }


        void AmendMethod2(IMethodAmendment methodAmendment, MethodBody methodBody)
		{
			bool hasReturnValue = !TypeHelper.TypesAreEquivalent(methodBody.MethodDefinition.Type, host.PlatformType.SystemVoid);
			// Create an IL generator to amend the operations
			var il = new ILAmender(host, methodBody);
			// Implementation
			Action implement = () =>
			{
				ImplementMethod(methodAmendment, methodBody, il);
			};

			// Before Method
			LocalDefinition context = null;
			if (methodAmendment.Before != null)
			{
				CallMethodDelegate(methodBody, methodAmendment.Before, true, il, null, MethodDelegateType.Before, null, null);

				// Track the result of context-based method amendments
				if (methodAmendment.Before.ReturnType != typeof(void))
				{
					context = new LocalDefinition() { Name = host.NameTable.GetNameFor("_ctx_"), Type = ResolveType(methodAmendment.Before.ReturnType) };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>();
					methodBody.LocalVariables.Add(context);
					il.Emit(OperationCode.Stloc, context);
				}
			}

			// Optionally start try catch finally block
			if (methodAmendment.Finally != null)
			{
				il.BeginTryBody();
			}
			if (methodAmendment.Catch != null)
			{
				il.BeginTryBody();
			}

			// Implement
			if (methodAmendment.After == null || methodAmendment.After.ReturnType == typeof(void))
			{
				implement();
				implement = null;
			}

			// After Method
			if (methodAmendment.After != null)
			{
				il.GoToEndOfOperations();

				CallMethodDelegate(methodBody, methodAmendment.After, false, il, implement, MethodDelegateType.After, context, null);
				if (methodAmendment.Catch == null && methodAmendment.Finally == null)
				{
					il.Emit(OperationCode.Ret);
				}
				else
				{

				}
			}

			// Emit the remaining operations
			il.EmitUntilReturn();

			// Catch/Finally
			if (methodAmendment.Catch != null || methodAmendment.Finally != null)
			{

				ILocalDefinition result = null;

				// Store the result before starting the catch/finally blocks
				if (hasReturnValue)
				{
					result = new LocalDefinition() { Name = host.NameTable.GetNameFor("_result_"), Type = methodBody.MethodDefinition.Type };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>();
					methodBody.LocalVariables.Add(result);
					il.Emit(OperationCode.Stloc, result);
				}


				// Skip to the return statement
				var exit = new ILGeneratorLabel();
				il.Emit(OperationCode.Leave_S, exit);

				// Catch
				if (methodAmendment.Catch != null)
				{
					// Determine the type of delegate
					var delegateType = MethodDelegateType.Catch;
					if (context != null)
						delegateType |= MethodDelegateType.WithContext;
					ResolveMethodDelegate(methodBody.MethodDefinition.ContainingType, methodAmendment.Catch, methodBody.MethodDefinition, ref delegateType);

					// Determine the type of exception being caught
					var exceptionType = ResolveType(methodAmendment.Catch.GetParameters()[(delegateType.HasFlag(MethodDelegateType.ArraySyntax) ? 1 : 0) + (context != null ? 2 : 1)].ParameterType);

					// Start the catch block
					il.BeginCatchBlock(exceptionType);

					// Store exception in local variable
					var exception = new LocalDefinition() { Name = host.NameTable.GetNameFor("_exception_"), Type = exceptionType };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>();
					methodBody.LocalVariables.Add(exception);
					il.Emit(OperationCode.Stloc, exception);

					// Call the catch delegate
					CallMethodDelegate(methodBody, methodAmendment.Catch, false, il, null, MethodDelegateType.Catch, context, exception);

					// Store the new result if the method has a return value
					if (hasReturnValue)
						il.Emit(OperationCode.Stloc, result);

					// Exit the catch block
					il.Emit(OperationCode.Leave_S, exit);

					// End nested try block if necessary
					if (methodAmendment.Finally != null)
						il.EndTryBody();
				}

				// Finally
				if (methodAmendment.Finally != null)
				{
					// Start the finally block
					il.BeginFinallyBlock();

					// Call the finally delegate
					CallMethodDelegate(methodBody, methodAmendment.Finally, false, il, null, MethodDelegateType.Finally, context, null);

					// End the finally block
					il.Emit(OperationCode.Endfinally);
				}

				// End the try clause
				il.MarkLabel(exit);
				il.EndTryBody();

				// Load the result
				if (hasReturnValue)
					il.Emit(OperationCode.Ldloc, result);
			}

			// Emit a return for new/overriden methods
			if (methodAmendment.Implementation != null || methodAmendment.Overrides != null || methodAmendment.Raises != null)
				il.Emit(OperationCode.Ret);

			// Update the method body
			il.UpdateMethodBody(6);
		}

		private void ImplementMethod(IMethodAmendment methodAmendment, MethodBody methodBody, ILAmender il)
		{
			// Emit the implementation if specified
			if (methodAmendment.Implementation != null)
			{
				// Clear the original method body
				il.Operations.Clear();
				CallMethodDelegate(methodBody, methodAmendment.Implementation, false, il, null, MethodDelegateType.Implement, null, null);
			}

			// Emit a call to the base method if overriding and an implementation was not specified
			else if (methodAmendment.Overrides != null)
			{
				// Load this pointer onto stack
				il.Emit(OperationCode.Ldarg_0);

				// Load method arguments onto the stack
				foreach (var arg in methodBody.MethodDefinition.Parameters)
					il.Emit(OperationCode.Ldarg_S, arg);

				// Call the base method
				il.Emit(OperationCode.Call, ResolveMethod(ResolveType(methodAmendment.Overrides.DeclaringType), methodAmendment.Overrides));
			}

			// Emit calls to raise the specified event
			else if (methodAmendment.Raises != null)
			{
				// Get the event raise arguments
				var invokeMethod = methodAmendment.Raises.Type.GetMethod("Invoke");
				var thisFirstParam = invokeMethod.GetParameters()[0].ParameterType == typeof(object);
				var eventArgsSecondParam = invokeMethod.GetParameters()[1].ParameterType == typeof(EventArgs);
				var args = invokeMethod.GetParameters()
					.SkipWhile((p, i) => (i == 0 && thisFirstParam) || (i == 1 && eventArgsSecondParam))
					.Select(p => p.ParameterType).ToArray();
				var backingField = GetCurrentType().Fields.Single(f => f.Name.Value == methodAmendment.Raises.Name);

				// Load this pointer onto stack
				il.Emit(OperationCode.Ldarg_0);

				// Load event handler backing field onto stack
				il.Emit(OperationCode.Ldfld, backingField);

				// Load null onto stack
				il.Emit(OperationCode.Ldnull);

				// See if the field is null
				il.Emit(OperationCode.Ceq);

				// Create a branching label
				var ifRaise = new ILGeneratorLabel();
				il.Emit(OperationCode.Brtrue_S, ifRaise);

				// Load this pointer onto stack
				il.Emit(OperationCode.Ldarg_0);

				// Load event handler backing field onto stack
				il.Emit(OperationCode.Ldfld, backingField);

				// Load this pointer onto stack if required
				if (thisFirstParam)
					il.Emit(OperationCode.Ldarg_0);

				// Create event args if required
				if (eventArgsSecondParam)
					il.Emit(OperationCode.Newobj, ResolveConstructor(ResolveType(typeof(EventArgs)), typeof(EventArgs).GetConstructor(Type.EmptyTypes)));

				// Load all method arguments
				foreach (var arg in methodBody.MethodDefinition.Parameters)
					il.Emit(OperationCode.Ldarg_S, arg);

				// Invoke the delegate
				il.Emit(OperationCode.Callvirt, ResolveMethod(ResolveType(invokeMethod.DeclaringType), invokeMethod));

				// Finish the raise conditional block
				il.MarkLabel(ifRaise);
				il.Emit(OperationCode.Nop);
			}

			// Emit the original method operations if the method implementation was not overriden
			if (methodAmendment.MethodInfo != null && methodAmendment.Implementation == null)
				il.EmitUntilReturn();
		}

		/// <summary>
		/// Amends an event definition using the specified event amendment.
		/// </summary>
		/// <param name="eventDef"></param>
		/// <param name="eventAmendment"></param>
		/// <param name="methodBody"></param>
		void AmendEvent(IEventDefinition eventDef, IEventAmendment eventAmendment, MethodBody methodBody)
		{
			// Create an IL generator to amend the operations
			var il = new ILAmender(host, methodBody);

			// Event Get
			if (methodBody.MethodDefinition.Name.Value.StartsWith("add_") || methodBody.MethodDefinition.Name.Value.Contains(".add_"))
			{
				// Adder for new or existing event
				if (eventAmendment.Adder != null)
				{
					// Clear the original method body
					il.Operations.Clear();

					// Get the delegate to call 
					var getter = ResolveEventDelegate(eventDef, eventAmendment.Adder);

					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load event name onto stack
					il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

					// Load value argument onto stack
					il.Emit(OperationCode.Ldarg_1);

					// Call Adder delegate
					il.Emit(OperationCode.Call, getter.Instance);

					// Return
					il.Emit(OperationCode.Ret);
				}

				// New event without defined adder
				else if (eventAmendment.EventInfo == null)
					EmitDefaultEvent(eventDef, eventAmendment, methodBody, il, true);

				// Amend the existing event adder
				else
				{
					// Before Add
					if (eventAmendment.BeforeAdd != null)
					{
						// Get the delegate to call before the event adder
						var beforeAdd = ResolveEventDelegate(eventDef, eventAmendment.BeforeAdd);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load event name onto stack
						il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

						// Load value argument onto stack
						il.Emit(OperationCode.Ldarg_1);

						// Call BeforeAdd delegate
						il.Emit(OperationCode.Call, beforeAdd.Instance);
					}

					// Emit the original event adder instructions
					il.EmitUntilReturn();

					// After Add
					if (eventAmendment.AfterAdd != null)
					{
						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load event name onto stack
						il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

						// Load value argument onto stack
						il.Emit(OperationCode.Ldarg_1);

						// Call AfterAdd delegate
						il.Emit(OperationCode.Call, ResolveEventDelegate(eventDef, eventAmendment.AfterAdd).Instance);
					}
				}
			}

			// Event Remove
			else
			{
				// Remover for new or existing event
				if (eventAmendment.Remover != null)
				{
					// Clear the original method body
					il.Operations.Clear();

					// Get the delegate to call 
					var remover = ResolveEventDelegate(eventDef, eventAmendment.Remover);

					// Load this pointer onto stack
					il.Emit(OperationCode.Ldarg_0);

					// Load event name onto stack
					il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

					// Load value argument onto stack
					il.Emit(OperationCode.Ldarg_1);

					// Call Remover delegate
					il.Emit(OperationCode.Call, remover.Instance);

					// Return
					il.Emit(OperationCode.Ret);
				}

				// New event without setter
				else if (eventAmendment.EventInfo == null)
					EmitDefaultEvent(eventDef, eventAmendment, methodBody, il, false);

				// Amend the existing event remover
				else
				{
					// Before Remove
					if (eventAmendment.BeforeRemove != null)
					{
						// Get the delegate to call 
						var beforeRemove = ResolveEventDelegate(eventDef, eventAmendment.BeforeRemove);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load event name onto stack
						il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

						// Load value argument pointer
						il.Emit(OperationCode.Ldarg_1);

						// Call BeforeRemove delegate
						il.Emit(OperationCode.Call, beforeRemove.Instance);
					}

					// Emit the old event setter instructions
					il.EmitUntilReturn();

					// After Remove
					if (eventAmendment.AfterRemove != null)
					{
						// Get the delegate to call 
						var afterRemove = ResolveEventDelegate(eventDef, eventAmendment.AfterRemove);

						// Load this pointer onto stack
						il.Emit(OperationCode.Ldarg_0);

						// Load event name onto stack
						il.Emit(OperationCode.Ldstr, eventDef.Name.Value);

						// Load value argument onto stack
						il.Emit(OperationCode.Ldarg_1);

						// Call AfterRemove delegate
						il.Emit(OperationCode.Call, afterRemove.Instance);
					}
				}
			}

			// Update the method body
			il.UpdateMethodBody(6);
		}

		/// <summary>
		/// Emits the IL implementation for a default event adder or remover following the approach used by the C# compiler.
		/// </summary>
		/// <param name="eventDef"></param>
		/// <param name="eventAmendment"></param>
		/// <param name="methodBody"></param>
		/// <param name="il"></param>
		/// <param name="isAdder"></param>
		void EmitDefaultEvent(IEventDefinition eventDef, IEventAmendment eventAmendment, MethodBody methodBody, ILAmender il, bool isAdder)
		{
			// Get the previously created backing field for this event
			var backingField = GetCurrentType().Fields.Where(f => f.Name.Value == eventAmendment.Name).First();

			// Create local variables to track the event handler state
			if (methodBody.LocalVariables == null)
				methodBody.LocalVariables = new List<ILocalDefinition>();
			methodBody.LocalVariables.Add(new LocalDefinition() { Name = host.NameTable.GetNameFor("_h0_"), Type = eventDef.Type });
			methodBody.LocalVariables.Add(new LocalDefinition() { Name = host.NameTable.GetNameFor("_h1_"), Type = eventDef.Type });
			methodBody.LocalVariables.Add(new LocalDefinition() { Name = host.NameTable.GetNameFor("_h2_"), Type = eventDef.Type });
			methodBody.LocalVariables.Add(new LocalDefinition() { Name = host.NameTable.GetNameFor("_flag_"), Type = host.PlatformType.SystemBoolean });

			// Load this pointer onto stack
			il.Emit(OperationCode.Ldarg_0);

			// Load the backing field
			il.Emit(OperationCode.Ldfld, backingField);

			// Store the value in the first local variable
			il.Emit(OperationCode.Stloc_0);

			// Begin a loop to perform the change
			var loop = new ILGeneratorLabel();
			il.MarkLabel(loop);

			// Load the value of the first local variable
			il.Emit(OperationCode.Ldloc_0);

			// Store the value in the second local variable
			il.Emit(OperationCode.Stloc_1);

			// Load the value of the second local variable
			il.Emit(OperationCode.Ldloc_1);

			// Load the event value argument
			il.Emit(OperationCode.Ldarg_1);

			// Either call System.Delegate.Combine(System.Delegate, System.Delegate) or System.Delegate.Remove(System.Delegate, System.Delegate)
			il.Emit(OperationCode.Call, ResolveMethod(
				ResolveType(typeof(System.Delegate)),
				typeof(System.Delegate).GetMethod(isAdder ? "Combine" : "Remove", new Type[] { typeof(Delegate), typeof(Delegate) })));

			// Cast to the correct delegate type
			il.Emit(OperationCode.Castclass, eventDef.Type);

			// Store the value in the third local variable
			il.Emit(OperationCode.Stloc_2);

			// Load this pointer onto stack
			il.Emit(OperationCode.Ldarg_0);

			// Load the backing field address
			il.Emit(OperationCode.Ldflda, backingField);

			// Load the value of the third local variable
			il.Emit(OperationCode.Ldloc_2);

			// Load the value of the second local variable
			il.Emit(OperationCode.Ldloc_1);

			// Call System.Threading.Interlocked.CompareExchange<TEvent>(ref TEvent, TEvent, TEvent)
			il.Emit(OperationCode.Call,
				new GenericMethodInstance(
					ResolveType(typeof(System.Threading.Interlocked)).Methods.Where(m => m.Name.Value == "CompareExchange" && m.IsGeneric && m.GenericParameterCount == 1 && m.ParameterCount == 3).First(),
					new ITypeReference[] { eventDef.Type },
					host.InternFactory));

			// Store the value in the first local variable
			il.Emit(OperationCode.Stloc_0);

			// Load the value of the first local variable
			il.Emit(OperationCode.Ldloc_0);

			// Load the value of the second local variable
			il.Emit(OperationCode.Ldloc_1);

			// Compare the event handlers
			il.Emit(OperationCode.Ceq);

			// Load 0 onto the stack
			il.Emit(OperationCode.Ldc_I4_0);

			// Compare equality of the result to get a boolean value
			il.Emit(OperationCode.Ceq);

			// Store the value in the fourth local variable
			il.Emit(OperationCode.Stloc_3);

			// Load the value of the fourth local variable
			il.Emit(OperationCode.Ldloc_3);

			// Continue looping until the operation has completed
			il.Emit(OperationCode.Brtrue_S, loop);

			// Return
			il.Emit(OperationCode.Ret);
		}

		/// <summary>
		/// Calls the specified method delegate.
		/// </summary>
		/// <param name="methodBody"></param>
		/// <param name="method"></param>
		/// <param name="updateArguments"></param>
		/// <param name="il"></param>
		/// <param name="implement"></param>
		void CallMethodDelegate(MethodBody methodBody, System.Reflection.MethodInfo method, bool updateArguments, ILAmender il, Action implement, MethodDelegateType delegateType, LocalDefinition context, LocalDefinition exception)
		{
			// Get the corresponding before method definition
			var methodDef = methodBody.MethodDefinition;

			// Determine whether this is a contextual method call
			if ((delegateType.HasFlag(MethodDelegateType.Before) && method.ReturnType != typeof(void)) || context != null)
				delegateType |= MethodDelegateType.WithContext;

			// Get the method definition to emit a call to
			var targetMethodDef = ResolveMethodDelegate(methodBody.MethodDefinition.ContainingType, method, methodDef, ref delegateType);

			// Array syntax (T instance, string methodName[, TContext context][, TException exception], object[] parameters[, object result])
			if (delegateType.HasFlag(MethodDelegateType.ArraySyntax))
			{
				// Add local variable to store the return value before delegate
				ITypeDefinition argTypeDef = targetMethodDef.Parameters.Skip(2 + (context != null ? 1 : 0) + (exception != null ? 1 : 0)).First().Type.ResolvedType;
				var args = new LocalDefinition() { Name = host.NameTable.GetNameFor("_args_"), Type = argTypeDef };
				if (methodBody.LocalVariables == null)
					methodBody.LocalVariables = new List<ILocalDefinition>();
				methodBody.LocalVariables.Add(args);

				// Load the number of arguments onto the stack
				il.Emit(OperationCode.Ldc_I4, methodDef.Parameters.Count());

				// Create an object array
				il.Emit(OperationCode.Newarr, argTypeDef);

				// Store the new array
				il.Emit(OperationCode.Stloc, args);

				// Populate the array with method arguments
				foreach (var parameter in methodDef.Parameters)
				{
					// Load the argument array
					il.Emit(OperationCode.Ldloc, args);

					// Add argument index to the stack
					il.Emit(OperationCode.Ldc_I4, parameter.Index);

					// Load the argument
					il.Emit(OperationCode.Ldarg, parameter);

					// Box value types
					if (parameter.Type.IsValueType)
						il.Emit(OperationCode.Box, parameter.Type);

					// Store the argument in the array
					il.Emit(OperationCode.Stelem_Ref);
				}

				// Emit the method implementation here if specified
				if (implement != null)
					implement();

				// Add local variable to store the return value before delegate
				LocalDefinition result = null;
				if (delegateType.HasFlag(MethodDelegateType.HasResultParameter))
				{
					// Create the result local variable
					result = new LocalDefinition() { Name = host.NameTable.GetNameFor("_result_"), Type = methodBody.MethodDefinition.Type };
					if (methodBody.LocalVariables == null)
						methodBody.LocalVariables = new List<ILocalDefinition>();
					methodBody.LocalVariables.Add(result);

					// Store the result
					il.Emit(OperationCode.Stloc, result);
				}

				// Load this pointer onto stack
				il.Emit(OperationCode.Ldarg_0);

				// Load method name onto stack
				il.Emit(OperationCode.Ldstr, methodBody.MethodDefinition.Name.Value);

				// Optionally load the method context
				if (context != null)
					il.Emit(OperationCode.Ldloc, context);

				// Optionally load exception
				if (exception != null)
					il.Emit(OperationCode.Ldloc, exception);

				// Load the argument array
				il.Emit(OperationCode.Ldloc, args);

				// Load the result variable is required for the delegate being invoked
				if (delegateType.HasFlag(MethodDelegateType.HasResultParameter))
				{
					// Load the result
					il.Emit(OperationCode.Ldloc, result);

					// Box result values if necessary
					if (delegateType.HasFlag(MethodDelegateType.HasResultParameter) && methodDef.Type.IsValueType)
						il.Emit(OperationCode.Box, methodDef.Type);
				}

				// Call the target method
				il.Emit(OperationCode.Call, targetMethodDef);

				// Unbox result values if necessary
				if (delegateType.HasFlag(MethodDelegateType.Function) && (delegateType.HasFlag(MethodDelegateType.After) || delegateType.HasFlag(MethodDelegateType.Catch)) && methodDef.Type.IsValueType)
					il.Emit(OperationCode.Unbox_Any, methodDef.Type);

				// Handle method delegates that potentially return updated arguments
				if (updateArguments)
				{
					// Update the method arguments with the result of the invocation
					foreach (var parameter in methodDef.Parameters)
					{
						// Load the arguments back on the stack
						il.Emit(OperationCode.Ldloc, args);

						// Load the index of the argument value
						il.Emit(OperationCode.Ldc_I4, parameter.Index);

						// Get the argument value from the array
						il.Emit(OperationCode.Ldelem_Ref);

						// Unbox value types
						if (parameter.Type.IsValueType)
							il.Emit(OperationCode.Unbox_Any, parameter.Type);

						// Update the actual argument
						il.Emit(OperationCode.Starg, parameter);
					}
				}
			}

			// Explicit syntax
			else
			{
				// Load this pointer onto stack
				il.Emit(OperationCode.Ldarg_0);

				// Optionally load the method context
				if (context != null)
					il.Emit(OperationCode.Ldloc, context);

				// Optionally load exception
				if (exception != null)
					il.Emit(OperationCode.Ldloc, exception);

				// Determine whether to load the argument value or argument references
				var ldArg = methodDef.ParameterCount > 0 && targetMethodDef.Parameters.Last().IsByReference ? OperationCode.Ldarga_S : OperationCode.Ldarg_S;

				// Load the method arguments onto the stack
				foreach (var parameter in methodDef.Parameters)
					il.Emit(ldArg, parameter);

				// Emit the method implementation here if specified
				if (implement != null)
					implement();

				// Call the target method
				il.Emit(OperationCode.Call, targetMethodDef);
			}
		}

		/// <summary>
		/// Get the property with the specified name on the current type or base types.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		IPropertyDefinition GetProperty(ITypeDefinition type, string property)
		{
			var propertyDef = type.Properties.Where(p => p.Name.Value == property).FirstOrDefault();
			if (propertyDef == null && type.BaseClasses.Any())
				return GetProperty(type.BaseClasses.Select(t => t.ResolvedType).FirstOrDefault(), property);
			return propertyDef;
		}

		/// <summary>
		/// Get the <see cref="INamedTypeDefinition"/> corresponding to the specified <see cref="Type"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		ITypeDefinition ResolveType(Type type)
		{
			// Return the cached type definition if available
			ITypeDefinition typeDef;
			if (resolvedTypes.TryGetValue(type, out typeDef))
				return typeDef;

			// Handle arrays
			if (type.IsArray)
			{
				ITypeDefinition elementTypeDef = ResolveType(type.GetElementType());
				typeDef = Vector.GetVector(elementTypeDef, host.InternFactory);
			}

			// Generic types
			else if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				var genericType = type.GetGenericTypeDefinition();
				var genericTypeDef = ResolveType(genericType);

				// Return the generic type
				typeDef = GenericTypeInstance.GetGenericTypeInstance((INamedTypeDefinition)genericTypeDef, type.GetGenericArguments().Select(t => ResolveType(t)).Cast<ITypeReference>(), host.InternFactory);
			}

			// Generic parameter
			else if (type.IsGenericParameter)
			{
				return null;				
			}

			// Otherwise, just find the type
			else
				typeDef = ResolveAssembly(type.Assembly).GetAllTypes()
					.OfType<INamedTypeDefinition>()
					.Where(t => GetTypeName(t) == type.FullName)
					.FirstOrDefault();

			// Cache an return the resolved type definition
			resolvedTypes[type] = typeDef;
			resolvedTypeDefs[typeDef] = type;

			return typeDef;
		}

		IAssembly ResolveAssembly(System.Reflection.Assembly assembly)
		{
			IAssembly assemblyDef = null;
			if (resolvedAssemblies.TryGetValue(assembly, out assemblyDef))
				return assemblyDef;

			if (assembly.FullName.StartsWith("mscorlib") && TargetRuntimeVersion.StartsWith("v2"))
				assemblyDef = host.FindAssembly(host.CoreAssemblySymbolicIdentity);
			else
			{
				foreach (var unit in host.LoadedUnits)
				{
					if (unit.Name.Value == assembly.FullName)
					{
						assemblyDef = (IAssembly)unit;
						break;
					}
				}
			}

			// Otherwise, load the assembly
			if (assemblyDef == null)
				assemblyDef = (IAssembly)host.LoadUnitFrom(assembly.Location);

			// Cache the assembly
			CacheAssembly(assembly, assemblyDef);

			return assemblyDef;
		}

		/// <summary>
		/// Caches the specified assembly mapping and preloads type mappings for all types in the assembly.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="assemblyDef"></param>
		void CacheAssembly(System.Reflection.Assembly assembly, IAssembly assemblyDef)
		{
			// Avoid processing the same assembly more than once
			if (resolvedAssemblies.ContainsKey(assembly))
				return;

			// Cache the resolved assembly
			resolvedAssemblies.Add(assembly, assemblyDef);

			// Preload all types in the assembly
			foreach (var typeDef in assemblyDef.GetAllTypes())
			{
				Type type = assembly.GetType(GetTypeName(typeDef), false);
				if (type != null)
				{
					resolvedTypes[type] = typeDef;
					resolvedTypeDefs[typeDef] = type;
				}
			}
		}

		/// <summary>
		/// Gets the <see cref="IFieldDefinition"/> that corresponds to the specified <see cref="FieldInfo"/>.
		/// </summary>
		/// <param name="declaringType"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		IFieldDefinition ResolveField(ITypeDefinition declaringType, System.Reflection.FieldInfo field)
		{
			return declaringType.Fields.Where(f => AreEquivalent(f, field)).FirstOrDefault();
		}

		/// <summary>
		/// Gets the <see cref="IMethodDefinition"/> that corresponds to the specified <see cref="ConstructorInfo"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		IMethodDefinition ResolveConstructor(ITypeDefinition declaringType, System.Reflection.ConstructorInfo constructor)
		{
			return declaringType.Methods.Where(m => AreEquivalent(m, constructor)).FirstOrDefault();
		}

		/// <summary>
		/// Gets the <see cref="IPropertyDefinition"/> that corresponds to the specified <see cref="PropertyInfo"/>.
		/// </summary>
		/// <param name="declaringType"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		IPropertyDefinition ResolveProperty(ITypeDefinition declaringType, System.Reflection.PropertyInfo property)
		{
			return declaringType.Properties.Where(p => AreEquivalent(p, property)).FirstOrDefault();
		}

		/// <summary>
		/// Gets the <see cref="IMethodDefinition"/> that corresponds to the specified <see cref="MethodInfo"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		IMethodDefinition ResolveMethod(ITypeDefinition declaringType, System.Reflection.MethodInfo method)
		{
			return TypeHelper.GetMethod(declaringType, host.NameTable.GetNameFor(method.Name), method.GetParameters().Select(p => ResolveType(p.ParameterType)).ToArray());
		}

		/// <summary>
		/// Gets the <see cref="IEventDefinition"/> that corresponds to the specified <see cref="EventInfo"/>.
		/// </summary>
		/// <param name="declaringType"></param>
		/// <param name="eventInfo"></param>
		/// <returns></returns>
		IEventDefinition ResolveEvent(ITypeDefinition declaringType, System.Reflection.EventInfo eventInfo)
		{
			return declaringType.Events.Where(e => AreEquivalent(e, eventInfo)).FirstOrDefault();
		}

		/// <summary>
		/// Indicates whether the specified <see cref="INamedTypeDefinition"/> and
		/// <see cref="Type"/> are equivalent, representing the same type.
		/// </summary>
		/// <param name="typeDef"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		internal bool AreEquivalent(ITypeReference typeRef, Type type)
		{
			Type mappedType;
			return (resolvedTypeDefs.TryGetValue(typeRef.ResolvedType, out mappedType) && type == mappedType) || TypeHelper.TypesAreEquivalent(typeRef, ResolveType(type));
		}

		/// <summary>
		/// Gets the name of the specified <see cref="INamedTypeDefinition"/> consistent with the naming
		/// conventions used by the System.Reflection namespace.
		/// </summary>
		/// <param name="typeRef"></param>
		/// <returns></returns>
		static string GetTypeName(ITypeReference typeRef)
		{
			if (typeRef is INestedTypeReference)
				return GetTypeName(((INestedTypeReference)typeRef).ContainingType) + "+" + ((INestedTypeReference)typeRef).Name.Value + (((INestedTypeReference)typeRef).MangleName && ((INestedTypeReference)typeRef).GenericParameterCount > 0 ? "`" + ((INestedTypeReference)typeRef).GenericParameterCount : "");
			else if (typeRef is INamedTypeReference)
				return typeRef.ToString() + (((INamedTypeReference)typeRef).MangleName && ((INamedTypeReference)typeRef).GenericParameterCount > 0 ? "`" + ((INamedTypeReference)typeRef).GenericParameterCount : "");
			else
				return typeRef.ToString();
		}

		/// <summary>
		/// Indicates whether the specified <see cref="IFieldDefinition"/> and
		/// <see cref="FieldInfo"/> are equivalent, representing the same property.
		/// </summary>
		/// <param name="fieldDef"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		internal bool AreEquivalent(IFieldDefinition fieldDef, System.Reflection.FieldInfo field)
		{
			return

				// Ensure field names match
				fieldDef.Name.Value == field.Name &&

				// Ensure field types match
				AreEquivalent(fieldDef.Type, field.FieldType);
		}


		/// <summary>
		/// Indicates whether the specified <see cref="IPropertyDefinition"/> and
		/// <see cref="PropertyInfo"/> are equivalent, representing the same property.
		/// </summary>
		/// <param name="propertyDef"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		internal bool AreEquivalent(IPropertyDefinition propertyDef, System.Reflection.PropertyInfo property)
		{
			return

				// Ensure property names match
				propertyDef.Name.Value == property.Name &&

				// Ensure property types match
				AreEquivalent(propertyDef.Type, property.PropertyType) &&

				// Ensure parameter counts match
				propertyDef.Parameters.Count() == property.GetIndexParameters().Length &&

				// Ensure parameter types match
				property.GetIndexParameters().Select(p => ResolveType(p.ParameterType)).Cast<ITypeDefinition>().SequenceEqual(propertyDef.Parameters.Select(p => p.Type.ResolvedType));
		}

		/// <summary>
		/// Indicates whether the specified <see cref="IMethodDefinition"/> and
		/// <see cref="MethodBase"/> are equivalent, representing the same method.
		/// </summary>
		/// <param name="methodDef"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		internal bool AreEquivalent(IMethodDefinition methodDef, System.Reflection.MethodBase method)
		{
			return

				// Ensure method names match
				methodDef.Name.Value == method.Name &&

				// Ensure parameter counts match
				methodDef.ParameterCount == method.GetParameters().Length &&

				// Ensure parameter types match
				method.GetParameters().All(p => AreEquivalent(methodDef.Parameters.ElementAt(p.Position).Type, p.ParameterType)) &&

				(
				// Method specific tests
					!(method is System.Reflection.MethodInfo) ||
					(
				// Ensure return types match
						AreEquivalent(methodDef.Type, ((System.Reflection.MethodInfo)method).ReturnType) &&

						// Ensure generic parameter types match
						method.GetGenericArguments().Select(t => ResolveType(t)).Cast<ITypeDefinition>().SequenceEqual(methodDef.GenericParameters.Cast<IGenericParameterReference>().Select(p => p.ResolvedType)) &&

						// Ensure generic type parameter counts match
						methodDef.GenericParameterCount == method.GetGenericArguments().Length
					)
				);
		}

		/// <summary>
		/// Indicates whether the specified <see cref="IEventDefinition"/> and
		/// <see cref="PropertyInfo"/> are equivalent, representing the same property.
		/// </summary>
		/// <param name="eventDef"></param>
		/// <param name="eventInfo"></param>
		/// <returns></returns>
		internal bool AreEquivalent(IEventDefinition eventDef, System.Reflection.EventInfo eventInfo)
		{
			return

				// Ensure event names match
				eventDef.Name.Value == eventInfo.Name &&

				// Ensure event handler types match
				AreEquivalent(eventDef.Type, eventInfo.EventHandlerType);
		}

		/// <summary>
		/// Gets the delegate to call to amend a property.
		/// </summary>
		/// <param name="propertyDef"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		GenericMethod ResolvePropertyDelegate(IPropertyDefinition propertyDef, System.Reflection.MethodInfo method)
		{
			var declaringType = ResolveType(method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericTypeDefinition() : method.DeclaringType);

			// Then get the generic Amendment method
			var genericMethod =
				(
					declaringType.IsGeneric ?
					GenericTypeInstance.GetGenericTypeInstance((INamedTypeDefinition)declaringType, new ITypeReference[] { propertyDef.ContainingType }, host.InternFactory) :
					declaringType
				)
				.Methods.Where(m => m.Name.Value == method.Name && m.ParameterCount == method.GetParameters().Length).FirstOrDefault();

			// Finally, return a concrete method instance
			return new GenericMethod()
			{
				Operations = declaringType.Methods.Where(m => m.Name.Value == method.Name && m.ParameterCount == method.GetParameters().Length).FirstOrDefault().Body.Operations,
				Instance = genericMethod.GenericParameterCount == 0 ? genericMethod : new GenericMethodInstance(genericMethod, new ITypeReference[] { propertyDef.Type }, host.InternFactory)
			};
		}

		/// <summary>
		/// Gets the delegate to call to amend a method.
		/// </summary>
		/// <param name="instanceType"></param>
		/// <param name="method"></param>
		/// <param name="methodDef"></param>
		/// <param name="delegateType"></param>
		/// <returns></returns>
		IMethodDefinition ResolveMethodDelegate(ITypeReference instanceType, System.Reflection.MethodInfo method, IMethodDefinition methodDef, ref MethodDelegateType delegateType)
		{
			var declaringType = ResolveType(method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericTypeDefinition() : method.DeclaringType);

			// Create a concrete type instance
			if (declaringType.IsGeneric)
				declaringType = GenericTypeInstance.GetGenericTypeInstance((INamedTypeDefinition)declaringType, new ITypeReference[] { instanceType }, host.InternFactory);

			// Then get the Amendment method
			var targetMethodDef = declaringType.Methods.Where(m => m.Name.Value == method.Name && m.ParameterCount == method.GetParameters().Length).FirstOrDefault();
			var paramDefs = targetMethodDef.Parameters.ToArray();
			var parameters = method.GetParameters();

			// Determine if the delegate should have a context parameter
			int contextParam = delegateType.HasFlag(MethodDelegateType.WithContext) && !delegateType.HasFlag(MethodDelegateType.Before) ? 1 : 0;

			// Determine if the delegate should have an exception parameter
			int exceptionParam = delegateType.HasFlag(MethodDelegateType.Catch) ? 1 : 0;

			// Determine if the target method is an action (void) or function (non-void)
			delegateType = delegateType | (TypeHelper.TypesAreEquivalent(targetMethodDef.Type, host.PlatformType.SystemVoid) ? MethodDelegateType.Action : MethodDelegateType.Function);

			// Determine if the target method should have a result parameter passed in
			if (delegateType.HasFlag(MethodDelegateType.Function) && delegateType.HasFlag(MethodDelegateType.After))
				delegateType = delegateType | MethodDelegateType.HasResultParameter;

			// Determine if the target method uses the object array or explicit parameter syntax
			if (parameters.Exists<string>(1) && // Method Name
				parameters.Exists<object[]>(2 + contextParam + exceptionParam) && // Method Parameters
				(!delegateType.HasFlag(MethodDelegateType.HasResultParameter) || (parameters.Exists<object>(parameters.Length - 1) && method.ReturnType == typeof(object)))) // Method Result
				delegateType |= MethodDelegateType.ArraySyntax;
			else
				delegateType |= MethodDelegateType.ExplicitSyntax;

			// Ensure the instance parameter is compatible
			if (!IsAssignable(targetMethodDef.Parameters.First().Type.ResolvedType, methodDef.ContainingType.ResolvedType))
				throw new ArgumentException("The specified method delegate does not have the correct instance parameter type.");

			// Determine if there are name, context, and/or exception parameters
			int nameParam = delegateType.HasFlag(MethodDelegateType.ArraySyntax) ? 1 : 0;

			// Verify that the exception type is compatible
			if (delegateType.HasFlag(MethodDelegateType.Catch) && !TypeHelper.Type1DerivesFromOrIsTheSameAsType2(paramDefs[1 + nameParam + contextParam].Type.ResolvedType, ResolveType(typeof(Exception))))
				throw new ArgumentException("The specified exception parameter is not a valid exception type.");

			// Perform additional checks for explicit delegates
			if (delegateType.HasFlag(MethodDelegateType.ExplicitSyntax))
			{
				// Return false if the target method does not have the correct number of parameters
				// It should always have one more than the method being amended to support the instance parameter
				// and will have one more parameter if the delegate has a return value
				if (methodDef.ParameterCount !=
					targetMethodDef.ParameterCount -
					1 - // Instance
					contextParam - // Context
					exceptionParam - // Exception
					(delegateType.HasFlag(MethodDelegateType.HasResultParameter) ? 1 : 0)) // Result
					throw new ArgumentException("The specified explicit method delegate does not have the correct number of parameters.");

				// Verify that the parameter types are compatible
				if (!methodDef.Parameters.All(p => TypeHelper.TypesAreEquivalent(p.Type, paramDefs[p.Index + 1 + contextParam + exceptionParam].Type)))
					throw new ArgumentException("The specified explicit method delegate does not have the correct parameter types.");

				// Verify that the result parameter is compatible
				if (delegateType.HasFlag(MethodDelegateType.HasResultParameter) && !TypeHelper.TypesAreEquivalent(methodDef.Type, paramDefs[parameters.Length - 1].Type))
					throw new ArgumentException("The specified result parameter is not valid.");

				// Verify that the return type is compatible
				if (delegateType.HasFlag(MethodDelegateType.Function) && !delegateType.HasFlag(MethodDelegateType.Before) && !TypeHelper.TypesAreEquivalent(methodDef.Type, targetMethodDef.Type))
					throw new ArgumentException("The specified return type is not valid.");
			}

			return targetMethodDef;
		}

		/// <summary>
		/// Determines whether the candidate type can be assigned to a variable of the target type,
		/// taking into account that the candidate, or its base types, may be amended to implement
		/// interfaces that will be assignable to the target type.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="candidate"></param>
		/// <returns></returns>
		bool IsAssignable(ITypeDefinition target, ITypeDefinition candidate)
		{
			while (candidate != null)
			{
				if (TypeHelper.TypesAreAssignmentCompatible(target, candidate))
					return true;

				// See if the candidate type is being amended
				ITypeAmendment amendment;
				if (typeAmendments.TryGetValue(GetTypeName(candidate), out amendment))
				{
					if (amendment.Interfaces.Any(i => TypeHelper.TypesAreAssignmentCompatible(target, ResolveType(i))))
						return true;
				}
				var baseType = candidate.BaseClasses.FirstOrDefault();
				candidate = baseType == null ? null : baseType.ResolvedType;
			}
			return false;
		}

		/// <summary>
		/// Gets the delegate to call to amend an event.
		/// </summary>
		/// <param name="eventDef"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		GenericMethod ResolveEventDelegate(IEventDefinition eventDef, System.Reflection.MethodInfo method)
		{
			var declaringType = ResolveType(method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericTypeDefinition() : method.DeclaringType);

			// Then get the generic Amendment method
			var genericMethod =
				(
					declaringType.IsGeneric ?
					GenericTypeInstance.GetGenericTypeInstance((INamedTypeDefinition)declaringType, new ITypeReference[] { eventDef.ContainingType }, host.InternFactory) :
					declaringType
				)
				.Methods.Where(m => m.Name.Value == method.Name && m.ParameterCount == method.GetParameters().Length).FirstOrDefault();

			// Finally, return a concrete method instance
			return new GenericMethod()
			{
				Operations = declaringType.Methods.Where(m => m.Name.Value == method.Name && m.ParameterCount == method.GetParameters().Length).FirstOrDefault().Body.Operations,
				Instance = genericMethod.GenericParameterCount == 0 ? genericMethod : new GenericMethodInstance(genericMethod, new ITypeReference[] { eventDef.Type }, host.InternFactory)
			};
		}

		/// <summary>
		/// Gets the name of the private backing field to use for new generated properties.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		string GetBackingFieldName(IPropertyAmendment property)
		{
			// Return backing field name consistent with C# compiler conventions
			return "<" + property.Name + ">k__BackingField";
		}
	}

	internal class GenericMethod
	{
		public IEnumerable<IOperation> Operations { get; set; }
		public IMethodReference Instance { get; set; }
	}

	internal static class MethodHelper
	{
		/// <summary>
		/// Determines whether the specified <see cref="MethodBody"/> references
		/// and argument at the specified index.
		/// </summary>
		/// <param name="body"></param>
		/// <param name="argument"></param>
		/// <returns></returns>
		internal static bool ReferencesArgument(this GenericMethod method, int argument)
		{
			return method != null && method.Operations.Where(o =>
			{
				switch (o.OperationCode)
				{
					case OperationCode.Ldarg:
					case OperationCode.Ldarg_S:
					case OperationCode.Ldarga:
					case OperationCode.Ldarga_S:
					case OperationCode.Starg:
					case OperationCode.Starg_S:
						return ((IParameterListEntry)o.Value).Index == argument;
					case OperationCode.Ldarg_0:
						return argument == 0;
					case OperationCode.Ldarg_1:
						return argument == 1;
					case OperationCode.Ldarg_2:
						return argument == 2;
					case OperationCode.Ldarg_3:
						return argument == 3;
					default:
						return false;
				}
			})
			.Any();
		}
	}

	internal static class ParameterHelper
	{
		/// <summary>
		/// Extension method that checks to see if the specified parameter exists in an array of parameters.
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		internal static bool Exists<T>(this System.Reflection.ParameterInfo[] parameters, int index)
		{
			return parameters != null && index < parameters.Length && parameters[index].ParameterType == typeof(T);
		}
	}
}
