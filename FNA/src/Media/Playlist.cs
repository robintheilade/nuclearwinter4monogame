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
	public sealed class Playlist : IDisposable
	{
		#region Public Properties

		public TimeSpan Duration
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal Playlist(TimeSpan duration, string name)
		{
			Duration = duration;
			Name = name;
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
		}

		#endregion
	}
}
