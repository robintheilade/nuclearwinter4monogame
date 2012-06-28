using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    public class Slider: Widget
    {
        bool                    mbIsHovered;
        bool                    mbIsPressed;

        // FIXME: There should be a IntSlider/FloatSlider or a Discrete/Continuous setting for the Slider
        public int              MinValue;
        public int              MaxValue;
        public int              Step = 1;

        int                     miValue;
        public int              Value
        {
            get {
                return miValue;
            }

            set {
                miValue = (int)MathHelper.Clamp( value, MinValue, MaxValue );
                miValue -= miValue % Step;

                mTooltip.Text = miValue.ToString();
            }
        }

        public Action           ChangeHandler;

        Tooltip                 mTooltip;

        //----------------------------------------------------------------------
        public Slider( Screen _screen, int _iMin, int _iMax, int _iInitialValue, int _iStep )
        : base( _screen )
        {
            Debug.Assert( _iMin < _iMax );

            mTooltip    = new Tooltip( Screen, "" );

            MinValue    = _iMin;
            MaxValue    = _iMax;
            Value       = _iInitialValue;
            Step        = _iStep;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        protected internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
        }

        protected internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
            mTooltip.EnableDisplayTimer = false;
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            Screen.Focus( this );
            mbIsPressed = true;
            mTooltip.DisplayNow();

            int handleSize = LayoutRect.Height;
            Value = MinValue + (int)( ( MaxValue - MinValue ) * ( _hitPoint.X - LayoutRect.X - handleSize / 4 ) / ( LayoutRect.Width - handleSize ) );
            if( ChangeHandler != null ) ChangeHandler();
        }

        protected internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsPressed )
            {
                int handleSize = LayoutRect.Height;

                int iValue = MinValue + (int)( ( MaxValue - MinValue ) * ( _hitPoint.X - LayoutRect.X - handleSize / 4 ) / ( LayoutRect.Width - handleSize ) );

                if( iValue != miValue ) 
                {
                    Value = iValue;
                    if( ChangeHandler != null ) ChangeHandler();
                }
            }
        }

        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove(Direction _direction)
        {
            if( _direction == Direction.Left )
            {
                Value -= Step;
                if( ChangeHandler != null ) ChangeHandler();
            }
            else
            if( _direction == Direction.Right )
            {
                Value += Step;
                if( ChangeHandler != null ) ChangeHandler();
            }
            else
            {
                base.OnPadMove( _direction );
            }
        }

        //----------------------------------------------------------------------
        protected internal override void Update( float _fElapsedTime )
        {
            mTooltip.EnableDisplayTimer = mbIsHovered;
            mTooltip.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        protected internal override void Draw()
        {
            Rectangle rect = new Rectangle( LayoutRect.X, LayoutRect.Center.Y - Screen.Style.SliderHandleSize / 2, LayoutRect.Width, Screen.Style.SliderHandleSize );

            Screen.DrawBox( Screen.Style.ListFrame, rect, Screen.Style.GridBoxFrameCornerSize, Color.White );

            int handleX = rect.X + (int)( ( rect.Width - Screen.Style.SliderHandleSize ) * (float)( Value - MinValue ) / ( MaxValue - MinValue ) );

            Screen.DrawBox( (!mbIsPressed) ? Screen.Style.ButtonFrame : Screen.Style.ButtonFrameDown, new Rectangle( handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize ), Screen.Style.ButtonCornerSize, Color.White );
            if( Screen.IsActive && mbIsHovered && ! mbIsPressed )
            {
                Screen.DrawBox( Screen.Style.ButtonHover, new Rectangle( handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize ), Screen.Style.ButtonCornerSize, Color.White );
            }

            if( Screen.IsActive && HasFocus && ! mbIsPressed )
            {
                Screen.DrawBox( Screen.Style.ButtonFocus, new Rectangle( handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize ), Screen.Style.ButtonCornerSize, Color.White );
            }
        }

        //----------------------------------------------------------------------
        protected internal override void DrawHovered()
        {
            mTooltip.Draw();
        }
    }
}
