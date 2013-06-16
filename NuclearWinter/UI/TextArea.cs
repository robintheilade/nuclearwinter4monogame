using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

#if !MONOGAME
using OSKey = System.Windows.Forms.Keys;
#elif !MONOMAC
using OSKey = OpenTK.Input.Key;
#else
using OSKey = MonoMac.AppKit.NSKey;
#endif

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
        internal virtual void Update( float _fElapsedTime )
        {
            Timer = TextArea.HasFocus ? ( Timer + _fElapsedTime ) : 0f;
        }

        //----------------------------------------------------------------------
        protected void DrawSelection( Rectangle _rect, Color _color )
        {
            if( HasSelection )
            {
                Point origin = new Point( _rect.X, _rect.Y - (int)TextArea.Scrollbar.LerpOffset );

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
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, _color );
                }
                else
                {
                    Rectangle startSelectionRectangle = new Rectangle( startPos.X, startPos.Y, _rect.Width - startPos.X, iStartCaretHeight );
                    startSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, startSelectionRectangle, _color );

                    if( iEndBlockLine > iStartBlockLine + 1 )
                    {
                        Rectangle selectionRectangle = new Rectangle( 0, startPos.Y + iStartCaretHeight, _rect.Width, iStartCaretHeight * ( iEndBlockLine - iStartBlockLine - 1 ) );
                        selectionRectangle.Offset( origin );
                        TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, _color );
                    }

                    int iEndCaretHeight = TextArea.LineHeight;

                    Rectangle endSelectionRectangle = new Rectangle( 0, endPos.Y, endPos.X, iEndCaretHeight );
                    endSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, endSelectionRectangle, _color );
                }
            }
        }

        //----------------------------------------------------------------------
        internal virtual void Draw( Rectangle _rect )
        {
            DrawSelection( _rect, TextArea.Screen.Style.DefaultTextColor * 0.3f );

            if( TextArea.Screen.IsActive && TextArea.HasFocus )
            {
                const float fBlinkInterval = 0.3f;

                if( Timer % (fBlinkInterval * 2) < fBlinkInterval )
                {
                    Point caretPos = TextArea.GetPositionForCaret( EndOffset );
                    int iCaretHeight = TextArea.LineHeight;

                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, TextArea.Screen.Style.CaretWidth, iCaretHeight ), TextArea.Screen.Style.DefaultTextColor );
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
                iOffset += TextArea.WrappedLines[ iLine ].Item1.Length;
            }

            int iOffsetInLine = ComputeCaretOffsetAtX( _iX, TextArea.WrappedLines[ iBlockLineIndex ].Item1, TextArea.Style.Font );
            iOffset += iOffsetInLine;
            if( _iLine < TextArea.WrappedLines.Count - 1 && iOffsetInLine == TextArea.WrappedLines[ iBlockLineIndex ].Item1.Length )
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

            if( TextArea.CaretMovedHandler != null )
            {
                TextArea.CaretMovedHandler( this );
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
            // FIXME: This does many calls to Style.Font.MeasureString
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

        public void SetSelection( int _iStartOffset, int? _iEndOffset=null )
        {
            StartOffset = _iStartOffset;

            if( _iEndOffset.HasValue )
            {
                EndOffset = _iEndOffset.Value;
            }

            if( TextArea.CaretMovedHandler != null )
            {
                TextArea.CaretMovedHandler( this );
            }
        }
    }

    //--------------------------------------------------------------------------
    public class RemoteTextCaret: TextCaret
    {
        public Color CaretColor = Color.Black;

        string mstrLabelString;
        Vector2 mvLabelSize;

        bool mbIsHovered;
        float mfLabelAlphaTimer = 0f;
        float mfLabelAlpha = 0f;

        //----------------------------------------------------------------------
        public RemoteTextCaret( string _strLabel, TextArea _textArea ): base ( _textArea )
        {
            mstrLabelString = _strLabel;
            
            mvLabelSize = TextArea.Screen.Style.SmallFont.MeasureString( mstrLabelString );
        }

        //----------------------------------------------------------------------
        public void OnMouseMove( Point _point )
        {
            Point caretPos = TextArea.GetPositionForCaret( EndOffset );
            int iCaretHeight = TextArea.LineHeight;

            Rectangle hitBox = new Rectangle( caretPos.X - 5, caretPos.Y, 10, iCaretHeight );

            if( hitBox.Contains( _point ) )
            {
                mfLabelAlpha = 1.0f;
                mfLabelAlphaTimer = 3.0f;
                mbIsHovered = true;
            }
            else
            {
                mbIsHovered = false;
            }
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            if( ! mbIsHovered )
            {
                mfLabelAlphaTimer -= 0.05f;
                mfLabelAlpha = MathHelper.Clamp( mfLabelAlphaTimer, 0, 1.0f );
            }

            base.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void Draw( Rectangle _rect )
        {
            DrawSelection( _rect, CaretColor * 0.3f );

            Point caretPos = TextArea.GetPositionForCaret( EndOffset );
            int iCaretHeight = TextArea.LineHeight;

            // Caret
            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, TextArea.Screen.Style.CaretWidth, iCaretHeight ), CaretColor );

            // Label
            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X - 4, caretPos.Y - (int)mvLabelSize.Y, (int)mvLabelSize.X + 6, (int)mvLabelSize.Y ), CaretColor * mfLabelAlpha );
            TextArea.Screen.Game.SpriteBatch.DrawString( TextArea.Screen.Style.SmallFont, mstrLabelString, new Vector2( caretPos.X - 1, caretPos.Y - mvLabelSize.Y ), Color.White * mfLabelAlpha );
            
        }
    }

    //--------------------------------------------------------------------------
    public class TextArea: Widget
    {
        public class TextAreaStyle
        {
            public UIFont Font;
            public UIFont BoldFont;

            public Color CommentColor = new Color( 0, 128, 0 );

            public Color StringColor = new Color( 128, 128, 128 );
            public Color NumberColor = new Color( 255, 0, 0 );

            public Color KeywordColor = new Color( 0, 0, 255 );
            public bool KeywordBold = true;

            public Color CustomKeywordColor = new Color( 0, 0, 128 );
            public bool CustomKeywordBold = true;
            public string[] CustomKeywords = {};
        }

        public TextAreaStyle Style = new TextAreaStyle();

        //----------------------------------------------------------------------
        public string           Text
        {
            get { return mstrText; }
            private set
            {
                mstrText = value;
                mbWrapTextNeeded = true;

                miGutterWidth = 0;

                if( DisplayLineNumbers )
                {
                    UpdateGutterWidth();
                }
            }
        }

        bool mbDisplayLineNumbers;
        public bool             DisplayLineNumbers
        {
            get { return mbDisplayLineNumbers; }
            set {
                mbDisplayLineNumbers = value;
                UpdateGutterWidth();
            }
        }

        void UpdateGutterWidth()
        {
            int iLines = 0;
            for( int i = 0; i < mstrText.Length; i++ )
            {
                if( mstrText[i] == '\n' ) iLines++;
            }

            miGutterWidth = (int)Style.Font.MeasureString( ( iLines + 1 ).ToString() ).X + Padding.Right;
        }

        int                     miGutterWidth;

        //public bool             WrapLines;

        List<Tuple<string,bool>> mlWrappedLines;
        public List<Tuple<string,bool>> WrappedLines
        {
            get {
                if( mlWrappedLines == null )
                {
                    DoWrap( LayoutRect.Width - Padding.Horizontal - ( DisplayLineNumbers ? miGutterWidth : 0 ) );
                }
                return mlWrappedLines; 
            }
            private set {
                mlWrappedLines = value;
            }
        }

        public List<int> WrappedLineNumberMappings;

        public TextCaret        Caret               { get; private set; }
        public Dictionary<UInt16,RemoteTextCaret>
                                RemoteCaretsById    { get; private set; }
        public int              TabSpaces   = 4;

        public bool             IsReadOnly;

        // FIXME: Properly support generic syntax highlighting instead
        // This is just a quick hack
        public bool             EnableLuaHighlight;

        // FIXME: Use EventHandler for those?
        public Func<TextArea,int,string,bool>           TextInsertedHandler;
        public Func<TextArea,int,int,bool>              TextRemovedHandler;
        public Action<TextCaret>                        CaretMovedHandler;

        //----------------------------------------------------------------------
        public int              LineHeight  { get { return Style.Font.LineSpacing; } }

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
            Style.Font      = Screen.Style.ParagraphFont;
            Style.BoldFont  = Screen.Style.ParagraphFont;
            Text            = "";
            mbWrapTextNeeded = true;
            Caret           = new TextCaret( this );
            RemoteCaretsById = new Dictionary<UInt16,RemoteTextCaret>();

            Padding         = Screen.Style.TextAreaPadding;

            Scrollbar       = new Scrollbar( _screen );
            Scrollbar.Parent = this;

            PanelTex        = Screen.Style.TextAreaFrame;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            if( mbWrapTextNeeded || ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height ) )
            {
                DoWrap( LayoutRect.Width - Padding.Horizontal - ( DisplayLineNumbers ? miGutterWidth : 0 ) );
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
        public override Widget HitTest( Point _point )
        {
            return Scrollbar.HitTest( _point ) ?? base.HitTest( _point );
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            if( ( _iDelta < 0 && Scrollbar.Offset >= Scrollbar.Max )
             || ( _iDelta > 0 && Scrollbar.Offset <= 0 ) )
            {
                base.OnMouseWheel( _hitPoint, _iDelta );
                return;
            }

            DoScroll( -_iDelta / 120 * 50 );
        }

        //----------------------------------------------------------------------
        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -Scrollbar.Offset, Math.Max( 0, Scrollbar.Max - Scrollbar.Offset ) );
            Scrollbar.Offset += iScrollChange;
        }

        //----------------------------------------------------------------------
        public void ScrollToCaret(){
            mbScrollToCaret = true;
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.IBeam;
#endif

            base.OnMouseEnter( _hitPoint );
        }

        //----------------------------------------------------------------------
        public override void OnMouseOut( Point _hitPoint )
        {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.Default;
#endif

            base.OnMouseOut( _hitPoint );
        }

        //----------------------------------------------------------------------
        void SetCaretPosition( Point _hitPoint, bool _bSelect )
        {
            Caret.SetCaretAt( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left + miGutterWidth ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset ), _bSelect );
        }

        //----------------------------------------------------------------------
        internal Point GetPositionForCaret( int _iOffset )
        {
            Point caretPos = GetXYForCaretOffset( _iOffset );
            caretPos.X += LayoutRect.X + Padding.Left + miGutterWidth;
            caretPos.Y += LayoutRect.Y + Padding.Top - (int)Scrollbar.LerpOffset;

            return caretPos;
        }

        //----------------------------------------------------------------------
        internal Point GetXYForCaretOffset( int _iCaretOffset )
        {
            int iLine;
            return GetXYForCaretOffset( _iCaretOffset, out iLine );
        }

        //----------------------------------------------------------------------
        internal Point GetXYForCaretOffset( int _iCaretOffset, out int _iLine )
        {
            int iOffset = 0;
            int iLine = 0;
            foreach( Tuple<string,bool> lineTuple in WrappedLines )
            {
                if( iOffset + lineTuple.Item1.Length == _iCaretOffset && iLine < WrappedLines.Count - 1 )
                {
                    iLine++;
                    _iLine = iLine;
                    return new Point( 0, iLine * LineHeight );
                }

                if( iOffset + lineTuple.Item1.Length < _iCaretOffset )
                {
                    iOffset += lineTuple.Item1.Length;
                }
                else
                {
                    _iLine = iLine;
                    return new Point( (int)Style.Font.MeasureString( lineTuple.Item1.Substring( 0, _iCaretOffset - iOffset ) ).X, iLine * LineHeight );
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
            mlWrappedLines = Screen.Game.WrapText( Style.Font, Text, iActualWidth );
            mbWrapTextNeeded = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return false;

            mbIsDragging = true;

            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );
            SetCaretPosition( _hitPoint, bShift );

            Screen.Focus( this );

            return true;
        }

        //----------------------------------------------------------------------
        public override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
            }
            else
            {
                foreach( var remoteCaret in RemoteCaretsById.Values )
                {
                    remoteCaret.OnMouseMove( _hitPoint );
                }
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
                mbIsDragging = false;
            }
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDoubleClick( Point _hitPoint )
        {
            SetCaretPosition( _hitPoint, true );
            Caret.StartOffset = GetPreviousCaretStop( Caret.StartOffset );
            Caret.EndOffset = GetNextCaretStop( Caret.StartOffset, false );

            return true;
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
                // Merge start and end
                Text = ( iStartOffset < Text.Length ? Text.Remove( iStartOffset ) : "" ) + Text.Substring( iEndOffset );

                // Clear selection
                Caret.SetSelection( iStartOffset );

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

#if !MONOMAC
                // NOTE: For this to work, you must put [STAThread] before your Main()
                
                // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
                System.Windows.Forms.Clipboard.SetText( strText ); //, System.Windows.Forms.TextDataFormat.Html );
#else
                var pasteBoard = MonoMac.AppKit.NSPasteboard.GeneralPasteboard;
                pasteBoard.ClearContents();
                pasteBoard.SetStringForType( strText, MonoMac.AppKit.NSPasteboard.NSStringType );
#endif
            }
        }

        public void PasteFromClipboard()
        {
            if( IsReadOnly ) return;

#if !MONOMAC
            // NOTE: For this to work, you must put [STAThread] before your Main()
            
            // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
            // GetText( System.Windows.Forms.TextDataFormat.Html );
            string strPastedText = System.Windows.Forms.Clipboard.GetText();
#else
            var pasteBoard = MonoMac.AppKit.NSPasteboard.GeneralPasteboard;
            string strPastedText = pasteBoard.GetStringForType( MonoMac.AppKit.NSPasteboard.NSStringType );
#endif
            if( strPastedText != null )
            {
                strPastedText = strPastedText.Replace( "\r\n", "\n" ).Replace( "\t", new String( ' ', TabSpaces ) );
                DeleteSelectedText();

                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strPastedText ) )
                {
                    Text = Text.Insert( Caret.StartOffset, strPastedText );
                    Caret.SetSelection( Caret.StartOffset + strPastedText.Length );
                    mbScrollToCaret = true;
                }
            }
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            Caret.SetSelection( 0, Text.Length );

            mbIsDragging = false;
        }

        public void ClearSelection()
        {
            Caret.SetSelection( Caret.StartOffset, Caret.StartOffset );
        }

        //----------------------------------------------------------------------
        protected internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ! char.IsControl( _char ) )
            {
                DeleteSelectedText();

                string strAddedText = _char.ToString();
                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strAddedText ) )
                {
                    Text = Text.Insert( Caret.StartOffset, strAddedText );
                    Caret.SetSelection( Caret.StartOffset + 1 );

                    mbScrollToCaret = true;
                }
            }
        }

        protected internal override void OnOSKeyPress( OSKey _key )
        {
            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftControl, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightControl, true );
            bool bShortcutKey = Screen.Game.InputMgr.IsShortcutKeyDown();
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );

            switch( _key )
            {
                case OSKey.A:
                    if( bShortcutKey )
                    {
                        SelectAll();
                    }
                    break;
                case OSKey.X:
                    if( bShortcutKey )
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case OSKey.C:
                    if( bShortcutKey )
                    {
                        CopySelectionToClipboard();
                    }
                    break;
                case OSKey.V:
                    if( bShortcutKey )
                    {
                        PasteFromClipboard();
                    }
                    break;
#if !MONOMAC
                case OSKey.Enter:
#else
                case OSKey.Return:
#endif
                    if( ! IsReadOnly )
                    {
                        int iLineStartIndex = Text.LastIndexOf( '\n', Math.Max( 0, Caret.StartOffset - 1 ) ) + 1;
                        int iSpaces = 0;
                        while( iLineStartIndex + iSpaces < Text.Length && Text[ iLineStartIndex + iSpaces ] == ' ' ) iSpaces++;

                        string strNewLine = "\n" + new String( ' ', iSpaces );

                        if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartOffset, strNewLine  ) )
                        {
                            Text = Text.Insert( Caret.StartOffset, strNewLine );
                            Caret.SetSelection( Caret.StartOffset + strNewLine.Length );
                            mbScrollToCaret = true;
                        }
                    }
                    break;
