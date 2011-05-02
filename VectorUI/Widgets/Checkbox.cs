using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace VectorUI.Widgets
{
    public class Checkbox: Widget
    {
        //----------------------------------------------------------------------
        public Checkbox( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mOffTexture     = UISheet.Game.Content.Load<Texture2D>( "Sprites/Menus/CheckboxOff" );
            mOnTexture      = UISheet.Game.Content.Load<Texture2D>( "Sprites/Menus/CheckboxOn" );

            mbOn = _marker.MarkerType == "CheckboxOn";

            Matrix matrix = Matrix.CreateScale( _marker.Scale.X, _marker.Scale.Y, 0f ) * Matrix.CreateRotationZ( _marker.Angle );
            mfAngle = (float)Math.Atan2( matrix.M12 /* cos angle */, matrix.M11 /* sin angle */ );

            Vector2 vRotatedCenter = Vector2.Transform( _marker.Size / 2f, matrix );
            mvPosition = _marker.Position + vRotatedCenter;

            mvOrigin = new Vector2( mOffTexture.Width, mOffTexture.Height ) / 2f;
            mvScale = _marker.Size / new Vector2( mOffTexture.Width, mOffTexture.Height ) * _marker.Scale;

            mColor = _marker.Color;

            mHitRectangle = new Rectangle( (int)(mvPosition.X - mvOrigin.X ), (int)(mvPosition.Y - mvOrigin.Y ), (int)_marker.Size.X, (int)_marker.Size.Y );
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            foreach( TouchLocation touch in UISheet.Game.TouchMgr.Touches )
            {
                if( touch.State == TouchLocationState.Pressed )
                {
                    Vector2 vPos = touch.Position;
                    vPos -= mvOrigin;
                    vPos = Vector2.Transform( vPos, Matrix.CreateRotationZ( -mfAngle ) );
                    vPos += mvOrigin;

                    if( mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y ) )
                    {
                        mbOn = ! mbOn;
                        break;
                    }
                }
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.SpriteBatch.Draw( mbOn ? mOnTexture : mOffTexture, mvPosition, null, mColor, mfAngle, mvOrigin, mvScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        Texture2D       mOffTexture;
        Texture2D       mOnTexture;

        bool            mbOn;

        Rectangle       mHitRectangle;

        Vector2         mvPosition;
        float           mfAngle;
        Vector2         mvOrigin;
        Vector2         mvScale;
        Color           mColor;
    }
}
