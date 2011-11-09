using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace VectorUI
{
    public class UIStyle
    {
        public UIStyle()
        {

        }

        public SpriteFont       Font;
        public SpriteFont       SmallFont;
        public SoundEffect      MenuValidateSFX;
        public SoundEffect      MenuClickSFX;

        public Matrix           SpriteMatrix;
    }
}
