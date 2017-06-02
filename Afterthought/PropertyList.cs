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
		#region PropertyList

		public class PropertyList : PropertyEnumeration
		{
			new IList<Amendment.Property> properties;

			internal PropertyList()
				: base(new List<Amendment.Property>())
			{
				this.properties = (IList<Amendment.Property>)base.properties;
			}

			internal Property Add(Property property)
			{
				properties.Add(property);
				return property;
			}

			public Property<TProperty> Add<TProperty>(string name)
			{
				return (Property<TProperty>)Add(new Property<TProperty>(name));
			}

			public Property<TProperty> Add<TProperty>(string name, Property<TProperty>.PropertyInitializer lazyInitializer)
			{
				return (Property<TProperty>)Add(new Property<TProperty>(name).LazyInitialize(lazyInitializer));
			}

			public Property<TProperty> Add<TProperty>(string name, Property<TProperty>.PropertyGetter getter, Property<TProperty>.PropertySetter setter)
			{
				return (Property<TProperty>)Add(new Property<TProperty>(name).Get(getter).Set(setter));
			}
		}

		#endregion

		#region PropertyEnumeration

		public partial class PropertyEnumeration : MemberEnumeration<PropertyEnumeration>, IEnumerable
		{
			internal IEnumerable<Amendment.Property> properties;

			internal PropertyEnumeration(IEnumerable<Amendment.Property> properties)
			{
				this.properties = properties;
			}

			#region Delegates

			public delegate object PropertyInitializer(TAmended instance, string propertyName);

			public delegate object PropertyGetter(TAmended instance, string propertyName);

			public delegate void PropertySetter(TAmended instance, string propertyName, object value);

			public delegate void BeforePropertyGet(TAmended instance, string propertyName);

			public delegate object AfterPropertyGet(TAmended instance, string propertyName, object returnValue);

			public delegate object BeforePropertySet(TAmended instance, string propertyName, object oldValue, object value);

			public delegate void AfterPropertySet(TAmended instance, string propertyName, object oldValue, object value, object newValue);

			#endregion

			#region Methods

			public PropertyEnumeration Initialize(PropertyInitializer initializer)
			{
				foreach (Amendment.Property property in this)
					property.InitializerMethod = initializer.Method;
				return this;
			}

			public PropertyEnumeration LazyInitialize(PropertyInitializer initializer)
			{
				foreach (Amendment.Property property in this)
					property.LazyInitializerMethod = initializer.Method;
				return this;
			}

			public PropertyEnumeration Get(PropertyGetter getter)
			{
				foreach (Amendment.Property property in this)
					property.GetterMethod = getter.Method;
				return this;
			}

			public PropertyEnumeration Set(PropertySetter setter)
			{
				foreach (Amendment.Property property in this)
					property.SetterMethod = setter.Method;
				return this;
			}

			public PropertyEnumeration BeforeGet(BeforePropertyGet beforeGet)
			{
				foreach (Amendment.Property property in this)
					property.BeforeGetMethod = beforeGet.Method;
				return this;
			}

			public PropertyEnumeration AfterGet(AfterPropertyGet afterGet)
			{
				foreach (Amendment.Property property in this)
					property.AfterGetMethod = afterGet.Method;
				return this;
			}

			public PropertyEnumeration BeforeSet(BeforePropertySet beforeSet)
			{
				foreach (Amendment.Property property in this)
					property.BeforeSetMethod = beforeSet.Method;
				return this;
			}

			public PropertyEnumeration AfterSet(AfterPropertySet afterSet)
			{
				foreach (Amendment.Property property in this)
					property.AfterSetMethod = afterSet.Method;
				return this;
			}

			/// <summary>
			/// Gets all properties in the set with the specified name.
			/// </summary>
			/// <param name="properties"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public PropertyEnumeration Named(string name)
			{
				return new PropertyEnumeration(properties.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets the set of properties that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public PropertyEnumeration Where(Func<Amendment.Property, bool> predicate)
			{
				return new PropertyEnumeration(properties.Where(predicate));
			}

			/// <summary>
			/// Gets all properties in the set with the specified property type.
			/// </summary>
			/// <typeparam name="TProperty"></typeparam>
			/// <returns></returns>
			public PropertyEnumeration<TProperty> OfType<TProperty>()
			{
				return new PropertyEnumeration<TProperty>(properties.OfType<Property<TProperty>>());
			}

			/// <summary>
			/// Gets all properties in the set with the specified property type.
			/// </summary>
			/// <typeparam name="TProperty"></typeparam>
			/// <returns></returns>
			public PropertyEnumeration<TProperty> AsType<TProperty>()
			{
				return new PropertyEnumeration<TProperty>(properties.Cast<Property<TProperty>>());
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return properties.GetEnumerator();
			}

			#endregion
		}

		#endregion

		#region PropertyEnumeration<TProperty>

		public partial class PropertyEnumeration<TProperty> : MemberEnumeration<PropertyEnumeration<TProperty>>, IEnumerable
		{
			IEnumerable<Property<TProperty>> properties;

			internal PropertyEnumeration(IEnumerable<Property<TProperty>> properties)
			{
				this.properties = properties;
			}

			#region Methods

			public PropertyEnumeration<TProperty> Initialize(Property<TProperty>.PropertyInitializer initializer)
			{
				foreach (Amendment.Property property in this)
					property.InitializerMethod = initializer.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> LazyInitialize(Property<TProperty>.PropertyInitializer initializer)
			{
				foreach (Amendment.Property property in this)
					property.LazyInitializerMethod = initializer.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> Get(Property<TProperty>.PropertyGetter getter)
			{
				foreach (Amendment.Property property in this)
					property.GetterMethod = getter.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> Set(Property<TProperty>.PropertySetter setter)
			{
				foreach (Amendment.Property property in this)
					property.SetterMethod = setter.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> BeforeGet(Property<TProperty>.BeforePropertyGet beforeGet)
			{
				foreach (Amendment.Property property in this)
					property.BeforeGetMethod = beforeGet.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> AfterGet(Property<TProperty>.AfterPropertyGet afterGet)
			{
				foreach (Amendment.Property property in this)
					property.AfterGetMethod = afterGet.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> BeforeSet(Property<TProperty>.BeforePropertySet beforeSet)
			{
				foreach (Amendment.Property property in this)
					property.BeforeSetMethod = beforeSet.Method;
				return this;
			}

			public PropertyEnumeration<TProperty> AfterSet(Property<TProperty>.AfterPropertySet afterSet)
			{
				foreach (Amendment.Property property in this)
					property.AfterSetMethod = afterSet.Method;
				return this;
			}

			/// <summary>
			/// Gets all properties in the set with the specified name.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="properties"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public PropertyEnumeration<TProperty> Named(string name)
			{
				return new PropertyEnumeration<TProperty>(properties.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets all properties in the set that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public PropertyEnumeration<TProperty> Where(Func<Property<TProperty>, bool> predicate)
			{
				return new PropertyEnumeration<TProperty>(properties.Where(predicate));
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return properties.GetEnumerator();
			}

			#endregion
		}

		#endregion
	}
}
