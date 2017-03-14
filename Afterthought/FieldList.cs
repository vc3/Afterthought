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
		#region FieldList

		public class FieldList : FieldEnumeration
		{
			new IList<Amendment.Field> fields;

			internal FieldList()
				: base(new List<Amendment.Field>())
			{
				this.fields = (IList<Amendment.Field>)base.fields;
			}

			internal Field Add(Field field)
			{
				fields.Add(field);
				return field;
			}

			public Field<TField> Add<TField>(string name)
			{
				return (Field<TField>)Add(new Field<TField>(name));
			}

			public Field<TField> Add<TField>(string name, Field<TField>.FieldInitializer initializer)
			{
				return (Field<TField>)Add(new Field<TField>(name).Initialize(initializer));
			}
		}

		#endregion

		#region FieldEnumeration

		public partial class FieldEnumeration : MemberEnumeration<FieldEnumeration>, IEnumerable
		{
			internal IEnumerable<Amendment.Field> fields;

			internal FieldEnumeration(IEnumerable<Amendment.Field> fields)
			{
				this.fields = fields;
			}

			#region Delegates

			public delegate object FieldInitializer(TAmended instance, string fieldName);

			#endregion

			#region Methods

			public FieldEnumeration Initialize(FieldInitializer initializer)
			{
				foreach (Amendment.Field field in this)
					field.InitializerMethod = initializer.Method;
				return this;
			}

			/// <summary>
			/// Gets all fields in the set with the specified name.
			/// </summary>
			/// <param name="fields"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public FieldEnumeration Named(string name)
			{
				return new FieldEnumeration(fields.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets the set of fields that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public FieldEnumeration Where(Func<Amendment.Field, bool> predicate)
			{
				return new FieldEnumeration(fields.Where(predicate));
			}

			/// <summary>
			/// Gets all fields in the set with the specified field type.
			/// </summary>
			/// <typeparam name="TField"></typeparam>
			/// <returns></returns>
			public FieldEnumeration<TField> OfType<TField>()
			{
				return new FieldEnumeration<TField>(fields.OfType<Field<TField>>());
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return fields.GetEnumerator();
			}

			#endregion
		}

		#endregion

		#region FieldEnumeration<TField>

		public partial class FieldEnumeration<TField> : MemberEnumeration<FieldEnumeration<TField>>, IEnumerable
		{
			IEnumerable<Field<TField>> fields;

			internal FieldEnumeration(IEnumerable<Field<TField>> fields)
			{
				this.fields = fields;
			}

			#region Methods

			public FieldEnumeration<TField> Initialize(Field<TField>.FieldInitializer initializer)
			{
				foreach (Amendment.Field field in this)
					field.InitializerMethod = initializer.Method;
				return this;
			}

			/// <summary>
			/// Gets all fields in the set with the specified name.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="fields"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public FieldEnumeration<TField> Named(string name)
			{
				return new FieldEnumeration<TField>(fields.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets all fields in the set that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public FieldEnumeration<TField> Where(Func<Field<TField>, bool> predicate)
			{
				return new FieldEnumeration<TField>(fields.Where(predicate));
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return fields.GetEnumerator();
			}

			#endregion
		}

		#endregion
	}
}
