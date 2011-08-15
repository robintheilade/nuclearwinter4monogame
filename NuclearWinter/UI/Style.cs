using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    /*
     * Holds styles for widgets
     */
    public class Style
    {
        public SpriteFont       SmallFont;
        public SpriteFont       MediumFont;
        public SpriteFont       BigFont;

        public Texture2D        ButtonFrame;
        public Texture2D        ButtonFrameHover;
        public Texture2D        ButtonFrameDown;
        public Texture2D        ButtonFramePressed;

        public Texture2D        ButtonFrameFocused;
        public Texture2D        ButtonFrameDownFocused;

        public Texture2D        ButtonFrameLeft;
        public Texture2D        ButtonFrameLeftDown;
        public Texture2D        ButtonFrameMiddle;
        public Texture2D        ButtonFrameMiddleDown;
        public Texture2D        ButtonFrameRight;
        public Texture2D        ButtonFrameRightDown;

        public Texture2D        DropDownArrow;

        public Texture2D        GridFrame;
        public Texture2D        GridHeaderFrame;
        public Texture2D        GridBoxFrame;
        public Texture2D        GridBoxFrameHover;
        public Texture2D        GridBoxFrameSelected;
        public Texture2D        GridBoxFrameFocused;
    }
}
