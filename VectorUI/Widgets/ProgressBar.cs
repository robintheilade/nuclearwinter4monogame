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
    public class ProgressBar: Widget
    {
        //----------------------------------------------------------------------
        public ProgressBar( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mBarTex             = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );
            mBorderTex          = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "Border" );

            Position  = new Point( (int)_marker.Position.X, (int)_marker.Position.Y );
            Size      = new Point( (int)_marker.Size.X, (int)_marker.Size.Y );

            mColor = Color.White;

            Value = 0;
            mfSmoothValue = 0;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
            mfSmoothValue = MathHelper.Lerp( mfSmoothValue, Value, _fElapsedTime * 3f );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Point actualPosition = new Point( Position.X + (int)Offset.X, Position.Y + (int)Offset.Y );

            UISheet.DrawBox( mBorderTex, new Rectangle( actualPosition.X, actualPosition.Y, Size.X, Size.Y ), 20, mColor * Opacity );
            
            if( Value > 0 )
            {
                UISheet.DrawBox( mBarTex, new Rectangle( actualPosition.X, actualPosition.Y, (int)(Size.X * mfSmoothValue), Size.Y ), 20, mColor * Opacity );
            }
        }

        //----------------------------------------------------------------------
        public void ForceValue( float _fValue )
        {
            Value = _fValue;
            mfSmoothValue = Value;
        }

        //----------------------------------------------------------------------
        Texture2D                       mBarTex;
        Texture2D                       mBorderTex;

        public Point                    Position;
        public Point                    Size;
        Color                           mColor;

        public float                    Value;
        public float                    mfSmoothValue;
    }
}
