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
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Genre : IDisposable
	{
		#region Public Properties

		/// <summary>
		/// Gets the AlbumCollection for the Genre.
		/// </summary>
		public AlbumCollection Albums
		{
			get
			{
				throw new NotImplementedException();
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
		/// Gets the name of the Genre.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the SongCollection for the Genre.
		/// </summary>
		public SongCollection Songs
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Public Constructor

		public Genre(string genre)
		{
			Name = genre;
			IsDisposed = false;
		}

		#endregion

		#region Public Dispose Method

		/// <summary>
		/// Immediately releases the unmanaged resources used by this object.
		/// </summary>
		public void Dispose()
		{
			IsDisposed = true;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a String representation of the Genre.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// Gets the hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		#endregion
	}
}
