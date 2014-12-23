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

namespace Microsoft.Xna.Framework.Media
{
	public sealed class AlbumCollection : IDisposable
	{
		#region Public Properties

		/// <summary>
		/// Gets the number of Album objects in the AlbumCollection.
		/// </summary>
		public int Count
		{
			get
			{
				return albumCollection.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the Album at the specified index in the AlbumCollection.
		/// </summary>
		/// <param name="index">Index of the Album to get.</param>
		public Album this[int index]
		{
			get
			{
				return albumCollection[index];
			}
		}

		#endregion

		#region Private Variables

		private List<Album> albumCollection;

		#endregion

		#region Public Constructor

		public AlbumCollection(List<Album> albums)
		{
			albumCollection = albums;
			IsDisposed = false;
		}

		#endregion

		#region Public Dispose Method

		/// <summary>
		/// Immediately releases the unmanaged resources used by this object.
		/// </summary>
		public void Dispose()
		{
			foreach (Album album in albumCollection)
			{
				album.Dispose();
			}
			IsDisposed = true;
		}

		#endregion
	}
}