#if !MONOMAC
                case OSKey.Back:
#else
                case OSKey.ForwardDelete:
#endif
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
                                Caret.SetSelection( Caret.StartOffset - 1 );
                                Text = Text.Remove( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case OSKey.Delete:
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
                case OSKey.Tab:
                    if( ! IsReadOnly && ! bCtrl )
                    {
                        if( Caret.HasSelection )
                        {
                            bool bWasForwardSelection = Caret.HasForwardSelection;
                            int iStartOffset = bWasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
                            int iEndOffset = bWasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

                            int iLastLineBreak = Text.LastIndexOf( '\n', Math.Max( 0, iStartOffset - 1 ) );

                            if( ! bShift )
                            {
                                string strTab = new String( ' ', TabSpaces );

                                do
                                {
                                    if( TextInsertedHandler == null || TextInsertedHandler( this, iLastLineBreak + 1, strTab ) )
                                    {
                                        Text = Text.Insert( iLastLineBreak + 1, strTab );

                                        if( iLastLineBreak + 1 <= iStartOffset )
                                        {
                                            iStartOffset += strTab.Length;
                                        }

                                        iEndOffset += strTab.Length;
                                    }

                                    iLastLineBreak = Text.IndexOf( '\n', iLastLineBreak + 1, Math.Max( 0, iEndOffset - iLastLineBreak - 2 ) );
                                }
                                while( iLastLineBreak != -1 && iLastLineBreak < iEndOffset );
                            }
                            else
                            {
                                do
                                {
                                    int iSpaces = 0;
                                    while( iLastLineBreak + 1 + iSpaces < Text.Length && iSpaces < TabSpaces && Text[ iLastLineBreak + 1 + iSpaces ] == ' ' ) iSpaces++;

                                    if( TextRemovedHandler == null || TextRemovedHandler( this, iLastLineBreak + 1, iLastLineBreak + 1 + iSpaces ) )
                                    {
                                        Text = Text.Remove( iLastLineBreak + 1, iSpaces );

                                        if( iLastLineBreak + 1 <= iStartOffset )
                                        {
                                            iStartOffset = Math.Max( iLastLineBreak + 1, iStartOffset - iSpaces );
                                        }

                                        iEndOffset -= iSpaces;
                                    }

                                    iLastLineBreak = Text.IndexOf( '\n', iLastLineBreak + 1, Math.Max( 0, iEndOffset - iLastLineBreak - 2 ) );
                                }
                                while( iLastLineBreak != -1 && iLastLineBreak < iEndOffset );
                            }

                            Caret.SetSelection( bWasForwardSelection ? iStartOffset : iEndOffset, bWasForwardSelection ? iEndOffset : iStartOffset );
                        }
                        else
                        {
                            int iLastLineBreak = Text.LastIndexOf( '\n', Math.Max( 0, Caret.StartOffset - 1 ) );

                            if( ! bShift )
                            {
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
                                    Caret.SetSelection( Caret.StartOffset + strTab.Length );
                                    mbScrollToCaret = true;
                                }
                            }
                            else
                            if( Caret.StartOffset > 0 )
                            {
                                int iStartIndex = Caret.StartOffset - iLastLineBreak - 1;

                                int iSpacesToNearestTabStop = ( iStartIndex % TabSpaces );
                                if( iSpacesToNearestTabStop == 0 ) iSpacesToNearestTabStop = TabSpaces;

                                int iSpaces = 0;
                                while( iLastLineBreak + iStartIndex - iSpaces >= 0 && iSpaces < iSpacesToNearestTabStop && Text[ iLastLineBreak + iStartIndex - iSpaces ] == ' ' ) iSpaces++;

                                if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartOffset - iSpaces, Caret.StartOffset ) )
                                {
                                    Text = Text.Remove( Caret.StartOffset - iSpaces, iSpaces );
                                    Caret.SetSelection( Caret.StartOffset - iSpaces );
                                    mbScrollToCaret = true;
                                }
                            }
                        }
                    }
                    break;
