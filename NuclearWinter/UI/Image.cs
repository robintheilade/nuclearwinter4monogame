using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    /*
     * A Image to be displayed
     */
    public class Image: Widget
    {
        public override bool CanFocus { get { return false; } }

        protected bool mbStretch;
        public bool Stretch {
            get { return mbStretch; }
            set { mbStretch = value; }
        }

        public Color Color = Color.White;

        //----------------------------------------------------------------------
        protected Texture2D mTexture;
        public Texture2D    Texture {
            get { return mTexture; }
            set {
                mTexture = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            int iWidth = mTexture != null ? mTexture.Width : 0;
            int iHeight = mTexture != null ? mTexture.Height : 0;
            
            ContentWidth    = iWidth + Padding.Horizontal;
            ContentHeight   = iHeight + Padding.Vertical;
        }

        //----------------------------------------------------------------------
        public Image( Screen _screen, Texture2D _texture, bool _bStretch )
        : base( _screen )
        {
            mTexture = _texture;
            mbStretch = _bStretch;
            UpdateContentSize();
        }

        public Image( Screen _screen, Texture2D _texture )
        : this( _screen, _texture, false )
        {
        }

        public Image( Screen _screen )
        : this( _screen, null, false )
        {
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            return null;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            HitBox = new Rectangle(
                pCenter.X - ContentWidth / 2,
                pCenter.Y - ContentHeight / 2,
                ContentWidth,
                ContentHeight
            );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if( mTexture == null ) return;

            if( ! mbStretch )
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Vector2( Position.X + Padding.Left, Position.Y + Padding.Top ), Color );
            }
            else
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Rectangle( Position.X + Padding.Left, Position.Y + Padding.Top, Size.X - Padding.Horizontal, Size.Y - Padding.Vertical ), Color );
            }
        }
    }
}
