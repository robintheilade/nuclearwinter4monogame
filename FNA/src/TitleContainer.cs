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

using Microsoft.Xna.Framework.Utilities;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class TitleContainer
	{
		#region Internal Static Properties

		static internal string Location
		{
			get;
			private set;
		}

		#endregion

		#region Static Constructor

		static TitleContainer()
		{
			Location = AppDomain.CurrentDomain.BaseDirectory;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns an open stream to an exsiting file in the title storage area.
		/// </summary>
		/// <param name="name">The filepath relative to the title storage area.</param>
		/// <returns>A open stream or null if the file is not found.</returns>
		public static Stream OpenStream(string name)
		{
			// Normalize the file path.
			string safeName = FileHelpers.NormalizeFilePathSeparators(name);

			// We do not accept absolute paths here.
			if (Path.IsPathRooted(safeName))
			{
				throw new ArgumentException(
					"Invalid filename. TitleContainer.OpenStream " +
					"requires a relative path."
				);
			}

			string absolutePath = Path.Combine(Location, safeName);
			return File.OpenRead(absolutePath);
		}

		#endregion
	}
}

