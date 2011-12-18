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

        public float            LerpOffset;

        public bool             ScrollToBottom;

        //----------------------------------------------------------------------
        int                     miMax;

        int                     miScrollbarHeight;
        int                     miScrollbarOffset;

        public Widget           Parent;

        public Rectangle        ScrollRect;

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
        public void DoLayout( Rectangle _rect, int _iContentHeight )
        {
            ScrollRect = _rect;

            bool bScrolledToBottom = ScrollToBottom && Offset >= Max;

            Max = Math.Max( 0, _iContentHeight - ScrollRect.Height );
            Offset = (int)MathHelper.Clamp( Offset, 0, Max );

            if( bScrolledToBottom )
            {
                Offset = Max;
            }

            miScrollbarHeight = (int)( ( ScrollRect.Height - 20 ) / ( (float)_iContentHeight / ( ScrollRect.Height - 20 ) ) );
            miScrollbarOffset = (int)( (float)LerpOffset / Max * (float)( ScrollRect.Height - 20 - miScrollbarHeight ) );
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            if( miMax > 0 )
            {
                Parent.Screen.DrawBox(
                    Parent.Screen.Style.VerticalScrollbar,
                    new Rectangle(
                        ScrollRect.Right - 5 - Parent.Screen.Style.VerticalScrollbar.Width / 2,
                        ScrollRect.Y + 10 + miScrollbarOffset,
                        Parent.Screen.Style.VerticalScrollbar.Width,
                        miScrollbarHeight ),
                    Parent.Screen.Style.VerticalScrollbarCornerSize,
                    Color.White );
            }
        }
    }
}
