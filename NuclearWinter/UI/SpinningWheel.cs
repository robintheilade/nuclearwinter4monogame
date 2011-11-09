using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public class SpinningWheel: Image
    {
        float           mfAngle;

        //----------------------------------------------------------------------
        public SpinningWheel( Screen _screen, Texture2D _texture )
        : base( _screen, _texture )
        {
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            mfAngle += _fElapsedTime * 3f;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( mTexture == null ) return;

            Vector2 vOrigin = new Vector2( mTexture.Width / 2f, mTexture.Height / 2f );

            if( ! mbStretch )
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Vector2( LayoutRect.Center.X - ContentWidth / 2 + Padding.Left, LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top ) + vOrigin, null, Color, mfAngle, vOrigin, 1f, SpriteEffects.None, 0f );
            }
            else
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Rectangle( LayoutRect.X + Padding.Left + (int)vOrigin.X, LayoutRect.Y + Padding.Top + (int)vOrigin.Y, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ), null, Color, mfAngle, new Vector2( mTexture.Width / 2f, mTexture.Height / 2f ), SpriteEffects.None, 0f );
            }
        }
    }
}
