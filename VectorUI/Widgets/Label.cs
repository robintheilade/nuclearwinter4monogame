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
            mvPosition = _text.Position - new Vector2( 0, _text.FontSize );
            mvOrigin = Vector2.Zero;
            mfScale = _text.FontSize / 40f /* FIXME: Hard-coded since there is no way to get the base font size from SpriteFront */;

            mAnchor = _text.Anchor;

            string strId = _text.TextSpans[0].Value;
            if( strId.StartsWith( "String" ) ) // Legacy stuff
            {
                strId = strId.Substring( "String".Length );
            }

            Text = UISheet.Game.GetUIString( strId );

            mColor = _text.FillColor;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.DrawBlurredText( UISheet.Style.Font, Text, mvPosition + Offset, mColor * Opacity, mvOrigin, mfScale * Scale.X );
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
                        mvOrigin = new Vector2( UISheet.Style.Font.MeasureString( mstrText ).X / 2f, 0f );
                        break;
                    case TextAnchor.End:
                        mvOrigin = new Vector2( UISheet.Style.Font.MeasureString( mstrText ).X, 0f );
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
                    mvOrigin = new Vector2( UISheet.Style.Font.MeasureString( Text ).X / 2f, 0f );
                }
            }
        }

        TextAnchor      mAnchor;

        Vector2         mvPosition;
        Vector2         mvOrigin;
        float           mfScale;
        Color           mColor;
    }
}
