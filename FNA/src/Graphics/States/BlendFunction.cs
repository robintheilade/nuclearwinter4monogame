#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines a function for color blending.
	/// </summary>
	public enum BlendFunction
	{
		/// <summary>
		/// The function will adds destination to the source. (srcColor * srcBlend) + (destColor * destBlend)
		/// </summary>
		Add,
		/// <summary>
		/// The function will subtracts destination from source. (srcColor * srcBlend) − (destColor * destBlend)
		/// </summary>
		Subtract,
		/// <summary>
		/// The function will subtracts source from destination. (destColor * destBlend) - (srcColor * srcBlend)
		/// </summary>
		ReverseSubtract,
		/// <summary>
		/// The function will extracts minimum of the source and destination. min((srcColor * srcBlend),(destColor * destBlend))
		/// </summary>
		Max,
		/// <summary>
		/// The function will extracts maximum of the source and destination. max((srcColor * srcBlend),(destColor * destBlend))
		/// </summary>
		Min,
	}
}
