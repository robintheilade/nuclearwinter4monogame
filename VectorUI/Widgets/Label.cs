using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorUI.Widgets
{
    class Label: Widget
    {
        //----------------------------------------------------------------------
        public Label( UISheet _sheet, Text _text )
        : base( _text.Name, _sheet )
        {
            Text = UISheet.Game.GetUIString( _text.TextSpans[0].Value.Substring( "String".Length ) );

            //Vector2 vSize = UISheet.Font.MeasureString( Text );
            mvPosition = _text.Position - new Vector2( 0, /* FIXME: hard-coded stuff! */ 35 );
            mvOrigin = Vector2.Zero;

            if( _text.Anchor == TextAnchor.Middle )
            {
                mvOrigin = new Vector2( UISheet.Font.MeasureString( Text ).X / 2f, 0f );
            }

            mColor = _text.FillColor;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.DrawBlurredText( UISheet.Font, Text, mvPosition, mColor, mvOrigin, 1f );
        }

        //----------------------------------------------------------------------
        public string   Text;

        Vector2         mvPosition;
        Vector2         mvOrigin;
        Color           mColor;
    }
}
