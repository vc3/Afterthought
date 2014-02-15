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
	#region Amendment

	/// <summary>
	/// Abstract base class for concrete <see cref="Amendment<TType, TAmended>"/> which supports amending
	/// a specific <see cref="Type"/> af compilation.
	/// </summary>
	public abstract partial class Amendment
	{
		/// <summary>
		/// Constructs and initializes a new <see cref="Amendment"/>.
		/// </summary>
		internal Amendment()
		{ }

		public abstract Type Type { get; }

		public abstract Type AmendedType { get; }

		public override string ToString()
		{
			return Type.FullName;
		}
	}

	#endregion

	#region Amendment<TAmended>

	public partial class Amendment<TAmended> : Amendment, ITypeAmendment
	{
		List<Type> interfaces = new List<Type>();
	    private readonly Type amendingType;

		#region Constructors

		public Amendment(Type amendingType)
		{
		    this.amendingType = amendingType;

			// Get the type of the current amendment
			Type amendmentType = typeof(Amendment<>).MakeGenericType(AmendedType);

			// Attributes
			this.Attributes = new AttributeList();

			// Fields
			this.Fields = new FieldList();
			foreach (var fieldInfo in Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				Type fieldAmendmentType = amendmentType.GetNestedType("Field`1").MakeGenericType(AmendedType, fieldInfo.FieldType);
				Field field = (Field)fieldAmendmentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(FieldInfo) }, null).Invoke(new object[] { fieldInfo });
				Fields.Add(field);
			}

			// Constructors
			this.Constructors = new ConstructorList();
			foreach (ConstructorInfo constructor in Type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				Type[] parameters = GetType().BaseType.GetGenericArguments().Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
				var type = GetType().BaseType.GetNestedTypes().Where(t => t.BaseType.Name == "Constructor" && t.GetGenericArguments().Length == parameters.Length).FirstOrDefault();
				if (type != null)
					this.Constructors.Add((Amendment.Constructor)type.MakeGenericType(parameters).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(ConstructorInfo) }, null).Invoke(new object[] { constructor }));
				else
					this.Constructors.Add(new Constructor(constructor));
			}

			// Properties
			this.Properties = new PropertyList();
			foreach (var propertyInfo in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				Type propertyAmendmentType = amendmentType.GetNestedType("Property`1").MakeGenericType(AmendedType, propertyInfo.PropertyType);
				Property property = (Property)propertyAmendmentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(PropertyInfo) }, null).Invoke(new object[] { propertyInfo });
				Properties.Add(property);
			}

			// Methods
			this.Methods = new MethodList(Type);
			foreach (MethodInfo method in Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(m => !m.IsConstructor && !(m.IsSpecialName && m.IsHideBySig && (m.Name.StartsWith("get_") || m.Name.StartsWith("set_")))))
			{
				Type[] parameters = GetType().BaseType.GetGenericArguments().Concat(method.GetParameters().Select(p => p.ParameterType)).ToArray();
				var type = GetType().BaseType.GetNestedTypes().Where(t => t.BaseType.Name == "Method" && t.GetGenericArguments().Length == parameters.Length).FirstOrDefault();
				if (type != null && !parameters.Any(p => p.IsByRef) && !method.IsGenericMethodDefinition)
					this.Methods.Add((Amendment.Method)type.MakeGenericType(parameters).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MethodInfo) }, null).Invoke(new object[] { method }));
				else
					this.Methods.Add(new Method(method));
			}

			// Events
			this.Events = new EventList();
			foreach (var eventInfo in Type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				Type eventAmendmentType = amendmentType.GetNestedType("Event`1").MakeGenericType(AmendedType, eventInfo.EventHandlerType);
				Event @event = (Event)eventAmendmentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(EventInfo) }, null).Invoke(new object[] { eventInfo });
				Events.Add(@event);
			}
		}

		#endregion

		#region Properties

		public override Type Type
		{
		    get
		    {
		        return amendingType;
		    }
		}

		public override Type AmendedType
		{
			get
			{
				return typeof(TAmended);
			}
		}

		public AttributeList Attributes { get; private set; }

		public FieldList Fields { get; private set; }

		public ConstructorList Constructors { get; private set; }

		public PropertyList Properties { get; private set; }

		public MethodList Methods { get; private set; }

		public EventList Events { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Allows subclasses to implement interfaces for the types being amended.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <param name="members"></param>
		public void Implement<TInterface>(params InterfaceMember[] members)
		{
			var interfaceType = typeof(TInterface);

			// Ensure the specified type is an interface
			if (interfaceType == null || !interfaceType.IsInterface)
				throw new ArgumentException("Only interface types can be implemented.");

			// Track the interface being implemented
			interfaces.Add(interfaceType);

			// Add members that implement the interface
			if (members != null)
			{
				foreach (var member in members)
				{
					// Properties
					if (member is Property)
					{
						var property = (Property)member;

						// Determine the property being implemented
						property.Implements = interfaceType.GetProperty(property.Name);

						// Verify that the property actually implements the specified interface
						if (property.Implements == null || property.Implements.PropertyType != property.Type)
							throw new ArgumentException("The specified property, " + property.Name + ", is not valid for interface " + interfaceType.FullName + ".");
					}

					// Methods
					else if (member is Amendment.Method)
					{
						var method = (Amendment.Method)member;

						// Verify that the method has an implementation or raises an event
						if (method.ImplementationMethod == null && method.RaisesEvent == null)
							throw new ArgumentException("The method must have an implementation in order to implement an interface.");

						// Get the method arguments
						var args = method.ImplementationMethod != null ?
							method.ImplementationMethod.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray() :
							method.RaisesEvent.Type.GetMethod("Invoke").GetParameters()
								.SkipWhile((p, i) => (i == 0 && p.ParameterType == typeof(object)) || (i == 1 && p.ParameterType == typeof(EventArgs)))
								.Select(p => p.ParameterType).ToArray();

						// Determine the method being implemented
						method.Implements = interfaceType.GetMethods()
							.FirstOrDefault(m => m.Name == method.Name && m.GetParameters().Length == args.Length &&
								m.GetParameters().All(p => args[p.Position] == p.ParameterType));

						// Verify that the method actually implements the specified interface
						if (method.Implements == null)
							throw new ArgumentException("The specified method, " + method.Name + ", is not valid for interface " + interfaceType.FullName + ".");
					}

					// Events
					if (member is Event)
					{
						var @event = (Event)member;

						// Determine the event being implemented
						@event.Implements = interfaceType.GetEvent(@event.Name);

						// Verify that the event actually implements the specified interface
						if (@event.Implements == null || @event.Implements.EventHandlerType != @event.Type)
							throw new ArgumentException("The specified event, " + @event.Name + ", is not valid for interface " + interfaceType.FullName + ".");
					}
				}
			}
		}

		#endregion

		#region ITypeAmendment

		string IMemberAmendment.Name { get { return Type.FullName; } }

		IEnumerable<IAttributeAmendment> IMemberAmendment.Attributes
		{
			get
			{
				return Attributes.Cast<IAttributeAmendment>();
			}
		}

		IEnumerable<Type> ITypeAmendment.Interfaces
		{
			get
			{
				return interfaces;
			}
		}

		IEnumerable<IFieldAmendment> ITypeAmendment.Fields
		{
			get
			{
				return Fields.Where(f => f.IsAmended).Cast<IFieldAmendment>();
			}
		}

		IEnumerable<IConstructorAmendment> ITypeAmendment.Constructors
		{
			get
			{
				return Constructors.Where(c => c.IsAmended).Cast<IConstructorAmendment>();
			}
		}

		IEnumerable<IPropertyAmendment> ITypeAmendment.Properties
		{
			get
			{
				return Properties.Where(p => p.IsAmended).Cast<IPropertyAmendment>();
			}
		}

		IEnumerable<IMethodAmendment> ITypeAmendment.Methods
		{
			get
			{
				return Methods.Where(m => m.IsAmended).Cast<IMethodAmendment>();
			}
		}

		IEnumerable<IEventAmendment> ITypeAmendment.Events
		{
			get
			{
				return Events.Where(e => e.IsAmended).Cast<IEventAmendment>();
			}
		}

		#endregion
	}

	#endregion

    #region Amendment<TType, TAmended>

    public class Amendment<TType, TAmended>: Amendment<TAmended>
    {
        public Amendment(): base(typeof (TType))
        {
        }
    }

    #endregion
}
