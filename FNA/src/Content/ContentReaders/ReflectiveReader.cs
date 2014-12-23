#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class ReflectiveReader<T> : ContentTypeReader
	{
		#region Reader Delegates

		delegate void ReadElement(ContentReader input, object parent);

		private List<ReadElement> readers;

		#endregion

		#region Private Variables

		private ConstructorInfo constructor;

		private ContentTypeReader baseTypeReader;

		#endregion

		#region Internal Constructor

		internal ReflectiveReader() : base(typeof(T))
		{
		}

		#endregion

		#region Protected ContentTypeReader Methods

		protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			base.Initialize(manager);

			Type baseType = TargetType.BaseType;
			if (baseType != null && baseType != typeof(object))
			{
				baseTypeReader = manager.GetTypeReader(baseType);
			}

			constructor = TargetType.GetDefaultConstructor();

			PropertyInfo[] properties = TargetType.GetAllProperties();
			FieldInfo[] fields = TargetType.GetAllFields();
			readers = new List<ReadElement>(fields.Length + properties.Length);

			// Gather the properties.
			foreach (PropertyInfo property in properties)
			{
				ReadElement read = GetElementReader(manager, property);
				if (read != null)
				{
					readers.Add(read);
				}
			}

			// Gather the fields.
			foreach (FieldInfo field in fields)
			{
				ReadElement read = GetElementReader(manager, field);
				if (read != null)
				{
					readers.Add(read);
				}
			}
		}

		protected internal override object Read(ContentReader input, object existingInstance)
		{
			T obj;
			if (existingInstance != null)
			{
				obj = (T) existingInstance;
			}
			else
			{
				if (constructor == null)
				{
					obj = (T) Activator.CreateInstance(typeof(T));
				}
				else
				{
					obj = (T) constructor.Invoke(null);
				}
			}
		
			if (baseTypeReader != null)
			{
				baseTypeReader.Read(input, obj);
			}

			// Box the type.
			object  boxed = (object) obj;

			foreach (ReadElement reader in readers)
			{
				reader(input, boxed);
			}

			// Unbox it... required for value types.
			obj = (T) boxed;

			return obj;
		}

		#endregion

		#region Private Static Methods

		private static ReadElement GetElementReader(
			ContentTypeReaderManager manager,
			MemberInfo member
		) {
			PropertyInfo property = member as PropertyInfo;
			FieldInfo field = member as FieldInfo;

			// Properties must have public get and set
			if (	property != null &&
				(	property.CanWrite == false ||
					property.CanRead == false	)	)
			{
				return null;
			}

			if (property != null && property.Name == "Item")
			{
				MethodInfo getMethod = property.GetGetMethod();
				MethodInfo setMethod = property.GetSetMethod();

				if (	(getMethod != null && getMethod.GetParameters().Length > 0) ||
					(setMethod != null && setMethod.GetParameters().Length > 0)	)
				{
					/* This is presumably a property like this[indexer] and this
					 * should not get involved in the object deserialization
					 */
					return null;
				}
			}

			Attribute attr = Attribute.GetCustomAttribute(
				member,
				typeof(ContentSerializerIgnoreAttribute)
			);
			if (attr != null)
			{
				return null;
			}

			ContentSerializerAttribute contentSerializerAttribute = Attribute.GetCustomAttribute(
				member,
				typeof(ContentSerializerAttribute)
			) as ContentSerializerAttribute;

			bool isSharedResource = false;
			if (contentSerializerAttribute != null)
			{
				isSharedResource = contentSerializerAttribute.SharedResource;
			}
			else
			{
				if (property != null)
				{
					MethodInfo getMethod = property.GetGetMethod();
					if (getMethod == null || !getMethod.IsPublic)
					{
						return null;
					}
					MethodInfo setMethod = property.GetSetMethod();
					if (setMethod == null || !setMethod.IsPublic)
					{
						return null;
					}
				}
				else
				{
					if (!field.IsPublic)
					{
						return null;
					}

					// evolutional: Added check to skip initialise only fields
					if (field.IsInitOnly)
					{
						return null;
					}

					/* Private fields can be serialized if they have
					 * ContentSerializerAttribute added to them.
					 */
					if (field.IsPrivate && contentSerializerAttribute == null)
					{
						return null;
					}
				}
			}

			Action<object, object> setter;
			ContentTypeReader reader;
			Type elementType;
			if (property != null)
			{
				elementType = property.PropertyType;
				reader = manager.GetTypeReader(property.PropertyType);
				setter = (o, v) => property.SetValue(o, v, null);
			}
			else
			{
				elementType = field.FieldType;
				reader = manager.GetTypeReader(field.FieldType);
				setter = field.SetValue;
			}

			if (isSharedResource)
			{
				return (input, parent) =>
				{
					Action<object> action = value => setter(parent, value);
					input.ReadSharedResource(action);
				};
			}

			Func<object> construct = () => null;
			if (elementType.IsClass && !elementType.IsAbstract)
			{
				ConstructorInfo defaultConstructor = elementType.GetDefaultConstructor();
				if (defaultConstructor != null)
				{
					construct = () => defaultConstructor.Invoke(null);
				}
			}

			// Reading elements serialized as "object".
			if (reader == null && elementType == typeof(object))
			{
				return (input, parent) =>
				{
					object obj2 = input.ReadObject<object>();
					setter(parent, obj2);
				};
			}

			// evolutional: Fix. We can get here and still be NULL, exit gracefully
			if (reader == null)
			{
				return null;
			}

			return (input, parent) =>
			{
				object existing = construct();
				object obj2 = input.ReadObject(reader, existing);
				setter(parent, obj2);
			};
		}

		#endregion
	}
}
