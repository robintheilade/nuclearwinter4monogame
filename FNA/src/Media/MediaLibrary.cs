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
	public class MediaLibrary : IDisposable
	{
		#region Public Properties

		public AlbumCollection Albums
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		public MediaSource MediaSource
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public PlaylistCollection Playlists
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public SongCollection Songs
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Public Constructors

		public MediaLibrary()
		{
			IsDisposed = false;
		}

		public MediaLibrary(MediaSource mediaSource)
		{
			IsDisposed = false;
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			IsDisposed = true;
		}

		#endregion

		#region Public Methods

		public void SavePicture(string name, byte[] imageBuffer)
		{
			// On XNA4, this fails on Windows/Xbox. Only Phone is supported.
			throw new NotSupportedException();
		}

		public void SavePicture(string name, Stream source)
		{
			// On XNA4, this fails on Windows/Xbox. Only Phone is supported.
			throw new NotSupportedException();
		}

		#endregion

	}
}
