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
            Text = UISheet.Game.GetUIString( _text.TextSpans[0].Value.Substring( 6 ) );

            //Vector2 vSize = UISheet.Font.MeasureString( Text );
            mvPosition = _text.Position - new Vector2( 0, /* FIXME: hard-coded stuff! */ 35 );
            mColor = Color.Black; // FIXME: _text.Color
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.SpriteBatch.DrawString( UISheet.Font, Text, mvPosition, mColor );
        }

        //----------------------------------------------------------------------
        public string   Text;

        Vector2         mvPosition;
        Color           mColor;
    }
}
