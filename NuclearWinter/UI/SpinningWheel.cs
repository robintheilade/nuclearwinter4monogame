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
            Screen.AddWidgetToUpdateList( this );
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fElapsedTime )
        {
            mfAngle += _fElapsedTime * 3f;

            return true;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if( mTexture == null ) return;

            Vector2 vOrigin = new Vector2( mTexture.Width / 2f, mTexture.Height / 2f );

            if( ! mbStretch )
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Vector2( Position.X + Padding.Left, Position.Y + Padding.Top ) + vOrigin, null, Color, mfAngle, vOrigin, 1f, SpriteEffects.None, 0f );
            }
            else
            {
                Screen.Game.SpriteBatch.Draw( mTexture, new Rectangle( Position.X + Padding.Left + (int)vOrigin.X, Position.Y + Padding.Top + (int)vOrigin.Y, Size.X - Padding.Horizontal, Size.Y - Padding.Vertical ), null, Color, mfAngle, new Vector2( mTexture.Width / 2f, mTexture.Height / 2f ), SpriteEffects.None, 0f );
            }
        }
    }
}
