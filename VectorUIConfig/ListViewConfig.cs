using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace VectorUI.Widgets
{
    public class ListViewConfig
    {
        public int      FrameCornerSize;
        public int      FramePadding;

        public int      ItemCornerSize;
        public int      ItemPadding;
        public int      ItemHeight;

        public Color    TextColor;
        public Color    TextBlurColor;
    }

}
