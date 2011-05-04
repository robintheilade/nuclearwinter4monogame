using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorUI.Widgets
{
    public class Label: Widget
    {
        //----------------------------------------------------------------------
        public Label( UISheet _sheet, Text _text )
        : base( _text.Name, _sheet )
        {
            //Vector2 vSize = UISheet.Font.MeasureString( Text );
            mvPosition = _text.Position - new Vector2( 0, /* FIXME: hard-coded stuff! */ 35 );
            mvOrigin = Vector2.Zero;

            mAnchor = _text.Anchor;
            Text = UISheet.Game.GetUIString( _text.TextSpans[0].Value.Substring( "String".Length ) );


            mColor = _text.FillColor;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.DrawBlurredText( UISheet.Font, Text, mvPosition + Offset, mColor * Opacity, mvOrigin, 1f );
        }

        //----------------------------------------------------------------------
        public string   Text
        {
            get {
                return mstrText;
            }

            set {
                mstrText = value;

                switch( Anchor )
                {
                    case TextAnchor.Start:
                        mvOrigin = new Vector2( 0f, 0f );
                        break;
                    case TextAnchor.Middle:
                        mvOrigin = new Vector2( UISheet.Font.MeasureString( mstrText ).X / 2f, 0f );
                        break;
                    case TextAnchor.End:
                        mvOrigin = new Vector2( UISheet.Font.MeasureString( mstrText ).X, 0f );
                        break;
                }
            }
        }

        string          mstrText;

        TextAnchor      Anchor
        {
            get {
                return mAnchor;
            }

            set {
                mAnchor = value;
                if( mAnchor == TextAnchor.Middle )
                {
                    mvOrigin = new Vector2( UISheet.Font.MeasureString( Text ).X / 2f, 0f );
                }
            }
        }

        TextAnchor      mAnchor;

        Vector2         mvPosition;
        Vector2         mvOrigin;
        Color           mColor;
    }
}
