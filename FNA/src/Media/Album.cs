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
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Album : IDisposable
	{
		#region Public Properties

		/// <summary>
		/// Gets the artist of the Album.
		/// </summary>
		public Artist Artist
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the duration of the Album.
		/// </summary>
		public TimeSpan Duration
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the Genre of the Album.
		/// </summary>
		public Genre Genre
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the Album has associated album art.
		/// </summary>
		public bool HasArt
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
		/// Gets the name of the Album.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a SongCollection that contains the songs on the album.
		/// </summary>
		public SongCollection Songs
		{
			get;
			private set;
		}

		#endregion

		#region Private Constructor

		private Album(SongCollection songCollection, string name, Artist artist, Genre genre)
		{
			Songs = songCollection;
			Name = name;
			Artist = artist;
			Genre = genre;
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
		/// Returns the stream that contains the album art image data.
		/// </summary>
		public Stream GetAlbumArt()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the stream that contains the album thumbnail image data.
		/// </summary>
		public Stream GetThumbnail()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a String representation of this Album.
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
