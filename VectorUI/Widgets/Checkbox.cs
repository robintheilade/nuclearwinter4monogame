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
    public class Checkbox: Widget
    {
        //----------------------------------------------------------------------
        public Checkbox( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mbOn = _marker.MarkerType.EndsWith( "On" );

            string basePath = _marker.MarkerFullPath.Substring( 0, _marker.MarkerFullPath.Length - ( mbOn ? "On" : "Off" ).Length );

            mOffTex         = UISheet.Game.Content.Load<Texture2D>( basePath + "Off" );
            mOnTex          = UISheet.Game.Content.Load<Texture2D>( basePath + "On" );
            mOffPressedTex  = UISheet.Game.Content.Load<Texture2D>( basePath + "OffPressed" );
            mOnPressedTex   = UISheet.Game.Content.Load<Texture2D>( basePath + "OnPressed" );

            Matrix matrix = Matrix.CreateScale( _marker.Scale.X, _marker.Scale.Y, 0f ) * Matrix.CreateRotationZ( _marker.Angle );
            mfAngle = (float)Math.Atan2( matrix.M12 /* cos angle */, matrix.M11 /* sin angle */ );

            Vector2 vRotatedCenter = Vector2.Transform( _marker.Size / 2f, matrix );
            mvPosition = _marker.Position + vRotatedCenter;

            mvOrigin = new Vector2( mOffTex.Width, mOffTex.Height ) / 2f;
            mvScale = _marker.Size / new Vector2( mOffTex.Width, mOffTex.Height ) * _marker.Scale;

            mColor = _marker.Color;

            mHitRectangle = new Rectangle( (int)(mvPosition.X - mvOrigin.X ), (int)(mvPosition.Y - mvOrigin.Y ), (int)_marker.Size.X, (int)_marker.Size.Y );
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
            if( _bHandleInput )
            {
#if WINDOWS_PHONE
                foreach( TouchLocation touch in UISheet.Game.TouchMgr.Touches )
                {
                    Vector2 vPos = touch.Position;
#elif WINDOWS
                    Vector2 vPos = new Vector2( UISheet.Game.GamePadMgr.MouseState.X, UISheet.Game.GamePadMgr.MouseState.Y );
#endif
                
                    if(
#if WINDOWS_PHONE
                            touch.State == TouchLocationState.Pressed
#elif WINDOWS
                            UISheet.Game.GamePadMgr.WasMouseButtonJustPressed( 0 )
#endif
                    )
                    {
                        vPos -= mvOrigin;
                        vPos = Vector2.Transform( vPos, Matrix.CreateRotationZ( -mfAngle ) );
                        vPos += mvOrigin;

                        if( mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y ) )
                        {
                            mbPressed = true;
#if WINDOWS_PHONE
                            break;
#endif
                        }
                    }
                    else
                    if(
#if WINDOWS_PHONE
                        touch.State == TouchLocationState.Released
#elif WINDOWS
                        UISheet.Game.GamePadMgr.WasMouseButtonJustReleased( 0 )
#endif
                    )
                    {
                        if( mbPressed )
                        {
                            UISheet.MenuClickSFX.Play();
                            mbOn = ! mbOn;
                            mbPressed = false;
                        }
                    }
                    else
                    if(
#if WINDOWS_PHONE
                        touch.State == TouchLocationState.Moved
#else
                        UISheet.Game.GamePadMgr.MouseState.LeftButton == ButtonState.Pressed
#endif
                    )
                    {
                        mbPressed = mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y );
                    }
#if WINDOWS_PHONE
                }
#endif
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Texture2D tex;

            if( mbOn )
            {
                tex = mbPressed ? mOnPressedTex : mOnTex;
            }
            else
            {
                tex = mbPressed ? mOffPressedTex : mOffTex;
            }

            UISheet.Game.SpriteBatch.Draw( tex, mvPosition + Offset, null, mColor * Opacity, mfAngle, mvOrigin, mvScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        Texture2D       mOffTex;
        Texture2D       mOnTex;
        Texture2D       mOffPressedTex;
        Texture2D       mOnPressedTex;

        bool            mbOn;
        bool            mbPressed;

        Rectangle       mHitRectangle;

        Vector2         mvPosition;
        float           mfAngle;
        Vector2         mvOrigin;
        Vector2         mvScale;
        Color           mColor;
    }
}
