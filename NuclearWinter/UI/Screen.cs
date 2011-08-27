using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using NuclearWinter;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    /*
     * A Screen handles passing on user input, updating and drawing
     * a bunch of widgets
     */
    public class Screen
    {
        //----------------------------------------------------------------------
        public NuclearGame  Game                { get; private set; }
        public bool         IsActive;

        public Style        Style               { get; private set; }

        public int          Width               { get; private set; }
        public int          Height              { get; private set; }
        public Rectangle    Bounds              { get { return new Rectangle( 0, 0, Width, Height ); } }

        public FixedGroup   Root                { get; private set; }

        List<Widget>        mlUpdateList;

        public Widget       FocusedWidget       { get; private set; }
        bool                mbHasActivatedFocusedWidget;
        Widget              mClickedWidget;
        Widget              mHoveredWidget;

        //----------------------------------------------------------------------
        public Screen( NuclearWinter.NuclearGame _game, Style _style, int _iWidth, int _iHeight )
        {
            Game    = _game;
            Style   = _style;
            Width   = _iWidth;
            Height  = _iHeight;


            Root    = new FixedGroup( this );
            mlUpdateList = new List<Widget>();
        }

        //----------------------------------------------------------------------
        public void Resize( int _iWidth, int _iHeight )
        {
            Width = _iWidth;
            Height = _iHeight;
        }

        //----------------------------------------------------------------------
        public void AddWidgetToUpdateList( Widget _widget )
        {
            if( ! mlUpdateList.Contains( _widget ) )
            {
                mlUpdateList.Add( _widget );
            }
        }

        //----------------------------------------------------------------------
        public void HandleInput()
        {
            if( ! IsActive ) return;

            foreach( Buttons button in Enum.GetValues(typeof(Buttons)) )
            {
                PlayerIndex playerIndex;

                bool bPressed;

                if( Game.InputMgr.WasButtonJustPressed( button, Game.PlayerInCharge, out playerIndex, true ) )
                {
                    bPressed = true;
                }
                else
                if( Game.InputMgr.WasButtonJustReleased( button, Game.PlayerInCharge, out playerIndex ) )
                {
                    bPressed = false;
                }
                else
                {
                    continue;
                }

                switch( button )
                {
                    case Buttons.A:
                        if( FocusedWidget != null )
                        {
                            if( bPressed )
                            {
                                FocusedWidget.OnActivateDown();
                                mbHasActivatedFocusedWidget = true;
                            }
                            else
                            if( mbHasActivatedFocusedWidget )
                            {
                                FocusedWidget.OnActivateUp();
                                mbHasActivatedFocusedWidget = false;
                            }
                        }
                        break;
                    case Buttons.B:
                        if( FocusedWidget == null || ! FocusedWidget.OnCancel( bPressed ) )
                        {
                            Root.OnPadButton( button, bPressed );
                        }
                        break;
                    case Buttons.LeftThumbstickLeft:
                    case Buttons.DPadLeft:
                        if( bPressed && FocusedWidget != null ) FocusedWidget.OnPadMove( UI.Direction.Left );
                        break;
                    case Buttons.LeftThumbstickRight:
                    case Buttons.DPadRight:
                        if( bPressed && FocusedWidget != null ) FocusedWidget.OnPadMove( UI.Direction.Right );
                        break;
                    case Buttons.LeftThumbstickUp:
                    case Buttons.DPadUp:
                        if( bPressed && FocusedWidget != null ) FocusedWidget.OnPadMove( UI.Direction.Up );
                        break;
                    case Buttons.LeftThumbstickDown:
                    case Buttons.DPadDown:
                        if( bPressed && FocusedWidget != null ) FocusedWidget.OnPadMove( UI.Direction.Down );
                        break;
                    default:
                        Root.OnPadButton( button, bPressed );
                        break;
                }

            }

#if WINDOWS
            // Mouse buttons
            Point mouseHitPoint = new Point(
                (int)( Game.InputMgr.MouseState.X / Resolution.ScaleFactor ),
                (int)( ( Game.InputMgr.MouseState.Y - Game.GraphicsDevice.Viewport.Y ) / Resolution.ScaleFactor )
            );

            if( Game.InputMgr.WasMouseButtonJustPressed( 0 ) )
            {
                mClickedWidget = null;
                if( FocusedWidget != null )
                {
                    mClickedWidget = FocusedWidget.HitTest( mouseHitPoint );
                }

                if( mClickedWidget == null )
                {
                    mClickedWidget = Root.HitTest( mouseHitPoint );
                }

                if( mClickedWidget != null )
                {
                    mClickedWidget.OnMouseDown( mouseHitPoint );
                }
            }
            else
            if( Game.InputMgr.WasMouseButtonJustReleased( 0 ) )
            {
                if( mClickedWidget != null )
                {
                    mClickedWidget.OnMouseUp( mouseHitPoint );
                    mClickedWidget = null;
                }
            }
            else
            {
                Widget hoveredWidget = ( FocusedWidget == null ? null : FocusedWidget.HitTest( mouseHitPoint ) ) ?? Root.HitTest( mouseHitPoint );

                if( mClickedWidget == null )
                {
                    if( hoveredWidget != null && hoveredWidget == mHoveredWidget )
                    {
                        mHoveredWidget.OnMouseMove( mouseHitPoint );
                    }
                    else
                    {
                        if( mHoveredWidget != null )
                        {
                            mHoveredWidget.OnMouseOut( mouseHitPoint );
                        }
                    
                        mHoveredWidget = hoveredWidget;
                        if( mHoveredWidget != null )
                        {
                            mHoveredWidget.OnMouseEnter( mouseHitPoint );
                        }
                    }
                }
                else
                {
                    mClickedWidget.OnMouseMove( mouseHitPoint );
                }
            }

            // Mouse wheel
            int iWheelDelta = Game.InputMgr.GetMouseWheelDelta();
            if( iWheelDelta != 0 )
            {
                if( FocusedWidget != null )
                {
                    FocusedWidget.OnMouseWheel( iWheelDelta );
                }
            }

            // Keyboard
            if( FocusedWidget != null )
            {
                foreach( Keys key in Game.InputMgr.JustPressedKeys )
                {
                    FocusedWidget.OnKeyPress( key );
                }

                foreach( char character in Game.InputMgr.EnteredText )
                {
                    FocusedWidget.OnTextEntered( character );
                }
            }
#endif
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            Root.DoLayout( new Rectangle( 0, 0, Width, Height ) );

            List<Widget> lUpdateRemovedWidgets = new List<Widget>();
            foreach( Widget widget in mlUpdateList )
            {
                if( ! widget.Update( _fElapsedTime ) )
                {
                    lUpdateRemovedWidgets.Add( widget );
                }
            }

            foreach( Widget widget in lUpdateRemovedWidgets )
            {
                mlUpdateList.Remove( widget );
            }
        }

        //----------------------------------------------------------------------
        public void Focus( Widget _widget )
        {
            Debug.Assert( _widget.Screen == this );
            Debug.Assert( _widget.CanFocus );

            mbHasActivatedFocusedWidget = false;
            if( FocusedWidget != null && FocusedWidget != _widget )
            {
                FocusedWidget.OnBlur();
            }

            FocusedWidget = _widget;
            FocusedWidget.OnFocus();
        }

        //----------------------------------------------------------------------
        public void DrawBox( Texture2D _tex, Rectangle _extents, int _cornerSize, Color _color )
        {
            // Corners
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Top,                   _cornerSize, _cornerSize ), new Rectangle( 0,                           0,                          _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Top,                   _cornerSize, _cornerSize ), new Rectangle( _tex.Width - _cornerSize,    0,                          _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Bottom - _cornerSize,  _cornerSize, _cornerSize ), new Rectangle( 0,                           _tex.Height - _cornerSize,  _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Bottom - _cornerSize,  _cornerSize, _cornerSize ), new Rectangle( _tex.Width - _cornerSize,    _tex.Height - _cornerSize,  _cornerSize, _cornerSize ), _color );

            // Content
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Top + _cornerSize,     _extents.Width - _cornerSize * 2, _extents.Height - _cornerSize * 2 ), new Rectangle( _cornerSize, _cornerSize, _tex.Width - _cornerSize * 2, _tex.Height - _cornerSize * 2 ), _color );

            // Border top / bottom
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Top,                   _extents.Width - _cornerSize * 2, _cornerSize ), new Rectangle( _cornerSize, 0, _tex.Width - _cornerSize * 2, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Bottom - _cornerSize,  _extents.Width - _cornerSize * 2, _cornerSize ), new Rectangle( _cornerSize, _tex.Height - _cornerSize, _tex.Width - _cornerSize * 2, _cornerSize ), _color );

            // Border left / right
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Top + _cornerSize,     _cornerSize, _extents.Height - _cornerSize * 2 ), new Rectangle( 0, _cornerSize, _cornerSize, _tex.Height - _cornerSize * 2 ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Top + _cornerSize,     _cornerSize, _extents.Height - _cornerSize * 2 ), new Rectangle( _tex.Width - _cornerSize, _cornerSize, _cornerSize, _tex.Height - _cornerSize * 2 ), _color );
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            Root.Draw();

            if( FocusedWidget != null && IsActive )
            {
                FocusedWidget.DrawFocused();
            }
        }
    }
}
