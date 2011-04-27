using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorUI.Widgets
{
    public class Button: Widget
    {
        //----------------------------------------------------------------------
        public Button( UISheet _sheet, Marker _marker )
        : base( _sheet )
        {
            mIdleTexture    = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );
            mPressedTexture = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "Pressed" );

            Matrix matrix = Matrix.CreateScale( _marker.Scale.X, _marker.Scale.Y, 0f ) * Matrix.CreateRotationZ( _marker.Angle );
            mfAngle = (float)Math.Atan2( matrix.M12 /* cos angle */, matrix.M11 /* sin angle */ );

            Vector2 vRotatedCenter = Vector2.Transform( _marker.Size / 2f, matrix );
            mvPosition = _marker.Position + vRotatedCenter;

            mvOrigin = new Vector2( mIdleTexture.Width, mIdleTexture.Height ) / 2f;
            mvScale = _marker.Size / new Vector2( mIdleTexture.Width, mIdleTexture.Height ) * _marker.Scale;

            mColor = _marker.Color;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.SpriteBatch.Draw( mIdleTexture, mvPosition, null, mColor, mfAngle, mvOrigin, mvScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        Texture2D       mIdleTexture;
        Texture2D       mPressedTexture;

        Vector2         mvPosition;
        float           mfAngle;
        Vector2         mvOrigin;
        Vector2         mvScale;
        Color           mColor;
    }
}