#if !MONOMAC
                case OSKey.Left: {
#else
                case OSKey.LeftArrow: {
#endif
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset ) - 1;

                    if( bCtrl )
                    {
                        iNewOffset = GetPreviousCaretStop( iNewOffset );
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
                        Caret.SetSelection( Caret.StartOffset, iNewOffset );
                    }
                    else
                    {
                        Caret.SetSelection( iNewOffset );
                    }

                    mbScrollToCaret = true;
                    break;
                }
#if !MONOMAC
                case OSKey.Right: {
#else
                case OSKey.RightArrow: {
#endif
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset );

                    if( bCtrl )
                    {
                        iNewOffset = GetNextCaretStop( iNewOffset );
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
                    else
                    {
                        iNewOffset++;
                    }

                    if( bShift )
                    {
                        Caret.SetSelection( Caret.StartOffset, iNewOffset );
                    }
                    else
                    {
                        Caret.SetSelection( iNewOffset );
                    }

                    mbScrollToCaret = true;
                    break;
                }
                case OSKey.End:
                    Caret.MoveEnd( bShift );
                    mbScrollToCaret = true;
                    break;
                case OSKey.Home:
                    Caret.MoveStart( bShift );
                    mbScrollToCaret = true;
                    break;
#if !MONOMAC
                case OSKey.Up:
#else
                case OSKey.UpArrow:
#endif
                    Caret.MoveUp( bShift );
                    mbScrollToCaret = true;
                    break;
#if !MONOMAC
                case OSKey.Down:
#else
                case OSKey.DownArrow:
#endif
                    Caret.MoveDown( bShift );
                    mbScrollToCaret = true;
                    break;
                case OSKey.PageUp: {
                    Point target = GetPositionForCaret( Caret.EndOffset );
                    target.Y -= LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                case OSKey.PageDown: {
                    Point target = GetPositionForCaret( Caret.EndOffset );
                    target.Y += LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                default:
                    base.OnOSKeyPress( _key );
                    break;
            }
        }

        //----------------------------------------------------------------------
        int GetPreviousCaretStop( int _iOffset )
        {
            if( _iOffset > 0 && mstrText[ _iOffset ] != '\n' )
            {
                int iPreviousWordOffset = _iOffset;
                char startChar = mstrText[ iPreviousWordOffset ];

                bool bStartInWhitespace = startChar == ' ';
                bool bStartInWord = ! bStartInWhitespace && TextManipulation.WordBoundaries.IndexOf( startChar ) == -1;

                while( iPreviousWordOffset > 0 )
                {
                    bool bIsLineBreak = mstrText[ iPreviousWordOffset - 1 ] == '\n';
                    bool bIsWhiteSpace = mstrText[ iPreviousWordOffset - 1 ] == ' ' || bIsLineBreak;
                    bool bIsInWord = ! bIsWhiteSpace && TextManipulation.WordBoundaries.IndexOf( mstrText[ iPreviousWordOffset - 1 ] ) == -1;

                    if( bStartInWord )
                    {
                        if( ! bIsInWord || bIsWhiteSpace )
                        {
                            break;
                        }
                    }
                    else
                    if( bStartInWhitespace )
                    {
                        if( bIsLineBreak )
                        {
                            break;
                        }

                        if( ! bIsWhiteSpace )
                        {
                            bStartInWhitespace = false;
                            bStartInWord = bIsInWord;
                        }
                    }
                    else
                    {
                        if( bIsInWord || bIsWhiteSpace ) break;
                    }

                    iPreviousWordOffset--;
                }

                _iOffset = iPreviousWordOffset;
            }

            return _iOffset;
        }

        //----------------------------------------------------------------------
        int GetNextCaretStop( int _iOffset, bool _bGrabNextWhiteSpace=true )
        {
            if( _iOffset < mstrText.Length )
            {
                char startChar = Text[ _iOffset ];

                _iOffset++;

                if( startChar != '\n' )
                {
                    bool bStartInWhitespace = startChar == ' ';
                    bool bStartInWord = ! bStartInWhitespace && TextManipulation.WordBoundaries.IndexOf( startChar ) == -1;
                                
                    while( _iOffset < mstrText.Length )
                    {
                        bool bIsLineBreak = mstrText[ _iOffset ] == '\n';
                        bool bIsWhiteSpace = mstrText[ _iOffset ] == ' ' || bIsLineBreak;
                        bool bIsInWord = ! bIsWhiteSpace && TextManipulation.WordBoundaries.IndexOf( mstrText[ _iOffset ] ) == -1;

                        if( bIsLineBreak || ( bIsWhiteSpace && ! bStartInWhitespace && ! _bGrabNextWhiteSpace ) ) break;

                        if( bStartInWord )
                        {
                            if( bIsWhiteSpace )
                            {
                                bStartInWhitespace = true;
                                bStartInWord = false;
                            }
                            else
                            if( ! bIsInWord )
                            {
                                break;
                            }
                        }
                        else
                        if( bStartInWhitespace )
                        {
                            if( bIsLineBreak )
                            {
                                break;
                            }

                            if( ! bIsWhiteSpace )
                            {
                                break;
                            }
                        }
                        else
                        {
                            if( bIsInWord || bIsWhiteSpace ) break;
                        }

                        _iOffset++;
                    }
                }
            }

            return _iOffset;
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove( Direction _direction )
        {
            return;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            Caret.Update( _fElapsedTime );

            foreach( var caret in RemoteCaretsById )
            {
                caret.Value.Update( _fElapsedTime );
            }

            Scrollbar.Update( _fElapsedTime );
        }

        enum HighlightType
        {
            None,
            QuotedString,
            DoubleQuotedString,
            Number,
            InlineComment,
            BlockComment
        }
        
        string[] LuaKeywords = { 
            "and",      "break",    "do",       "else",         "elseif",
            "end",      "false",    "for",      "function",     "if",
            "in",       "local",    "nil",      "not",          "or",
            "repeat",   "return",   "then",     "true",         "until",    "while"
        };

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( PanelTex, LayoutRect, Screen.Style.TextAreaFrameCornerSize, Color.White );
            if( DisplayLineNumbers )
            {
                Screen.DrawBox( Screen.Style.TextAreaGutterFrame, new Rectangle( LayoutRect.X, LayoutRect.Y, miGutterWidth + Padding.Left, LayoutRect.Height ), Screen.Style.TextAreaGutterCornerSize, Color.White );
            }

            //------------------------------------------------------------------
            // Text
            var rect = LayoutRect; rect.Inflate( -Screen.Style.TextAreaScissorOffset, -Screen.Style.TextAreaScissorOffset );
            Screen.PushScissorRectangle( rect );

            int iX = LayoutRect.X + Padding.Left + miGutterWidth;
            int iY = LayoutRect.Y + Padding.Top;

            int iLine = 1;
            bool bNewLine = true;

            HighlightType hlType = HighlightType.None;

            foreach( Tuple<string,bool> lineTuple in WrappedLines )
            {
                string strText = lineTuple.Item1;

                if( bNewLine )
                {
                    if( DisplayLineNumbers )
                    {
                        string strLineNumber = iLine.ToString();
                        int iWidth = (int)Style.Font.MeasureString( strLineNumber ).X;
                        Screen.Game.SpriteBatch.DrawString( Style.Font, strLineNumber, new Vector2( LayoutRect.X + Padding.Left - Padding.Right + miGutterWidth - iWidth, iY + Style.Font.YOffset - Scrollbar.LerpOffset ), Screen.Style.DefaultTextColor * 0.5f );
                    }

                    bNewLine = false;
                }

                if( ! EnableLuaHighlight )
                {
                    Screen.Game.SpriteBatch.DrawString( Style.Font, strText, new Vector2( iX, iY + Style.Font.YOffset - Scrollbar.LerpOffset ), Screen.Style.DefaultTextColor );
                }
                else
                {
                    var color = Screen.Style.DefaultTextColor;
                    var font = Style.Font;
                    int iXOffset = 0;
                    string strAccumulator = "";

                    int iOffset = 0;
                    bool bCharIsAlpha = false;

                    while( iOffset < strText.Length )
                    {
                        char newChar = strText[iOffset];

                        bool bPrevCharIsAlpha = bCharIsAlpha;
                        bCharIsAlpha = ( newChar >= 'A' && newChar <= 'Z' ) || ( newChar >= 'a' && newChar <= 'z' );

                        if( ! bCharIsAlpha || bCharIsAlpha != bPrevCharIsAlpha )
                        {
                            bool bAddCharAfter = true;

                            if( hlType == HighlightType.BlockComment )
                            {
                                if( newChar == ']' && ( iOffset > 0 ) && strText[iOffset - 1] == ']' )
                                {
                                    bAddCharAfter = false;
                                    hlType = HighlightType.None;
                                }
                            }
                            else
                            if( hlType == HighlightType.InlineComment )
                            {
                                
                            }
                            else
                            if( hlType == HighlightType.QuotedString )
                            {
                                if( newChar == '\''  && ( iOffset == 0 || strText[iOffset - 1] != '\\' ) )
                                {
                                    color = Style.StringColor;
                                    hlType = HighlightType.None;
                                    bAddCharAfter = false;
                                }
                            }
                            else
                            if( hlType == HighlightType.DoubleQuotedString )
                            {
                                if( newChar == '"'  && ( iOffset == 0 || strText[iOffset - 1] != '\\' ) )
                                {
                                    color = Style.StringColor;
                                    hlType = HighlightType.None;
                                    bAddCharAfter = false;
                                }
                            }
                            else
                            if( hlType == HighlightType.Number )
                            {
                                if( ! bCharIsAlpha && ! ( newChar >= '0' && newChar <= '9' ) )
                                {
                                    color = Style.NumberColor;
                                    hlType = HighlightType.None;
                                }
                            }
                            else
                            if( newChar == '\'' )
                            {
                                if( hlType == HighlightType.None )
                                {
                                    hlType = HighlightType.QuotedString;
                                }
                            }
                            else
                            if( newChar == '"' )
                            {
                                if( hlType == HighlightType.None )
                                {
                                    hlType = HighlightType.DoubleQuotedString;
                                }
                            }
                            else
                            if( ( newChar >= '0' && newChar <= '9' ) )
                            {
                                if( ! bPrevCharIsAlpha && hlType == HighlightType.None )
                                {
                                    hlType = HighlightType.Number;
                                }
                            }
                            else
                            if( newChar == '-' && ( iOffset < strText.Length - 1) && strText[iOffset + 1] == '-' )
                            {
                                if( iOffset < strText.Length - 3 && strText[iOffset + 2] == '[' && strText[iOffset + 3]  == '[' )
                                {
                                    hlType = HighlightType.BlockComment;
                                }
                                else
                                {
                                    hlType = HighlightType.InlineComment;
                                }
                            }
                            else
                            if( LuaKeywords.Contains( strAccumulator ) )
                            {
                                color = Style.KeywordColor;
                                font = Style.KeywordBold ? Style.BoldFont : Style.Font;
                            }
                            else
                            if( Style.CustomKeywords.Contains( strAccumulator ) )
                            {
                                color = Style.CustomKeywordColor;
                                font = Style.CustomKeywordBold ? Style.BoldFont : Style.Font;
                            }
                            else
                            {
                                color = Screen.Style.DefaultTextColor;
                            }

                            if( ! bAddCharAfter )
                            {
                                strAccumulator += newChar;
                            }

                            Screen.Game.SpriteBatch.DrawString( font, strAccumulator, new Vector2( iX + iXOffset, iY + Style.Font.YOffset - Scrollbar.LerpOffset ), color );
                            iXOffset += (int)font.MeasureString( strAccumulator ).X;

                            strAccumulator = "";
                            font = Style.Font;

                            if( bAddCharAfter )
                            {
                                strAccumulator += newChar;
                            }

                            switch( hlType )
                            {
                                case HighlightType.BlockComment:
                                case HighlightType.InlineComment:
                                    color = Style.CommentColor;
                                    break;

                                case HighlightType.QuotedString:
                                case HighlightType.DoubleQuotedString:
                                    color = Style.StringColor;
                                    break;

                                case HighlightType.Number:
                                    color = Style.NumberColor;
                                    break;

                                case HighlightType.None:
                                    color = Screen.Style.DefaultTextColor;
                                    break;
                            }
                        }
                        else
                        {
                            strAccumulator += newChar;
                        }

                        iOffset++;
                    }
                }

                iY += Style.Font.LineSpacing;

                if( lineTuple.Item2 )
                {
                    iLine++;
                    bNewLine = true;

                    if( hlType == HighlightType.InlineComment )
                    {
                        hlType = HighlightType.None;
                    }
                }
            }

            //------------------------------------------------------------------
            // Draw carets & selections
            foreach( var caret in RemoteCaretsById.Values )
            {
                caret.Draw( new Rectangle( LayoutRect.X + Padding.Left + miGutterWidth, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal - miGutterWidth, LayoutRect.Height - Padding.Vertical ) );
            }

            Caret.Draw( new Rectangle( LayoutRect.X + Padding.Left + miGutterWidth, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal - miGutterWidth, LayoutRect.Height - Padding.Vertical ) );

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
