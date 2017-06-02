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
	#region Amendment.Event

	/// <summary>
	/// Abstract base class for concrete <see cref="Amendment<TType, TAmended>"/> which supports amending code into 
	/// a specific <see cref="Type"/> during compilation.
	/// </summary>
	public abstract partial class Amendment
	{
		public abstract class Event : InterfaceMember, IEventAmendment
		{
			internal Event(string name)
				: base(name)
			{ }

			internal Event(EventInfo eventInfo)
				: base(eventInfo.Name)
			{
				this.EventInfo = eventInfo;
			}

			public abstract Type Type { get; }

			public EventInfo EventInfo { get; private set; }

			public override bool IsAmended
			{
				get
				{
					return base.IsAmended || AdderMethod != null || RemoverMethod != null ||
						BeforeAddMethod != null || AfterAddMethod != null || BeforeRemoveMethod != null || AfterRemoveMethod != null;
				}
			}

			EventInfo IEventAmendment.Implements { get { return Implements; } }

			MethodInfo IEventAmendment.Adder { get { return AdderMethod; } }

			MethodInfo IEventAmendment.Remover { get { return RemoverMethod; } }

			MethodInfo IEventAmendment.BeforeAdd { get { return BeforeAddMethod; } }

			MethodInfo IEventAmendment.AfterAdd { get { return AfterAddMethod; } }

			MethodInfo IEventAmendment.BeforeRemove { get { return BeforeRemoveMethod; } }

			MethodInfo IEventAmendment.AfterRemove { get { return AfterRemoveMethod; } }

			EventInfo implements;
			internal EventInfo Implements
			{
				get
				{
					return implements;
				}
				set
				{
					if (implements != null)
						throw new InvalidOperationException("The event implementation may only be set once.");
					implements = value;
					if (implements != null)
						Name = implements.DeclaringType.FullName + "." + implements.Name;
				}
			}

			internal MethodInfo AdderMethod { get; set; }

			internal MethodInfo RemoverMethod { get; set; }

			internal MethodInfo BeforeAddMethod { get; set; }

			internal MethodInfo AfterAddMethod { get; set; }

			internal MethodInfo BeforeRemoveMethod { get; set; }

			internal MethodInfo AfterRemoveMethod { get; set; }

			/// <summary>
			/// Creates a concrete event with the specified instance type, event type, and name.
			/// </summary>
			/// <param name="instanceType"></param>
			/// <param name="eventType"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public static Event Create(Type instanceType, Type eventType, string name)
			{
				Type amendmentType = typeof(Amendment<>).MakeGenericType(instanceType);
				Type eventAmendmentType = amendmentType.GetNestedType("Event`1").MakeGenericType(instanceType, eventType);
				return (Event)eventAmendmentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null).Invoke(new object[] { name });
			}

			/// <summary>
			/// Creates a new <see cref="Event"/> that implements the specified interface event.
			/// </summary>
			/// <param name="instanceType"></param>
			/// <param name="interfaceEvent"></param>
			/// <returns></returns>
			public static Event Implement(Type instanceType, EventInfo interfaceEvent)
			{
				// Ensure the event is declared on an interface
				if (!interfaceEvent.DeclaringType.IsInterface)
					throw new ArgumentException("Only interface events may be implemented.");

				var evt = Create(instanceType, interfaceEvent.EventHandlerType, interfaceEvent.Name);
				evt.Implements = interfaceEvent;
				return evt;
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Event<TEvent>

	public partial class Amendment<TAmended> : Amendment
	{
		public class Event<TEvent> : Event
		{
			internal Event(string name)
				: base(name)
			{ }

			internal Event(EventInfo prop)
				: base(prop)
			{ }

			public override Type Type
			{
				get
				{
					return typeof(TEvent);
				}
			}

			#region Delegates

			public delegate void EventAdder(TAmended instance, string eventName, TEvent value);

			public delegate void EventRemover(TAmended instance, string eventName, TEvent value);

			#endregion

			#region Methods

			public Event<TEvent> Add(EventAdder adder)
			{
				base.AdderMethod = adder.Method;
				return this;
			}

			public Event<TEvent> Remove(EventRemover remover)
			{
				base.RemoverMethod = remover.Method;
				return this;
			}

			public Event<TEvent> BeforeAdd(EventAdder beforeAdd)
			{
				base.BeforeAddMethod = beforeAdd.Method;
				return this;
			}

			public Event<TEvent> AfterAdd(EventAdder afterAdd)
			{
				base.AfterAddMethod = afterAdd.Method;
				return this;
			}

			public Event<TEvent> BeforeRemove(EventRemover beforeRemove)
			{
				base.BeforeRemoveMethod = beforeRemove.Method;
				return this;
			}

			public Event<TEvent> AfterRemove(EventRemover afterRemove)
			{
				base.AfterRemoveMethod = afterRemove.Method;
				return this;
			}

			#endregion
		}
	}

	#endregion
}
