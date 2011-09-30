using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afterthought;

namespace ExoGraph.Injector
{ 
	/// <summary>
	/// Weave types marked with the <see cref="GraphTypeAttribute"/> to automatically implement
	/// <see cref="IGraphInstance"/> and raise property get and set notifications.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GraphTypeWeaver<T> : Weaver<T>
		where T : IGraphInstance
	{
		public override void Weave()
		{
			// Explicitly implements the IGraphInstance interface by adding a graph instance property
			ImplementInterface(
				typeof(IGraphInstance),
				new Property[] 
				{
					// Create a graph instance property an automatically initialize it to a new graph instance
					new Property<GraphInstance>("GraphInstance")
					{
						Initializer = (instance, propertyName) => new GraphInstance(instance)
					}
				},
				null);
		}

		public static void OnPropertyGet(T instance, string property)
		{
			instance.Instance.OnPropertyGet(property);
		}

		public static void OnPropertySet<P>(T instance, string property, P oldValue, P newValue)
		{
			instance.Instance.OnPropertySet(property, oldValue, newValue);
		}

		/// <summary>
		/// Weaves properties to raise get and set events in ExoGraph.
		/// </summary>
		/// <typeparam name="P"></typeparam>
		/// <param name="property"></param>
		public override void Weave<P>(Property<P> property)
		{
			// Only weave public settable properties
			if (property.PropertyInfo.GetGetMethod() == null || property.PropertyInfo.GetSetMethod() == null || !property.PropertyInfo.GetGetMethod().IsPublic)
				return;

			// Notify the graph instance that the property is being retrieved
			property.BeforeGet = OnPropertyGet;

			// Notify the graph instance that the property has been set
			property.AfterSet = OnPropertySet;
		}
	}
}
