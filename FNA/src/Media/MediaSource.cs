#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaSource
	{
		#region Public Properties

		public MediaSourceType MediaSourceType
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

		internal MediaSource(string name, MediaSourceType type)
		{
			Name = name;
			MediaSourceType = type;
		}

		#endregion

		#region Public Methods

		public static IList<MediaSource> GetAvailableMediaSources()
		{
			MediaSource[] result = 
			{ 
				new MediaSource("DummyMediaSource", MediaSourceType.LocalDevice)
			};

			return result;
		}

		#endregion
	}
}
