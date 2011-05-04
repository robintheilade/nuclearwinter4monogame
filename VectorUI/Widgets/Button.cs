using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace VectorUI.Widgets
{
    public class Button: Widget
    {
        //----------------------------------------------------------------------
        public Button( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mIdleTexture    = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );
            mPressedTexture = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "Pressed" );

            Matrix matrix = Matrix.CreateScale( _marker.Scale.X, _marker.Scale.Y, 0f ) * Matrix.CreateRotationZ( _marker.Angle );
            mfAngle = (float)Math.Atan2( matrix.M12 /* cos angle */, matrix.M11 /* sin angle */ );

            Vector2 vRotatedCenter = Vector2.Transform( _marker.Size / 2f, matrix );
            mvPosition = _marker.Position + vRotatedCenter;

            mvOrigin = _marker.Size / 2f;
            mvScale = _marker.Size / new Vector2( mIdleTexture.Width, mIdleTexture.Height ) * _marker.Scale;

            mColor = _marker.Color;

            mHitRectangle = new Rectangle( (int)(mvPosition.X - mvOrigin.X ), (int)(mvPosition.Y - mvOrigin.Y ), (int)_marker.Size.X, (int)_marker.Size.Y );

            mbPressed = false;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
            if( ! _bHandleInput )
            {
                return;
            }

            mbPressed = false;
#if WINDOWS_PHONE
            foreach( TouchLocation touch in UISheet.Game.TouchMgr.Touches )
            {
                Vector2 vPos = touch.Position;
#elif WINDOWS
                Vector2 vPos = new Vector2( UISheet.Game.GamePadMgr.MouseState.X, UISheet.Game.GamePadMgr.MouseState.Y );
#endif
                vPos -= mvOrigin;
                vPos = Vector2.Transform( vPos, Matrix.CreateRotationZ( -mfAngle ) );
                vPos += mvOrigin;
                
                if( mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y ) 
                    )
                {
#if WINDOWS
                    if( UISheet.Game.GamePadMgr.MouseState.LeftButton == ButtonState.Pressed )
                    {
#endif
                        mbPressed = true;
#if WINDOWS
                    }
#endif

                    if(
#if WINDOWS_PHONE
                        touch.State == TouchLocationState.Released
#elif WINDOWS
                        UISheet.Game.GamePadMgr.WasMouseButtonJustReleased( 0 )
#endif
                        && Click != null )
                    {
                        UISheet.MenuClickSFX.Play();
                        Click( this );
                    }
#if WINDOWS_PHONE
                    break;
#endif
                }
#if WINDOWS_PHONE
            }
#endif
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.SpriteBatch.Draw( mbPressed ? mPressedTexture : mIdleTexture, mvPosition + Offset, null, mColor * Opacity, mfAngle, mvOrigin, mvScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        Texture2D       mIdleTexture;
        Texture2D       mPressedTexture;

        bool            mbPressed;

        Rectangle       mHitRectangle;

        Vector2         mvPosition;
        float           mfAngle;
        Vector2         mvOrigin;
        Vector2         mvScale;
        Color           mColor;
    }
}
