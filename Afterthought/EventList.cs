using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;

namespace Afterthought
{
	public partial class Amendment<TAmended> : Amendment
	{
		#region EventList

		public class EventList : EventEnumeration
		{
			new IList<Amendment.Event> events;

			internal EventList()
				: base(new List<Amendment.Event>())
			{
				this.events = (IList<Amendment.Event>)base.events;
			}

			internal Event Add(Event @event)
			{
				events.Add(@event);
				return @event;
			}

			public Event<TEvent> Add<TEvent>(string name)
			{
				return (Event<TEvent>)Add(new Event<TEvent>(name));
			}

			public Event<TEvent> Add<TEvent>(string name, Event<TEvent>.EventAdder adder, Event<TEvent>.EventRemover remover)
			{
				return (Event<TEvent>)Add(new Event<TEvent>(name).Add(adder).Remove(remover));
			}
		}

		#endregion

		#region EventEnumeration

		public partial class EventEnumeration : MemberEnumeration<EventEnumeration>, IEnumerable
		{
			internal IEnumerable<Amendment.Event> events;
			
			internal EventEnumeration(IEnumerable<Amendment.Event> events)
			{
				this.events = events;
			}

			#region Delegates

			public delegate void EventAdder(TAmended instance, string eventName, object value);

			public delegate void EventRemover(TAmended instance, string eventName, object value);

			#endregion

			#region Methods

			public EventEnumeration Add(EventAdder adder)
			{
				foreach (Amendment.Event @event in this)
					@event.AdderMethod = adder.Method;
				return this;
			}

			public EventEnumeration BeforeAdd(EventAdder beforeAdd)
			{
				foreach (Amendment.Event @event in this)
					@event.BeforeAddMethod = beforeAdd.Method;
				return this;
			}

			public EventEnumeration AfterAdd(EventAdder afterAdd)
			{
				foreach (Amendment.Event @event in this)
					@event.AfterAddMethod = afterAdd.Method;
				return this;
			}

			public EventEnumeration Remove(EventRemover remover)
			{
				foreach (Amendment.Event @event in this)
					@event.RemoverMethod = remover.Method;
				return this;
			}

			public EventEnumeration BeforeRemove(EventRemover beforeRemove)
			{
				foreach (Amendment.Event @event in this)
					@event.BeforeRemoveMethod = beforeRemove.Method;
				return this;
			}

			public EventEnumeration AfterRemove(EventRemover afterRemove)
			{
				foreach (Amendment.Event @event in this)
					@event.AfterRemoveMethod = afterRemove.Method;
				return this;
			}

			/// <summary>
			/// Gets all events in the set with the specified name.
			/// </summary>
			/// <param name="events"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public EventEnumeration Named(string name)
			{
				return new EventEnumeration(events.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets the set of events that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public EventEnumeration Where(Func<Amendment.Event, bool> predicate)
			{
				return new EventEnumeration(events.Where(predicate));
			}

			/// <summary>
			/// Gets all events in the set with the specified @event type.
			/// </summary>
			/// <typeparam name="TEvent"></typeparam>
			/// <returns></returns>
			public EventEnumeration<TEvent> OfType<TEvent>()
			{
				return new EventEnumeration<TEvent>(events.OfType<Event<TEvent>>());
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return events.GetEnumerator();
			}

			#endregion
		}

		#endregion

		#region EventEnumeration<TEvent>

		public partial class EventEnumeration<TEvent> : MemberEnumeration<EventEnumeration<TEvent>>, IEnumerable
		{
			IEnumerable<Event<TEvent>> events;

			internal EventEnumeration(IEnumerable<Event<TEvent>> events)
			{
				this.events = events;
			}

			#region Methods

			public EventEnumeration<TEvent> Add(Event<TEvent>.EventAdder adder)
			{
				foreach (Amendment.Event @event in this)
					@event.AdderMethod = adder.Method;
				return this;
			}

			public EventEnumeration<TEvent> Remove(Event<TEvent>.EventRemover remover)
			{
				foreach (Amendment.Event @event in this)
					@event.RemoverMethod = remover.Method;
				return this;
			}

			public EventEnumeration<TEvent> BeforeAdd(Event<TEvent>.EventAdder beforeAdd)
			{
				foreach (Amendment.Event @event in this)
					@event.BeforeAddMethod = beforeAdd.Method;
				return this;
			}

			public EventEnumeration<TEvent> AfterAdd(Event<TEvent>.EventAdder afterAdd)
			{
				foreach (Amendment.Event @event in this)
					@event.AfterAddMethod = afterAdd.Method;
				return this;
			}

			public EventEnumeration<TEvent> BeforeRemove(Event<TEvent>.EventRemover beforeRemove)
			{
				foreach (Amendment.Event @event in this)
					@event.BeforeRemoveMethod = beforeRemove.Method;
				return this;
			}

			public EventEnumeration<TEvent> AfterRemove(Event<TEvent>.EventRemover afterRemove)
			{
				foreach (Amendment.Event @event in this)
					@event.AfterRemoveMethod = afterRemove.Method;
				return this;
			}

			/// <summary>
			/// Gets all events in the set with the specified name.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="events"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public EventEnumeration<TEvent> Named(string name)
			{
				return new EventEnumeration<TEvent>(events.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets all events in the set that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public EventEnumeration<TEvent> Where(Func<Event<TEvent>, bool> predicate)
			{
				return new EventEnumeration<TEvent>(events.Where(predicate));
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return events.GetEnumerator();
			}

			#endregion
		}

		#endregion
	}
}
