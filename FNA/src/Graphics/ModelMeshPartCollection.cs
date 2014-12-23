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
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
		public ModelMeshPartCollection(IList<ModelMeshPart> list) : base(list)
		{
		}
	}
}
