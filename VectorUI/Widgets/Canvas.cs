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
    public class Canvas: Widget
    {
        //----------------------------------------------------------------------
        public Canvas( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            OnDraw( this );
        }

        //----------------------------------------------------------------------
        public Action<Widget>   OnDraw;
    }
}
