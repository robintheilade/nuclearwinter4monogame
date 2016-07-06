using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Text;

namespace NuclearWinter.UI
{
    /*
     * Various common enum, values & small classes / structs
     */

    //--------------------------------------------------------------------------
    public enum Direction
    {
        Right,
        Down,
        Left,
        Up

    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    //--------------------------------------------------------------------------
    public enum Anchor
    {
        Center,
        Start,
        End,
        Fill
    }

    public static class TextManipulation
    {
        // Used for detecting word boundaries
        public static string WordBoundaries = @",?.;:/\!$(){}[]@=+-*%^`""'~#";
        //public static char[] WordBoundaries = { ',', '?', '.', ';', ':', '/', '\\', '!', '$', '(', ')', '{', '}', '[', ']', '@', '=', '+', '-', '*', '%', '^', '`', '"', '\'', '~', '#' };
    }

    //--------------------------------------------------------------------------
    // SpriteFont decorator with support for custom vertical offset / line spacing
    // and implicit casting
    public class UIFont
    {
        //----------------------------------------------------------------------
        SpriteFont mSpriteFont;
        public int YOffset;

        public ReadOnlyCollection<char> Characters { get { return mSpriteFont.Characters; } }
        public char? DefaultCharacter { get { return mSpriteFont.DefaultCharacter; } set { mSpriteFont.DefaultCharacter = value; } }
        public int LineSpacing { get { return mSpriteFont.LineSpacing; } set { mSpriteFont.LineSpacing = value; } }
        public float Spacing { get { return mSpriteFont.Spacing; } set { mSpriteFont.Spacing = value; } }

        public Vector2 MeasureString(string text)
        {
            return mSpriteFont.MeasureString(text);
        }

        public Vector2 MeasureString(StringBuilder text)
        {
            return mSpriteFont.MeasureString(text);
        }

        //----------------------------------------------------------------------
        public static implicit operator SpriteFont(UIFont instance)
        {
            return instance.mSpriteFont;
        }

        //----------------------------------------------------------------------
        public UIFont(SpriteFont font, int lineSpacing, int yOffset)
        {
            mSpriteFont = font;
            mSpriteFont.LineSpacing = lineSpacing;
            YOffset = yOffset;
        }

        //----------------------------------------------------------------------
        public UIFont(SpriteFont font)
        : this(font, font.LineSpacing, 0)
        {
        }
    }
}
