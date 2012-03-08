using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI.Style
{
    /// <summary>
    /// A nine-patch can be used to texture a rectangle area with borders
    /// </summary>
    public class Ninepatch
    {
        /// <summary>
        /// The texture to be used
        /// </summary>
        public Texture2D            Texture;

        /// <summary>
        /// The sub rectangle to use from the texture
        /// </summary>
        public Rectangle            SourceRectangle;

        /// <summary>
        /// The border sizes
        /// </summary>
        public Box                  Borders;
    }
}
