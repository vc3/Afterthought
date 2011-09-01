using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Data.Objects;
using System;

namespace EntityFramework
{
	/// <summary>
	/// Static adapter class that provides the implementation logic for <see cref="IEntityWithKey"/>, 
	/// <see cref="IEntityWithRelationships"/>, and <see cref="IEntityWithChangeTracker"/>.
	/// </summary>
	public static class EntityAdapter
	{
		static MethodInfo getRelatedEnd = typeof(RelationshipManager).GetMethod("GetRelatedEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(string) }, null);

		/// <summary>
		/// Creates a new <see cref="RelationshipManager"/> for the specified instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static RelationshipManager InitializeRelationshipManager(IEntityType instance, string property)
		{
			return RelationshipManager.Create(instance);
		}

		/// <summary>
		/// Allows a <see cref="IEntityChangeTracker"/> to be assigned to the specified instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static void SetChangeTracker(IEntityType instance, IEntityChangeTracker changeTracker)
		{
			instance.ChangeTracker = changeTracker;
			instance.IsInitialized = true;
		}

		/// <summary>
		/// Gets a navigation property reference for the specified property.
		/// </summary>
		/// <typeparam name="TRef"></typeparam>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetReference(IEntityType instance, string property)
		{
			// Return the property reference
			var reference = ((IRelatedEnd)getRelatedEnd.Invoke(instance.RelationshipManager, new object[] { property })).GetEnumerator();
			reference.MoveNext();
			return reference.Current;
		}

		/// <summary>
		/// Gets a navigation property reference for the specified property.
		/// </summary>
		/// <typeparam name="TList"></typeparam>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetList(IEntityType instance, string property)
		{
			// Get the property reference
			var reference = (IRelatedEnd)getRelatedEnd.Invoke(instance.RelationshipManager, new object[] { property });

			// Load the reference if necessary
			if (!reference.IsLoaded)
				reference.Load();

			// Return the reference
			return reference;
		}

		/// <summary>
		/// Sets a navigation property reference for the specified property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <param name="value"></param>
		public static void SetReference(IEntityType instance, string property, object value)
		{
			// Ignore reference setting before the instance is initialized
			if (!instance.IsInitialized)
				return;

			// Get the entity reference
			var reference = (IRelatedEnd)getRelatedEnd.Invoke(instance.RelationshipManager, new object[] { property });

			// Track the current value
			var set = reference.GetEnumerator();
			set.MoveNext();
			var oldValue = set.Current;

			// Update the reference if it is being assigned a different value
			if ((oldValue == null ^ value == null) || (oldValue != null && !oldValue.Equals(value)))
			{
				if (oldValue != null)
					reference.Remove((IEntityType)oldValue);
				if (value != null)
					reference.Add((IEntityType)value);
			}
		}

		/// <summary>
		/// Raises member changing events before a value property is set.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <param name="oldValue"></param>
		/// <param name="value"></param>
		public static TProperty BeforeSetValue<TProperty>(IEntityType instance, string property, TProperty oldValue, TProperty value)
		{
			// Notify the change tracker that the property is changing
			if (instance.IsInitialized)
				instance.ChangeTracker.EntityMemberChanging(property);

			// Return the unmodified value
			return value;
		}

		/// <summary>
		/// Raise member changed events and property change notifications after a property is set.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="property"></param>
		/// <param name="oldValue"></param>
		/// <param name="value"></param>
		/// <param name="newValue"></param>
		public static void AfterSetValue<TProperty>(IEntityType instance, string property, TProperty oldValue, TProperty value, TProperty newValue)
		{
			// Notify the change tracker that the property has changed
			if (instance.IsInitialized)
				instance.ChangeTracker.EntityMemberChanged(property);
		}
	}
}
