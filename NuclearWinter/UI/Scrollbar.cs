using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public class Scrollbar
    {
        //----------------------------------------------------------------------

        public int              Offset;

        public int              Max
        {
            get { return miMax; }

            set
            {
                miMax = value;
                Offset = Math.Min( Offset, miMax );
            }
        }

        public float            LerpOffset  { get; private set; }

        public bool             ScrollToBottom;

        //----------------------------------------------------------------------
        int                     miMax;

        int                     miScrollbarHeight;
        int                     miScrollbarOffset;

        public Widget           Parent;

        //----------------------------------------------------------------------
        public Scrollbar( Widget _parent )
        {
            Parent = _parent;
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            float fLerpAmount = Math.Min( 1f, _fElapsedTime * NuclearGame.LerpMultiplier );
            LerpOffset = MathHelper.Lerp( LerpOffset, Offset, fLerpAmount );
            LerpOffset = Math.Min( LerpOffset, Max );
        }

        //----------------------------------------------------------------------
        public void DoLayout()
        {
            bool bScrolledToBottom = ScrollToBottom && Offset >= Max;

            Max = Math.Max( 0, Parent.ContentHeight - ( Parent.LayoutRect.Height - 20 ) + 5 );
            Offset = (int)MathHelper.Clamp( Offset, 0, Max );

            if( bScrolledToBottom )
            {
                Offset = Max;
            }

            miScrollbarHeight = (int)( ( Parent.LayoutRect.Height - 20 ) / ( (float)Parent.ContentHeight / ( Parent.LayoutRect.Height - 20 ) ) );
            miScrollbarOffset = (int)( (float)LerpOffset / Max * (float)( Parent.LayoutRect.Height - 20 - miScrollbarHeight ) );
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            if( miMax > 0 )
            {
                Parent.Screen.DrawBox(
                    Parent.Screen.Style.VerticalScrollbar,
                    new Rectangle(
                        Parent.LayoutRect.Right - 5 - Parent.Screen.Style.VerticalScrollbar.Width / 2,
                        Parent.LayoutRect.Y + 10 + miScrollbarOffset,
                        Parent.Screen.Style.VerticalScrollbar.Width,
                        miScrollbarHeight ),
                    Parent.Screen.Style.VerticalScrollbarCornerSize,
                    Color.White );
            }
        }
    }
}
