#region License
/* OpenAL# - C# Wrapper for OpenAL Soft
 *
 * Copyright (c) 2014-2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace OpenAL
{
	public static class ALEXT
	{
		/* TODO: All OpenAL Soft extensions! Complete as needed. */

		/* typedef int ALenum */
		public const int ALC_DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;
		public const int ALC_ALL_DEVICES_SPECIFIER = 0x1013;

		public const int AL_FORMAT_MONO_FLOAT32 =		0x10010;
		public const int AL_FORMAT_STEREO_FLOAT32 =		0x10011;

		public const int AL_LOOP_POINTS_SOFT =			0x2015;

		public const int AL_UNPACK_BLOCK_ALIGNMENT_SOFT =	0x200C;
		public const int AL_PACK_BLOCK_ALIGNMENT_SOFT =		0x200D;

		public const int AL_FORMAT_MONO_MSADPCM_SOFT =		0x1302;
		public const int AL_FORMAT_STEREO_MSADPCM_SOFT =	0x1303;
	}
}
