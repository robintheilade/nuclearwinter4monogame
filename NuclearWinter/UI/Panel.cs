﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    /*
     * A widget that draws a box with the specified texture & corner size
     * Can also contain stuff
     */
    public class Panel: FixedGroup
    {
        public Texture2D        Texture;
        public int              CornerSize;

        public bool             DoClipping;

        bool mbEnableScrolling;
        public bool             EnableScrolling {
            get { return mbEnableScrolling; }
            set {
                mbEnableScrolling = value;
                AutoSize = mbEnableScrolling;
            }
        }

        public int              ScrollOffset        { get; private set; }
        int                     miScrollMax;
        int                     miScrollbarHeight;
        int                     miScrollbarOffset;
        float                   mfLerpScrollOffset;

        protected Box           mMargin;
        public Box              Margin
        {
            get { return mMargin; }

            set {
                mMargin = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public Panel( Screen _screen, Texture2D _texture, int _iCornerSize )
        : base( _screen )
        {
            Texture     = _texture;
            CornerSize  = _iCornerSize;
            Padding     = new Box( CornerSize );
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            if( EnableScrolling )
            {
                float fLerpAmount = Math.Min( 1f, _fElapsedTime * NuclearGame.LerpMultiplier );
                mfLerpScrollOffset = MathHelper.Lerp( mfLerpScrollOffset, ScrollOffset, fLerpAmount );
                mfLerpScrollOffset = Math.Min( mfLerpScrollOffset, miScrollMax );
            }

            base.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            LayoutRect = _rect;

            if( EnableScrolling )
            {
                int iHeight = ContentHeight;
                miScrollMax = Math.Max( 0, iHeight - ( LayoutRect.Height - 20 ) + 5 );
                ScrollOffset = Math.Min( ScrollOffset, miScrollMax );

                if( miScrollMax > 0 )
                {
                    miScrollbarHeight = (int)( ( LayoutRect.Height - 20 ) / ( (float)iHeight / ( LayoutRect.Height - 20 ) ) );
                    miScrollbarOffset = (int)( (float)mfLerpScrollOffset / miScrollMax * (float)( LayoutRect.Height - 20 - miScrollbarHeight ) );
                }
            }

            base.DoLayout( new Rectangle( LayoutRect.X + Padding.Left + Margin.Left, LayoutRect.Y + Padding.Top + Margin.Top - (int)mfLerpScrollOffset, LayoutRect.Width - Padding.Horizontal - Margin.Horizontal, LayoutRect.Height - Padding.Vertical - Margin.Vertical ) );

            HitBox = LayoutRect;
        }

        public override Widget HitTest( Point _point )
        {
            return base.HitTest( _point ) ?? ( EnableScrolling && HitBox.Contains( _point ) ? this : null );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            DoScroll( -_iDelta / 120 * 50 );
        }

        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -ScrollOffset, Math.Max( 0, miScrollMax - ScrollOffset ) );
            ScrollOffset += iScrollChange;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Texture, new Rectangle( LayoutRect.X + Margin.Left, LayoutRect.Y + Margin.Top, LayoutRect.Width - Margin.Horizontal, LayoutRect.Height - Margin.Vertical ), CornerSize, Color.White );

            if( DoClipping )
            {
                Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left + Margin.Left, LayoutRect.Y + Padding.Top + Margin.Top, LayoutRect.Width - Padding.Horizontal - Margin.Horizontal, LayoutRect.Height - Padding.Vertical - Margin.Vertical ) );
            }

            base.Draw();

            if( DoClipping )
            {
                Screen.PopScissorRectangle();
            }

            if( EnableScrolling && miScrollMax > 0 )
            {
                Screen.DrawBox( Screen.Style.VerticalScrollbar, new Rectangle( LayoutRect.Right - 5 - Screen.Style.VerticalScrollbar.Width / 2, LayoutRect.Y + 10 + miScrollbarOffset, Screen.Style.VerticalScrollbar.Width, miScrollbarHeight ), Screen.Style.VerticalScrollbarCornerSize, Color.White );
            }
        }
    }
}
