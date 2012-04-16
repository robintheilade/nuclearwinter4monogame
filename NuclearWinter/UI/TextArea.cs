using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class TextCaret
    {
        public int StartOffset
        {
            get { return miStartOffset; }
            set 
            {
                miStartOffset   = (int)MathHelper.Clamp( value, 0, TextArea.Text.Length );
                EndOffset       = StartOffset;
                Timer = 0f;
            }
        }

        public int EndOffset
        {
            get { return miEndOffset; }
            set 
            {
                miEndOffset = (int)MathHelper.Clamp( value, 0, TextArea.Text.Length );
            }
        }

        //----------------------------------------------------------------------
        public bool HasSelection
        {
            get { return StartOffset != EndOffset; }
        }

        public bool HasBackwardSelection
        {
            get {
                return EndOffset < StartOffset;
            }
        }

        public bool HasForwardSelection
        {
            get {
                return EndOffset > StartOffset;
            }
        }

        //----------------------------------------------------------------------
        public TextArea         TextArea { get; private set; }

        internal float          Timer;

        //----------------------------------------------------------------------
        int                     miStartOffset;
        int                     miEndOffset;

        //----------------------------------------------------------------------
        public TextCaret( TextArea _textArea )
        {
            TextArea = _textArea;
        }

        //----------------------------------------------------------------------
        internal void Update( float _fElapsedTime )
        {
            Timer = TextArea.HasFocus ? ( Timer + _fElapsedTime ) : 0f;
        }

        //----------------------------------------------------------------------
        internal void Draw()
        {
            if( HasSelection )
            {
                Point origin = new Point(
                    TextArea.LayoutRect.X + TextArea.Padding.Left,
                    TextArea.LayoutRect.Y + TextArea.Padding.Top - (int)TextArea.Scrollbar.LerpOffset
                    );

                //--------------------------------------------------------------
                // Start
                int iStartOffset        = HasForwardSelection ? StartOffset: EndOffset;

                int iStartBlockLine;
                Point startPos = TextArea.GetXYForCaretOffset( iStartOffset, out iStartBlockLine );

                int iStartCaretHeight = TextArea.LineHeight;

                //--------------------------------------------------------------
                // End
                int iEndOffset          = HasForwardSelection ? EndOffset: StartOffset;

                int iEndBlockLine;
                Point endPos = TextArea.GetXYForCaretOffset( iEndOffset, out iEndBlockLine );

                //--------------------------------------------------------------
                if( iStartBlockLine == iEndBlockLine )
                {
                    Rectangle selectionRectangle = new Rectangle( startPos.X, startPos.Y, endPos.X - startPos.X, iStartCaretHeight );
                    selectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );
                }
                else
                {
                    Rectangle startSelectionRectangle = new Rectangle( startPos.X, startPos.Y, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal - startPos.X, iStartCaretHeight );
                    startSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, startSelectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );

                    if( iEndBlockLine > iStartBlockLine + 1 )
                    {
                        Rectangle selectionRectangle = new Rectangle( 0, startPos.Y + iStartCaretHeight, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal, iStartCaretHeight * ( iEndBlockLine - iStartBlockLine - 1 ) );
                        selectionRectangle.Offset( origin );
                        TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );
                    }

                    int iEndCaretHeight = TextArea.LineHeight;

                    Rectangle endSelectionRectangle = new Rectangle( 0, endPos.Y, endPos.X, iEndCaretHeight );
                    endSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, endSelectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );
                }
            }

            if( TextArea.Screen.IsActive && TextArea.HasFocus )
            {
                const float fBlinkInterval = 0.3f;
                const int iCaretWidth = 2;

                if( Timer % (fBlinkInterval * 2) < fBlinkInterval )
                {
                    Point caretPos = TextArea.GetPositionForCaret( EndOffset );
                    int iCaretHeight = TextArea.LineHeight;

                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, iCaretWidth, iCaretHeight ), TextArea.Screen.Style.DefaultTextColor );
                }
            }
        }

        internal void SetCaretAt( Point _point, bool _bSelect )
        {
            Move( _point.X, _point.Y / TextArea.LineHeight, _bSelect );
        }

        internal void Move( int _iX, int _iLine, bool _bSelect )
        {
            int iOffset = 0;
            int iBlockLineIndex = Math.Max( 0, Math.Min( TextArea.WrappedLines.Count - 1, _iLine ) );

            for( int iLine = 0; iLine < iBlockLineIndex; iLine++ )
            {
                iOffset += TextArea.WrappedLines[ iLine ].Length;
            }

            int iOffsetInLine = ComputeCaretOffsetAtX( _iX, TextArea.WrappedLines[ iBlockLineIndex ], TextArea.Font );
            iOffset += iOffsetInLine;
            if( _iLine < TextArea.WrappedLines.Count - 1 && iOffsetInLine == TextArea.WrappedLines[ iBlockLineIndex ].Length )
            {
                iOffset--;
            }

            if( _bSelect )
            {
                EndOffset = iOffset;
            }
            else
            {
                StartOffset = iOffset;
            }
        }

        internal void MoveStart( bool _bSelect )
        {
            int iLine;
            TextArea.GetXYForCaretOffset( EndOffset, out iLine );
            Move( 0, iLine, _bSelect );
        }

        internal void MoveEnd( bool _bSelect )
        {
            int iLine;
            TextArea.GetXYForCaretOffset( EndOffset, out iLine );
            Move( int.MaxValue, iLine, _bSelect );
        }

        internal void MoveUp( bool _bSelect )
        {
            int iLine;
            Point caretPos = TextArea.GetXYForCaretOffset( EndOffset, out iLine );
            Move( caretPos.X, Math.Max( 0, iLine - 1 ), _bSelect );
        }

        internal void MoveDown( bool _bSelect )
        {
            int iLine;
            Point caretPos = TextArea.GetXYForCaretOffset( EndOffset, out iLine );
            Move( caretPos.X, Math.Min( TextArea.WrappedLines.Count - 1, iLine + 1 ), _bSelect );
        }

        int ComputeCaretOffsetAtX( int _iX, string _strText, UIFont _font )
        {
            // FIXME: This does many calls to Font.MeasureString
            // We should do a binary search instead!
            int iIndex = 0;
            float fPreviousX = 0f;

            while( iIndex < _strText.Length )
            {
                iIndex++;

                float fX = _font.MeasureString( _strText.Substring( 0, iIndex ) ).X;
                if( fX > _iX )
                {
                    bool bAfter = ( fX - _iX ) < ( ( fX - fPreviousX ) / 2f );
                    return bAfter ? iIndex : ( iIndex - 1 );
                }

                fPreviousX = fX;
            }

            return _strText.Length;
        }
    }

    //--------------------------------------------------------------------------
    public class TextArea: Widget
    {
        //----------------------------------------------------------------------
        public string           Text
        {
            get { return mstrText; }
            private set
            {
                mstrText = value;
                mbWrapTextNeeded = true;
            }
        }

        List<string> mlWrappedLines;
        public List<string> WrappedLines
        {
            get {
                if( mlWrappedLines == null )
                {
                    DoWrap( LayoutRect.Width - Padding.Horizontal );
                }
                return mlWrappedLines; 
            }
            private set {
                mlWrappedLines = value;
            }
        }

        public TextCaret        Caret               { get; private set; }
        public int              TabSpaces   = 4;

        public bool             IsReadOnly;

        // FIXME: Use EventHandler for those?
        public Func<TextArea,int,string,bool>           TextInsertedHandler;
        public Func<TextArea,int,int,bool>              TextRemovedHandler;

        //----------------------------------------------------------------------
        public UIFont           Font;
        public int              LineHeight  { get { return Font.LineSpacing; } }

        //----------------------------------------------------------------------
        public Scrollbar        Scrollbar           { get; private set; }

        public Texture2D        PanelTex;

        //----------------------------------------------------------------------
        string                  mstrText;
        bool                    mbWrapTextNeeded;

        bool                    mbIsDragging;
        bool                    mbScrollToCaret;

        //----------------------------------------------------------------------
        public TextArea( Screen _screen )
        : base( _screen )
        {
            Font            = Screen.Style.MediumFont;
            Text            = "";
            mbWrapTextNeeded = true;
            Caret           = new TextCaret( this );

            Padding         = new Box(20);

            Scrollbar       = new Scrollbar( this );
            PanelTex        = Screen.Style.ListFrame;
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle  _rect )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            if( mbWrapTextNeeded || ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height ) )
            {
                DoWrap( LayoutRect.Width - Padding.Horizontal );
            }

            ContentHeight = Padding.Vertical + LineHeight * WrappedLines.Count + LineHeight / 2;

            if( mbScrollToCaret )
            {
                Point caretPos = GetPositionForCaret( Caret.EndOffset );
                int iCaretHeight = LineHeight;

                if( caretPos.Y < LayoutRect.Top + 20 )
                {
                    Scrollbar.Offset += caretPos.Y - ( LayoutRect.Top + 20 );
                }
                else
                if( caretPos.Y > LayoutRect.Bottom - 20 - iCaretHeight )
                {
                    Scrollbar.Offset += caretPos.Y - ( LayoutRect.Bottom - 20 - iCaretHeight );
                }

                mbScrollToCaret = false;
            }

            Scrollbar.DoLayout( LayoutRect, ContentHeight );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            if( ( _iDelta < 0 && Scrollbar.Offset >= Scrollbar.Max )
             || ( _iDelta > 0 && Scrollbar.Offset <= 0 ) )
            {
                base.OnMouseWheel( _hitPoint, _iDelta );
                return;
            }

            DoScroll( -_iDelta / 120 * 50 );
        }

        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -Scrollbar.Offset, Math.Max( 0, Scrollbar.Max - Scrollbar.Offset ) );
            Scrollbar.Offset += iScrollChange;
        }

        void SetCaretPosition( Point _hitPoint, bool _bSelect )
        {
            Caret.SetCaretAt( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset ), _bSelect );
        }

        internal Point GetPositionForCaret( int _iOffset )
        {
            Point caretPos = GetXYForCaretOffset( _iOffset );
            caretPos.X += LayoutRect.X + Padding.Left;
            caretPos.Y += LayoutRect.Y + Padding.Top - (int)Scrollbar.LerpOffset;

            return caretPos;
        }

        internal Point GetXYForCaretOffset( int _iCaretOffset )
        {
            int iLine;
            return GetXYForCaretOffset( _iCaretOffset, out iLine );
        }

        internal Point GetXYForCaretOffset( int _iCaretOffset, out int _iLine )
        {
            int iOffset = 0;
            int iLine = 0;
            foreach( string strLine in WrappedLines )
            {
                if( iOffset + strLine.Length == _iCaretOffset && iLine < WrappedLines.Count - 1 )
                {
                    iLine++;
                    _iLine = iLine;
                    return new Point( 0, iLine * LineHeight );
                }

                if( iOffset + strLine.Length < _iCaretOffset )
                {
                    iOffset += strLine.Length;
                }
                else
                {
                    _iLine = iLine;
                    return new Point( (int)Font.MeasureString( strLine.Substring( 0, _iCaretOffset - iOffset ) ).X, iLine * LineHeight );
                }

                iLine++;
            }

            _iLine = iLine;
            return Point.Zero;
        }

        //----------------------------------------------------------------------
        public void DoWrap( int _iWidth )
        {
            int iActualWidth = _iWidth;
            mlWrappedLines = Screen.Game.WrapText( Font, Text, iActualWidth );
            mbWrapTextNeeded = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            mbIsDragging = true;

            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );
            SetCaretPosition( _hitPoint, bShift );

            Screen.Focus( this );
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
            }
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
                mbIsDragging = false;
            }
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            if( ! Caret.HasSelection || IsReadOnly ) return;

            bool bHasForwardSelection = Caret.HasForwardSelection;
            int iStartOffset        = bHasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
            int iEndOffset          = bHasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

            if( TextRemovedHandler == null || TextRemovedHandler( this, iStartOffset, iEndOffset ) )
            {
                // Merge start and end block
                Text = ( iStartOffset < Text.Length  ? Text.Remove( iStartOffset ) : "" ) + Text.Substring( iEndOffset );

                // Clear selection
                Caret.StartOffset = iStartOffset;

                mbScrollToCaret = true;
            }
        }

        public void CopySelectionToClipboard()
        {
            if( Caret.HasSelection )
            {
                bool bHasForwardSelection = Caret.HasForwardSelection;
                int iStartOffset        = bHasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
                int iEndOffset          = bHasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

                string strText = "";
                strText += Text.Substring( iStartOffset, iEndOffset - iStartOffset );

                // NOTE: For this to work, you must put [STAThread] before your Main()

                // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
                System.Windows.Forms.Clipboard.SetText( strText ); //, System.Windows.Forms.TextDataFormat.Html );
            }
        }

        public void PasteFromClipboard()
        {
            if( IsReadOnly ) return;

            // NOTE: For this to work, you must put [STAThread] before your Main()

            // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
            string strPastedText = (string)System.Windows.Forms.Clipboard.GetText(); // System.Windows.Forms.TextDataFormat.Html );
            if( strPastedText != null )
            {
                DeleteSelectedText();

                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strPastedText ) )
                {
                    Text = Text.Insert( Caret.StartOffset, strPastedText );
                    Caret.StartOffset += strPastedText.Length;
                    mbScrollToCaret = true;
                }
            }
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            Caret.StartOffset = 0;
            Caret.EndOffset = Text.Length;

            mbIsDragging = false;
        }

        public void ClearSelection()
        {
            Caret.StartOffset = Caret.EndOffset;
        }

        //----------------------------------------------------------------------
        internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ! char.IsControl( _char ) )
            {
                DeleteSelectedText();

                string strAddedText = _char.ToString();
                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strAddedText ) )
                {
                    Text = Text.Insert( Caret.StartOffset, strAddedText );
                    Caret.StartOffset++;

                    mbScrollToCaret = true;
                }
            }
        }

        internal override void OnWindowsKeyPress( System.Windows.Forms.Keys _key )
        {
            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftControl, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightControl, true );
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );

            switch( _key )
            {
                case System.Windows.Forms.Keys.A:
                    if( bCtrl )
                    {
                        SelectAll();
                    }
                    break;
                case System.Windows.Forms.Keys.X:
                    if( bCtrl )
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case System.Windows.Forms.Keys.C:
                    if( bCtrl )
                    {
                        CopySelectionToClipboard();
                    }
                    break;
                case System.Windows.Forms.Keys.V:
                    if( bCtrl )
                    {
                        PasteFromClipboard();
                    }
                    break;
                case System.Windows.Forms.Keys.Enter:
                    if( ! IsReadOnly )
                    {
                        if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, "\n" ) )
                        {
                            Text = Text.Insert( Caret.StartOffset, "\n" );
                            Caret.StartOffset++;
                            mbScrollToCaret = true;
                        }
                    }
                    break;
                case System.Windows.Forms.Keys.Back:
                    if( ! IsReadOnly )
                    {
                        if( Caret.HasSelection )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.StartOffset > 0 )
                        {
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartOffset - 1, Caret.StartOffset ) )
                            {
                                Caret.StartOffset--;
                                Text = Text.Remove( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case System.Windows.Forms.Keys.Delete:
                    if( ! IsReadOnly )
                    {
                        if( Caret.HasSelection )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.StartOffset < Text.Length )
                        {
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartOffset, Caret.StartOffset + 1 ) )
                            {
                                Text = Text.Remove( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case System.Windows.Forms.Keys.Tab:
                    if( ! IsReadOnly )
                    {
                        int iLastLineBreak = Text.LastIndexOf( '\n', Math.Max( 0, Caret.StartOffset - 1 ) );

                        int iDistance = 0;
                        if( iLastLineBreak != -1 )
                        {
                            iDistance = Caret.StartOffset - iLastLineBreak - 1;
                        }

                        int iSpaces = TabSpaces - (iDistance % TabSpaces);
                        if( iSpaces == 0 ) iSpaces = TabSpaces;

                        string strTab = new String( ' ', iSpaces );

                        if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strTab ) )
                        {
                            Text = Text.Insert( Caret.StartOffset, strTab );
                            Caret.StartOffset += strTab.Length;
                            mbScrollToCaret = true;
                        }
                    }
                    break;
                case System.Windows.Forms.Keys.Left: {
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset ) - 1;

                    if( bCtrl )
                    {
                        // FIXME: This needs to be improved, the behavior isn't exactly right
                        if( iNewOffset > 0 && Text[ iNewOffset ] != '\n' )
                        {
                            int iPreviousSpaceOffset = Text.LastIndexOf( ' ', iNewOffset - 1, iNewOffset - 1 ) + 1;
                            int iPreviousLineBreakOffset = Text.LastIndexOf( '\n', iNewOffset - 1, iNewOffset - 1 ) + 1;

                            if( iPreviousSpaceOffset == 0 || iPreviousLineBreakOffset > iPreviousSpaceOffset )
                            {
                                iNewOffset = iPreviousLineBreakOffset;
                            }
                            else
                            {
                                iNewOffset = iPreviousSpaceOffset;
                            }
                        }
                    }
                    else
                    if( ! bShift && Caret.HasSelection )
                    {
                        if( Caret.HasBackwardSelection )
                        {
                            iNewOffset = Caret.EndOffset;
                        }
                        else
                        {
                            iNewOffset++;
                        }
                    }

                    if( bShift )
                    {
                        Caret.EndOffset             = iNewOffset;
                    }
                    else
                    {
                        Caret.StartOffset           = iNewOffset;
                    }

                    mbScrollToCaret = true;
                    break;
                }
                case System.Windows.Forms.Keys.Right: {
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset ) + 1;

                    if( bCtrl )
                    {
                        // FIXME: This needs to be improved, the behavior isn't exactly right
                        string strText = Text;

                        if( iNewOffset < strText.Length && Text[iNewOffset] != '\n' )
                        {
                            int iNextSpaceOffset = strText.IndexOf( ' ', iNewOffset, strText.Length - iNewOffset ) + 1;
                            int iNextLineBreakOffset = strText.IndexOf( '\n', iNewOffset, strText.Length - iNewOffset );

                            if( iNextSpaceOffset == 0 || iNextLineBreakOffset < iNextSpaceOffset )
                            {
                                iNewOffset = iNextLineBreakOffset;
                            }
                            else
                            {
                                iNewOffset = iNextSpaceOffset;
                            }

                            if( iNewOffset <= 0 )
                            {
                                iNewOffset = strText.Length;
                            }
                        }
                    }
                    else
                    if( ! bShift && Caret.HasSelection )
                    {
                        if( Caret.HasForwardSelection )
                        {
                            iNewOffset = Caret.EndOffset;
                        }
                        else
                        {
                            iNewOffset--;
                        }
                    }

                    if( bShift )
                    {
                        Caret.EndOffset             = iNewOffset;
                    }
                    else
                    {
                        Caret.StartOffset           = iNewOffset;
                    }

                    mbScrollToCaret = true;
                    break;
                }
                case System.Windows.Forms.Keys.End:
                    Caret.MoveEnd( bShift );
                    mbScrollToCaret = true;
                    break;
                case System.Windows.Forms.Keys.Home:
                    Caret.MoveStart( bShift );
                    mbScrollToCaret = true;
                    break;
                case System.Windows.Forms.Keys.Up:
                    Caret.MoveUp( bShift );
                    mbScrollToCaret = true;
                    break;
                case System.Windows.Forms.Keys.Down:
                    Caret.MoveDown( bShift );
                    mbScrollToCaret = true;
                    break;
                case System.Windows.Forms.Keys.PageUp: {
                    Point target = GetPositionForCaret( Caret.EndOffset );
                    target.Y -= LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                case System.Windows.Forms.Keys.PageDown: {
                    Point target = GetPositionForCaret( Caret.EndOffset );
                    target.Y += LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                default:
                    base.OnWindowsKeyPress( _key );
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnPadMove( Direction _direction )
        {
            return;
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            Caret.Update( _fElapsedTime );

            Scrollbar.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( PanelTex, LayoutRect, Screen.Style.PanelCornerSize, Color.White );

            //------------------------------------------------------------------
            // Text
            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10, LayoutRect.Width - 20, LayoutRect.Height - 20 ) );

            int iX = LayoutRect.X + Padding.Left;
            int iY = LayoutRect.Y + Padding.Top;

            foreach( string strText in WrappedLines )
            {
                Screen.Game.SpriteBatch.DrawString( Font, strText, new Vector2( iX, iY + Font.YOffset - Scrollbar.LerpOffset ), Screen.Style.DefaultTextColor );
                iY += Font.LineSpacing;
            }

            //------------------------------------------------------------------
            // Draw caret & selection
            Caret.Draw();

            Screen.PopScissorRectangle();

            Scrollbar.Draw();
        }

        public void ReplaceContent( string _strText )
        {
            Text = _strText;

            // Make sure caret is valid
            int iStartOffset    = Caret.StartOffset;
            int iEndOffset      = Caret.EndOffset;

            Caret.StartOffset   = iStartOffset;
            Caret.EndOffset     = iEndOffset;
        }
    }
}
