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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexDeclaration : GraphicsResource
	{
		#region Public Properties

		public int VertexStride
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private VertexElement[] elements;

		private Dictionary<int, List<Element>> shaderAttributeInfo = new Dictionary<int, List<Element>>();

		#endregion

		#region Public Constructors

		public VertexDeclaration(
			params VertexElement[] elements
		) : this(GetVertexStride(elements), elements) {
		}

		public VertexDeclaration(
			int vertexStride,
			params VertexElement[] elements
		) {
			if ((elements == null) || (elements.Length == 0))
			{
				throw new ArgumentNullException("elements", "Elements cannot be empty");
			}

			this.elements = (VertexElement[]) elements.Clone();
			VertexStride = vertexStride;
		}

		#endregion

		#region Public Methods

		public VertexElement[] GetVertexElements()
		{
			return (VertexElement[]) elements.Clone();
		}

		#endregion

		#region Internal Methods

		internal void Apply(Shader shader, IntPtr offset, int divisor = 0)
		{
			List<Element> attrInfo;
			int shaderHash = shader.GetHashCode();
			if (!shaderAttributeInfo.TryGetValue(shaderHash, out attrInfo))
			{
				// Get the vertex attribute info and cache it
				attrInfo = new List<Element>(16); // 16, per XNA4 HiDef spec

				foreach (VertexElement ve in elements)
				{
					int attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);

					// XNA appears to ignore usages it can't find a match for, so we will do the same
					if (attributeLocation >= 0)
					{
						attrInfo.Add(new Element()
						{
							Offset = ve.Offset,
							AttributeLocation = attributeLocation,
							NumberOfElements = GetNumberOfElements(ve.VertexElementFormat),
							VertexAttribPointerType = ve.VertexElementFormat,
							Normalized = GetVertexAttribNormalized(ve),
						});
					}
				}

				shaderAttributeInfo.Add(shaderHash, attrInfo);
			}

			// Apply the vertex attribute info
			foreach (Element element in attrInfo)
			{
				GraphicsDevice.GLDevice.AttributeEnabled[element.AttributeLocation] = true;
				GraphicsDevice.GLDevice.Attributes[element.AttributeLocation].Divisor = divisor;
				GraphicsDevice.GLDevice.VertexAttribPointer(
					element.AttributeLocation,
					element.NumberOfElements,
					element.VertexAttribPointerType,
					element.Normalized,
					VertexStride,
					(IntPtr) (offset.ToInt64() + element.Offset)
				);
			}
		}

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Returns the VertexDeclaration for Type.
		/// </summary>
		/// <param name="vertexType">A value type which implements the IVertexType interface.</param>
		/// <returns>The VertexDeclaration.</returns>
		/// <remarks>
		/// Prefer to use VertexDeclarationCache when the declaration lookup
		/// can be performed with a templated type.
		/// </remarks>
		internal static VertexDeclaration FromType(Type vertexType)
		{
			if (vertexType == null)
			{
				throw new ArgumentNullException("vertexType", "Cannot be null");
			}

			if (!vertexType.IsValueType)
			{
				throw new ArgumentException("vertexType", "Must be value type");
			}

			IVertexType type = Activator.CreateInstance(vertexType) as IVertexType;
			if (type == null)
			{
				throw new ArgumentException("vertexData does not inherit IVertexType");
			}

			VertexDeclaration vertexDeclaration = type.VertexDeclaration;
			if (vertexDeclaration == null)
			{
				throw new Exception("VertexDeclaration cannot be null");
			}

			return vertexDeclaration;
		}

		#endregion

		#region Private Static VertexElement Methods

		private static int GetVertexStride(VertexElement[] elements)
		{
			int max = 0;

			for (int i = 0; i < elements.Length; i += 1)
			{
				int start = elements[i].Offset + GetTypeSize(elements[i].VertexElementFormat);
				if (max < start)
				{
					max = start;
				}
			}

			return max;
		}

		private static int GetTypeSize(VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}
			return 0;
		}

		private static int GetNumberOfElements(VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 1;
				case VertexElementFormat.Vector2:
					return 2;
				case VertexElementFormat.Vector3:
					return 3;
				case VertexElementFormat.Vector4:
					return 4;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 2;
				case VertexElementFormat.Short4:
					return 2;
				case VertexElementFormat.NormalizedShort2:
					return 2;
				case VertexElementFormat.NormalizedShort4:
					return 4;
				case VertexElementFormat.HalfVector2:
					return 2;
				case VertexElementFormat.HalfVector4:
					return 4;
			}

			throw new ArgumentException("Should be a value defined in VertexElementFormat", "elementFormat");
		}

		private static bool GetVertexAttribNormalized(VertexElement element)
		{
			/* TODO: This may or may not be the right behavior.
			 *
			 * For instance the VertexElementFormat.Byte4 format is not supposed
			 * to be normalized, but this line makes it so.
			 *
			 * The question is in MS XNA are types normalized based on usage or
			 * normalized based to their format?
			 */
			if (element.VertexElementUsage == VertexElementUsage.Color)
			{
				return true;
			}

			switch (element.VertexElementFormat)
			{
				case VertexElementFormat.NormalizedShort2:
				case VertexElementFormat.NormalizedShort4:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Private Vertex Attribute Element Class

		private class Element
		{
			public int Offset;
			public int AttributeLocation;
			public int NumberOfElements;
			public VertexElementFormat VertexAttribPointerType;
			public bool Normalized;
		}

		#endregion
	}
}
