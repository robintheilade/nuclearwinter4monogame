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
    public class Screen // FIXME: Inherits from Group!
    {
        //----------------------------------------------------------------------
        public NuclearGame  Game                { get; private set; }
        public bool         IsActive;

        public Style        Style               { get; private set; }

        public int          Width               { get; private set; }
        public int          Height              { get; private set; }
        public Rectangle    Bounds              { get { return new Rectangle( 0, 0, Width, Height ); } }

        public Group        Root                { get; private set; }

        public Widget       FocusedWidget       { get; private set; }
        bool                mbHasActivatedFocusedWidget;
        Widget              mClickedWidget;
        int                 miClickedWidgetMouseButton;
        Widget              mHoveredWidget;
        Point               mPreviousMouseHitPoint;

        int                 miIgnoreClickFrames;

        //----------------------------------------------------------------------
        public Screen( NuclearWinter.NuclearGame _game, Style _style, int _iWidth, int _iHeight )
        {
            Game    = _game;
            Style   = _style;
            Width   = _iWidth;
            Height  = _iHeight;

            Root    = new Group( this );
        }

        //----------------------------------------------------------------------
        public void Resize( int _iWidth, int _iHeight )
        {
            Width = _iWidth;
            Height = _iHeight;

            // This will prevent accidental clicks when maximizing the window
            miIgnoreClickFrames = 3;

            Root.DoLayout( new Rectangle( 0, 0, Width, Height ) );
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
            //------------------------------------------------------------------
            // Mouse buttons
            Point mouseHitPoint = new Point(
                (int)( Game.InputMgr.MouseState.X / Resolution.ScaleFactor ),
                (int)( ( Game.InputMgr.MouseState.Y - Game.GraphicsDevice.Viewport.Y ) / Resolution.ScaleFactor )
            );

            bool bHasMouseEvent = false;

            if( miIgnoreClickFrames == 0 )
            {
                for( int iButton = 0; iButton < 3; iButton++ )
                {
                    if( Game.InputMgr.WasMouseButtonJustPressed( iButton ) )
                    {
                        bHasMouseEvent = true;

                        if( mClickedWidget == null )
                        {
                            miClickedWidgetMouseButton = iButton;

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
                                mClickedWidget.OnMouseDown( mouseHitPoint, iButton );
                            }
                        }
                    }
                }

                if( Game.InputMgr.WasMouseJustDoubleClicked() )
                {
                    bHasMouseEvent = true;

                    Widget widget  = FocusedWidget == null ? null : FocusedWidget.HitTest( mouseHitPoint );
                    if( widget != null )
                    {
                        widget.OnMouseDoubleClick( mouseHitPoint );
                    }
                }
            }
            else
            {
                miIgnoreClickFrames--;
            }

            for( int iButton = 0; iButton < 3; iButton++ )
            {
                if( Game.InputMgr.WasMouseButtonJustReleased( iButton ) )
                {
                    bHasMouseEvent = true;

                    if( mClickedWidget != null && iButton == miClickedWidgetMouseButton )
                    {
                        mClickedWidget.OnMouseUp( mouseHitPoint, iButton );
                        mClickedWidget = null;
                    }
                }
            }
            
            if( ! bHasMouseEvent )
            {
                Widget hoveredWidget = ( FocusedWidget == null ? null : FocusedWidget.HitTest( mouseHitPoint ) ) ?? Root.HitTest( mouseHitPoint );

                if( mouseHitPoint != mPreviousMouseHitPoint )
                {
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
            }

            // Mouse wheel
            int iWheelDelta = Game.InputMgr.GetMouseWheelDelta();
            if( iWheelDelta != 0 )
            {
                Widget hoveredWidget = ( FocusedWidget == null ? null : FocusedWidget.HitTest( mouseHitPoint ) ) ?? Root.HitTest( mouseHitPoint );

                if( hoveredWidget != null )
                {
                    hoveredWidget.OnMouseWheel( mouseHitPoint, iWheelDelta );
                }
            }

            mPreviousMouseHitPoint = mouseHitPoint;

            //------------------------------------------------------------------
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
            Root.Update( _fElapsedTime );
            Root.DoLayout( new Rectangle( 0, 0, Width, Height ) );
        }

        //----------------------------------------------------------------------
        public void Focus( Widget _widget )
        {
            Debug.Assert( _widget.Screen == this );

            mbHasActivatedFocusedWidget = false;
            if( FocusedWidget != null && FocusedWidget != _widget )
            {
                FocusedWidget.OnBlur();
            }

            if( FocusedWidget != _widget )
            {
                FocusedWidget = _widget;
                FocusedWidget.OnFocus();
            }
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
        Stack<Rectangle> mlScissorRects = new Stack<Rectangle>();

        Rectangle IntersectRects( Rectangle _rect1, Rectangle _rect2 )
        {
            Rectangle rect = new Rectangle(
                Math.Max( _rect1.X, _rect2.X ),
                Math.Max( _rect1.Y, _rect2.Y ),
                0, 0 );

            rect.Width = Math.Min( _rect1.Right, _rect2.Right ) - rect.X;
            rect.Height = Math.Min( _rect1.Bottom, _rect2.Bottom ) - rect.Y;

            if( rect.Width < 0 )
            {
                rect.X += rect.Width;
                rect.Width = 0;
            }

            if( rect.Height < 0 )
            {
                rect.Y += rect.Height;
                rect.Height = 0;
            }

            return rect;
        }

        Rectangle TransformRect( Rectangle _rect, Matrix _matrix )
        {
            Vector2 vMin = new Vector2( _rect.X, _rect.Y );
            Vector2 vMax = new Vector2( _rect.Right, _rect.Bottom );
            vMin = Vector2.Transform( vMin, _matrix );
            vMax = Vector2.Transform( vMax, _matrix );
            Rectangle bounds = new Rectangle(
                (int)vMin.X,
                (int)vMin.Y,
                (int)( vMax.X - vMin.X  ),
                (int)( vMax.Y - vMin.Y )
                );

            return bounds;
        }

        public void PushScissorRectangle( Rectangle _scissorRect )
        {
            Rectangle rect = TransformRect( _scissorRect, Game.SpriteMatrix );
            rect.Offset( 0, NuclearWinter.Resolution.Viewport.Y );
            Rectangle parentRect = mlScissorRects.Count > 0 ? mlScissorRects.Peek() : Game.GraphicsDevice.Viewport.Bounds;

            mlScissorRects.Push( IntersectRects( IntersectRects( parentRect, rect ), Game.GraphicsDevice.Viewport.Bounds ) );

            SuspendBatch();
            Game.GraphicsDevice.ScissorRectangle = mlScissorRects.Peek();
            ResumeBatch();
        }

        public void PopScissorRectangle()
        {
            SuspendBatch();
            mlScissorRects.Pop();
            ResumeBatch();
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            Game.SpriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix );

            Root.Draw();

            if( FocusedWidget != null && IsActive )
            {
                FocusedWidget.DrawFocused();
            }

            Game.SpriteBatch.End();

            Debug.Assert( mlScissorRects.Count == 0, "Unbalanced calls to PushScissorRectangles" );
        }

        internal void SuspendBatch()
        {
            Game.SpriteBatch.End();
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        }

        internal void ResumeBatch()
        {
            Game.GraphicsDevice.RasterizerState = Game.ScissorRasterizerState;

            if( mlScissorRects.Count > 0 )
            {
                Game.GraphicsDevice.ScissorRectangle = mlScissorRects.Peek();
                Game.SpriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, Game.ScissorRasterizerState, null, Game.SpriteMatrix );
            }
            else
            {
                Game.SpriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix );
            }
        }
    }
}
