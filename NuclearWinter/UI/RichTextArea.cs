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
    public class Caret
    {
        //----------------------------------------------------------------------
        public int StartTextBlockIndex
        {
            get { return miStartTextBlockIndex; }
            set
            {
                miStartTextBlockIndex = value;
                EndTextBlockIndex = miStartTextBlockIndex;
            }
        }

        public int StartOffset
        {
            get { return miStartOffset; }
            set 
            {
                miStartOffset = (int)MathHelper.Clamp( value, 0, TextArea.TextBlocks[ StartTextBlockIndex ].Text.Length );

                EndTextBlockIndex   = StartTextBlockIndex;
                EndOffset           = StartOffset;
                Timer = 0f;
            }
        }

        public int EndTextBlockIndex
        {
            get { return miEndTextBlockIndex; }
            set
            {
                miEndTextBlockIndex = value;
                Timer = 0f;

                if( TextBlockChangedHandler != null )
                {
                    TextBlockChangedHandler( this );
                }
            }
        }

        public int EndOffset
        {
            get { return miEndOffset; }
            set 
            {
                miEndOffset = (int)MathHelper.Clamp( value, 0, TextArea.TextBlocks[ EndTextBlockIndex ].Text.Length );
            }
        }

        //----------------------------------------------------------------------
        public bool HasSelection
        {
            get { return StartTextBlockIndex != EndTextBlockIndex || StartOffset != EndOffset; }
        }

        public bool HasBackwardSelection
        {
            get {
                return EndTextBlockIndex < StartTextBlockIndex || ( StartTextBlockIndex == EndTextBlockIndex && EndOffset < StartOffset );
            }
        }

        public bool HasForwardSelection
        {
            get {
                return EndTextBlockIndex > StartTextBlockIndex || ( StartTextBlockIndex == EndTextBlockIndex && EndOffset > StartOffset );
            }
        }

        //----------------------------------------------------------------------
        public RichTextArea     TextArea { get; private set; }
        public Action<Caret>    TextBlockChangedHandler;

        internal float          Timer;

        //----------------------------------------------------------------------
        int                     miStartTextBlockIndex;
        int                     miStartOffset;

        int                     miEndTextBlockIndex;
        int                     miEndOffset;

        //----------------------------------------------------------------------
        public Caret( RichTextArea _textArea )
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
                int iStartBlockIndex    = HasForwardSelection ? StartTextBlockIndex : EndTextBlockIndex;
                int iStartOffset        = HasForwardSelection ? StartOffset: EndOffset;

                int iStartBlockLine;
                Point startPos = TextArea.TextBlocks[ iStartBlockIndex ].GetXYForCaretOffset( iStartOffset, out iStartBlockLine );

                int iStartBlockY = 0;
                for( int iBlock = 0; iBlock < iStartBlockIndex; iBlock++ )
                {
                    iStartBlockY += TextArea.TextBlocks[ iBlock ].TotalHeight;
                }
                startPos.Y += iStartBlockY;

                int iStartCaretHeight = TextArea.TextBlocks[ iStartBlockIndex ].LineHeight;

                //--------------------------------------------------------------
                // End
                int iEndBlockIndex      = HasForwardSelection ? EndTextBlockIndex : StartTextBlockIndex;
                int iEndOffset          = HasForwardSelection ? EndOffset: StartOffset;

                int iEndBlockLine;
                Point endPos = TextArea.TextBlocks[ iEndBlockIndex ].GetXYForCaretOffset( iEndOffset, out iEndBlockLine );

                int iEndBlockY = 0;
                for( int iBlock = 0; iBlock < iEndBlockIndex; iBlock++ )
                {
                    iEndBlockY += TextArea.TextBlocks[ iBlock ].TotalHeight;
                }
                endPos.Y += iEndBlockY;

                //--------------------------------------------------------------
                if( iStartBlockIndex == iEndBlockIndex && iStartBlockLine == iEndBlockLine )
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

                    if( iStartBlockIndex == iEndBlockIndex )
                    {
                        if( iEndBlockLine > iStartBlockLine + 1 )
                        {
                            Rectangle selectionRectangle = new Rectangle( 0, startPos.Y + iStartCaretHeight, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal, iStartCaretHeight * ( iEndBlockLine - iStartBlockLine - 1 ) );
                            selectionRectangle.Offset( origin );
                            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );
                        }
                    }
                    else
                    {
                        int iBlockSelectionStartY   = startPos.Y + iStartCaretHeight;
                        int iBlockSelectionEndY     = endPos.Y;

                        Rectangle selectionRectangle = new Rectangle( 0, iBlockSelectionStartY, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal, iBlockSelectionEndY - iBlockSelectionStartY );
                        selectionRectangle.Offset( origin );
                        TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, TextArea.Screen.Style.DefaultTextColor * 0.3f );
                    }

                    int iEndCaretHeight = TextArea.TextBlocks[ iEndBlockIndex ].LineHeight;

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
                    Point caretPos = TextArea.GetPositionForCaret( EndTextBlockIndex, EndOffset );
                    int iCaretHeight = TextArea.TextBlocks[ EndTextBlockIndex ].LineHeight;

                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, iCaretWidth, iCaretHeight ), TextArea.Screen.Style.DefaultTextColor );
                }
            }
        }

        internal void MoveTo( Point _point, bool _bSelect )
        {
            int iTextBlock      = 0;
            int iBlockY         = 0;
            int iBlockCaretY    = 0;

            while( iTextBlock < TextArea.TextBlocks.Count )
            {
                TextBlock block = TextArea.TextBlocks[ iTextBlock ];

                iBlockCaretY = _point.Y - iBlockY;
                iBlockY += block.TotalHeight;

                if( _point.Y < iBlockY )
                {
                    if( _bSelect )
                    {
                        EndTextBlockIndex = iTextBlock;
                    }
                    else
                    {
                        StartTextBlockIndex = iTextBlock;
                    }
                    break;
                }

                iTextBlock++;
            }

            if( iTextBlock == TextArea.TextBlocks.Count )
            {
                // Clicked after the last block
                // Setup caret to point to the last offset of the last block

                if( _bSelect )
                {
                    EndTextBlockIndex = TextArea.TextBlocks.Count - 1;
                    EndOffset = TextArea.TextBlocks[ EndTextBlockIndex ].Text.Length;
                }
                else
                {
                    StartTextBlockIndex = TextArea.TextBlocks.Count - 1;
                    StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].Text.Length;
                }
                return;
            }

            MoveInsideBlock( _point.X, iBlockCaretY / TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ].LineHeight, _bSelect );
        }

        internal void MoveInsideBlock( int _iX, int _iLine, bool _bSelect )
        {
            TextBlock caretBlock = TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ];

            int iOffset = 0;
            int iBlockLineIndex = Math.Max( 0, Math.Min( caretBlock.WrappedLines.Count - 1, _iLine ) );

            for( int iLine = 0; iLine < iBlockLineIndex; iLine++ )
            {
                iOffset += caretBlock.WrappedLines[ iLine ].Length;
            }

            int iOffsetInLine = ComputeCaretOffsetAtX( _iX - caretBlock.EffectiveIndentLevel * RichTextArea.IndentOffset, caretBlock.WrappedLines[ iBlockLineIndex ], caretBlock.Font );
            iOffset += iOffsetInLine;
            if( _iLine < caretBlock.WrappedLines.Count - 1 && iOffsetInLine == caretBlock.WrappedLines[ iBlockLineIndex ].Length )
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
            TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );
            MoveInsideBlock( 0, iLine, _bSelect );
        }

        internal void MoveEnd( bool _bSelect )
        {
            int iLine;
            TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );
            MoveInsideBlock( int.MaxValue, iLine, _bSelect );
        }

        internal void MoveUp( bool _bSelect )
        {
            int iLine;
            Point caretPos = TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );

            if( iLine > 0 )
            {
                MoveInsideBlock( caretPos.X, iLine - 1, _bSelect );
            }
            else
            {
                if( _bSelect )
                {
                    if( EndTextBlockIndex > 0 )
                    {
                        EndTextBlockIndex--;
                        MoveInsideBlock( caretPos.X, TextArea.TextBlocks[ EndTextBlockIndex ].WrappedLines.Count - 1, _bSelect );
                    }
                    else
                    {
                        EndOffset = 0;
                    }
                }
                else
                {
                    if( StartTextBlockIndex > 0 )
                    {
                        StartTextBlockIndex--;
                        MoveInsideBlock( caretPos.X, TextArea.TextBlocks[ StartTextBlockIndex ].WrappedLines.Count - 1, _bSelect );
                    }
                    else
                    {
                        StartOffset = 0;
                    }
                }
            }
        }

        internal void MoveDown( bool _bSelect )
        {
            int iLine;
            Point caretPos = TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );

            if( iLine < TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ].WrappedLines.Count - 1 )
            {
                MoveInsideBlock( caretPos.X, iLine + 1, _bSelect );
            }
            else
            {
                if( _bSelect )
                {
                    if( EndTextBlockIndex < TextArea.TextBlocks.Count - 1 )
                    {
                        EndTextBlockIndex++;
                        MoveInsideBlock( caretPos.X, 0, _bSelect );
                    }
                    else
                    {
                        EndOffset = TextArea.TextBlocks[ EndTextBlockIndex ].Text.Length;
                    }
                }
                else
                {
                    if( StartTextBlockIndex < TextArea.TextBlocks.Count - 1 )
                    {
                        StartTextBlockIndex++;
                        MoveInsideBlock( caretPos.X, 0, _bSelect );
                    }
                    else
                    {
                        StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].Text.Length;
                    }
                }
            }
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
    public enum TextBlockType
    {
        Paragraph,
        Header,
        SubHeader,
        OrderedListItem,
        UnorderedListItem,

        Count
    }

    //--------------------------------------------------------------------------
    public enum SectionStyle
    {
        Normal,
        Bold,
        Italics,
        Underlined
    }

    //--------------------------------------------------------------------------
    public class BlockSection
    {
        public int              Start;
        public int              End;
        public SectionStyle     Style;
    }

    //--------------------------------------------------------------------------
    public class TextBlock
    {
        TextBlockType                mBlockType;
        public TextBlockType         BlockType
        {
            get {
                return mBlockType;
            }

            set {
                mBlockType = value;
                
                switch( mBlockType )
                {
                    case TextBlockType.Header:
                        Font = mTextArea.Screen.Style.ExtraLargeFont;
                        break;
                    case TextBlockType.SubHeader:
                        Font = mTextArea.Screen.Style.LargeFont;
                        break;
                    case TextBlockType.Paragraph:
                    case TextBlockType.OrderedListItem:
                    case TextBlockType.UnorderedListItem:
                        Font = mTextArea.Screen.Style.MediumFont;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                mlWrappedLines = null;
            }
        }

        public int                  IndentLevel;
        public int                  EffectiveIndentLevel
        {
            get { return IndentLevel + ( ( BlockType == TextBlockType.OrderedListItem || BlockType == TextBlockType.UnorderedListItem ) ? 1 : 0 ); }
        }

        string                      mstrText;

        public string               Text
        {
            get { return mstrText; }
            set {
                mstrText = value;
                mlWrappedLines = null;
            }
        }

        List<string> mlWrappedLines;
        public List<string> WrappedLines
        {
            get {
                if( mlWrappedLines == null )
                {
                    DoWrap( mTextArea.LayoutRect.Width - mTextArea.Padding.Horizontal );
                }
                return mlWrappedLines; 
            }
            private set {
                mlWrappedLines = value;
            }
        }

        public UIFont               Font        { get; private set; }
        public int                  LineHeight  { get { return Font.LineSpacing; } }
        public int                  TotalHeight { get { return LineHeight * WrappedLines.Count + LineHeight / 2; } }

        RichTextArea                mTextArea;

        public TextBlock( RichTextArea _textArea, string _strText, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            mTextArea   = _textArea;
            Text        = _strText;
            BlockType   = _lineType;
            IndentLevel = _iIndentLevel;
        }

        public void DoWrap( int _iWidth )
        {
            int iActualWidth = _iWidth - EffectiveIndentLevel * RichTextArea.IndentOffset;
            mlWrappedLines = mTextArea.Screen.Game.WrapText( Font, Text, iActualWidth );
        }

        public void Draw( int _iX, int _iY, int _iWidth )
        {
            int iIndentedX = _iX + EffectiveIndentLevel * RichTextArea.IndentOffset;

            switch( BlockType )
            {
                case TextBlockType.Header:
                    break;
                case TextBlockType.SubHeader:
                    break;
                case TextBlockType.Paragraph:
                    break;
                case TextBlockType.OrderedListItem:
                    string strItemPrefix = mTextArea.CurrentListIndex.ToString() + ". ";
                    mTextArea.Screen.Game.SpriteBatch.DrawString( Font, strItemPrefix, new Vector2( iIndentedX - Font.MeasureString( strItemPrefix ).X, _iY + Font.YOffset ), mTextArea.Screen.Style.DefaultTextColor );
                    break;
                case TextBlockType.UnorderedListItem:
                    mTextArea.Screen.Game.SpriteBatch.DrawString( Font, "*", new Vector2( iIndentedX - Font.MeasureString( "* " ).X, _iY + Font.YOffset ), mTextArea.Screen.Style.DefaultTextColor );
                    break;
                default:
                    throw new NotSupportedException();
            }

            int iTextY = _iY;
            foreach( string strText in WrappedLines )
            {
                mTextArea.Screen.Game.SpriteBatch.DrawString( Font, strText, new Vector2( iIndentedX, iTextY + Font.YOffset ), mTextArea.Screen.Style.DefaultTextColor );
                iTextY += Font.LineSpacing;
            }
        }

        public List<BlockSection>    Sections;

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
                    return new Point( EffectiveIndentLevel * RichTextArea.IndentOffset, iLine * LineHeight );
                }

                if( iOffset + strLine.Length < _iCaretOffset )
                {
                    iOffset += strLine.Length;
                }
                else
                {
                    _iLine = iLine;
                    return new Point( (int)Font.MeasureString( strLine.Substring( 0, _iCaretOffset - iOffset ) ).X + EffectiveIndentLevel * RichTextArea.IndentOffset, iLine * LineHeight );
                }

                iLine++;
            }

            _iLine = iLine;
            return Point.Zero;
        }

    }

    //--------------------------------------------------------------------------
    public class RichTextArea: Widget
    {
        //----------------------------------------------------------------------
        public List<TextBlock>  TextBlocks          { get; private set; }
        public Caret            Caret               { get; private set; }

        public const int        IndentOffset        = 50;

        public bool             IsReadOnly;

        internal int            CurrentListIndex;

        // FIXME: Use EventHandler for those?
        public Func<RichTextArea,int,int,TextBlockType,int,bool>    BlockStartInsertedHandler;
        public Func<RichTextArea,int,bool>                          BlockStartRemovedHandler;
        public Func<RichTextArea,int,TextBlockType,bool>            BlockTypeChangedHandler;
        public Func<RichTextArea,int,int,bool>                      BlockIndentLevelChangedHandler;

        public Func<RichTextArea,int,int,string,bool>               TextInsertedHandler;
        public Func<RichTextArea,int,int,int,int,bool>              TextRemovedHandler;

        //----------------------------------------------------------------------
        public Scrollbar        Scrollbar           { get; private set; }

        bool                    mbIsDragging;
        bool                    mbScrollToCaret;

        public Texture2D        PanelTex;

        //----------------------------------------------------------------------
        public RichTextArea( Screen _screen )
        : base( _screen )
        {
            Caret           = new Caret( this );
            TextBlocks       = new List<TextBlock>();
            TextBlocks.Add( new TextBlock( this, "" ) );

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

            bool bWrapTextNeeded = ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height );

            ContentHeight = 0;

            foreach( TextBlock textBlock in TextBlocks )
            {
                if( bWrapTextNeeded )
                {
                    textBlock.DoWrap( LayoutRect.Width - Padding.Horizontal );
                }
                ContentHeight += textBlock.TotalHeight;
            }

            if( mbScrollToCaret )
            {
                Point caretPos = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
                int iCaretHeight = TextBlocks[ Caret.EndTextBlockIndex ].LineHeight;

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

            Scrollbar.DoLayout();

            ContentHeight += Padding.Vertical;
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
            Caret.MoveTo( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset ), _bSelect );
        }

        internal Point GetPositionForCaret( int _iTextBlockIndex, int _iOffset )
        {
            Point caretPos = TextBlocks[ _iTextBlockIndex ].GetXYForCaretOffset( _iOffset );
            caretPos.X += LayoutRect.X + Padding.Left;
            caretPos.Y += LayoutRect.Y + Padding.Top - (int)Scrollbar.LerpOffset;

            for( int iTextBlock = 0; iTextBlock < _iTextBlockIndex; iTextBlock++ )
            {
                TextBlock block = TextBlocks[ iTextBlock ];
                caretPos.Y += block.TotalHeight;
            }

            return caretPos;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

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
            if( _iButton != 0 ) return;

            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
                mbIsDragging = false;
            }
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            if( ! Caret.HasSelection ) return;

            bool bHasForwardSelection = Caret.HasForwardSelection;
            int iStartBlockIndex    = bHasForwardSelection ? Caret.StartTextBlockIndex : Caret.EndTextBlockIndex;
            int iStartOffset        = bHasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
            int iEndBlockIndex      = bHasForwardSelection ? Caret.EndTextBlockIndex : Caret.StartTextBlockIndex;
            int iEndOffset          = bHasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

            if( TextRemovedHandler == null || TextRemovedHandler( this, iStartBlockIndex, iStartOffset, iEndBlockIndex, iEndOffset ) )
            {
                // Merge start and end block
                TextBlocks[ iStartBlockIndex ].Text =
                    ( iStartOffset < TextBlocks[ iStartBlockIndex ].Text.Length  ? TextBlocks[ iStartBlockIndex ].Text.Remove( iStartOffset ) : "" )
                    + TextBlocks[ iEndBlockIndex ].Text.Substring( iEndOffset );

                // Delete blocks in between
                for( int i = iEndBlockIndex; i > iStartBlockIndex; i-- )
                {
                    TextBlocks.RemoveAt( iStartBlockIndex + 1 );
                }

                // Clear selection
                Caret.StartTextBlockIndex = iStartBlockIndex;
                Caret.StartOffset = iStartOffset;

                mbScrollToCaret = true;
            }
        }

        public void CopySelectionToClipboard()
        {
            if( Caret.HasSelection )
            {
                bool bHasForwardSelection = Caret.HasForwardSelection;
                int iStartBlockIndex    = bHasForwardSelection ? Caret.StartTextBlockIndex : Caret.EndTextBlockIndex;
                int iStartOffset        = bHasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
                int iEndBlockIndex      = bHasForwardSelection ? Caret.EndTextBlockIndex : Caret.StartTextBlockIndex;
                int iEndOffset          = bHasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

                string strText = "";

                if( iStartBlockIndex != iEndBlockIndex )
                {
                    strText += TextBlocks[ iStartBlockIndex ].Text.Substring( iStartOffset ) + "\n\n";

                    for( int i = iStartBlockIndex + 1; i < iEndBlockIndex; i++ )
                    {
                        strText += TextBlocks[ i ].Text + "\n\n";
                    }

                    strText += TextBlocks[ iEndBlockIndex ].Text.Substring( 0, iEndOffset );
                }
                else
                {
                    strText += TextBlocks[ iStartBlockIndex ].Text.Substring( iStartOffset, iEndOffset - iStartOffset );
                }

                // NOTE: For this to work, you must put [STAThread] before your Main()

                // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
                System.Windows.Forms.Clipboard.SetText( strText ); //, System.Windows.Forms.TextDataFormat.Html );
            }
        }

        public void PasteFromClipboard()
        {
            // NOTE: For this to work, you must put [STAThread] before your Main()

            // TODO: Add HTML support - http://msdn.microsoft.com/en-us/library/Aa767917.aspx#unknown_156
            string strPastedText = (string)System.Windows.Forms.Clipboard.GetText(); // System.Windows.Forms.TextDataFormat.Html );
            if( strPastedText != null )
            {
                DeleteSelectedText();

                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, strPastedText ) )
                {
                    TextBlocks[ Caret.StartTextBlockIndex ].Text = TextBlocks[ Caret.StartTextBlockIndex ].Text.Insert( Caret.StartOffset, strPastedText );
                    Caret.StartOffset += strPastedText.Length;
                    mbScrollToCaret = true;
                }
            }
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            Caret.StartTextBlockIndex = 0;
            Caret.StartOffset = 0;

            Caret.EndTextBlockIndex = TextBlocks.Count - 1;
            Caret.EndOffset = TextBlocks[ Caret.EndTextBlockIndex ].Text.Length;

            mbIsDragging = false;
        }

        public void ClearSelection()
        {
            Caret.StartTextBlockIndex = Caret.EndTextBlockIndex;
            Caret.StartOffset = Caret.EndOffset;
        }

        //----------------------------------------------------------------------
        internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ! char.IsControl( _char ) )
            {
                DeleteSelectedText();

                string strAddedText = _char.ToString();
                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, strAddedText ) )
                {
                    TextBlock textBlock = TextBlocks[ Caret.StartTextBlockIndex ];
                    textBlock.Text = textBlock.Text.Insert( Caret.StartOffset, strAddedText );
                    Caret.StartOffset++;

                    mbScrollToCaret = true;
                }
            }
        }

        internal override void OnKeyPress( Keys _key )
        {
            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftControl, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightControl, true );
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );

            switch( _key )
            {
                case Keys.A:
                    if( bCtrl )
                    {
                        SelectAll();
                    }
                    break;
                case Keys.X:
                    if( bCtrl )
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case Keys.C:
                    if( bCtrl )
                    {
                        CopySelectionToClipboard();
                    }
                    break;
                case Keys.V:
                    if( bCtrl )
                    {
                        PasteFromClipboard();
                    }
                    break;
                case Keys.Enter:
                    if( ! IsReadOnly )
                    {
                        TextBlock textBlock = TextBlocks[ Caret.StartTextBlockIndex ];

                        if( bShift )
                        {
                            if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, "\n" ) )
                            {
                                textBlock.Text = textBlock.Text.Insert( Caret.StartOffset, "\n" );
                                Caret.StartOffset++;
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        {
                            if( textBlock.Text.Length == 0 && textBlock.BlockType != TextBlockType.Paragraph )
                            {
                                if( BlockTypeChangedHandler == null || BlockTypeChangedHandler( this, Caret.StartTextBlockIndex, TextBlockType.Paragraph ) )
                                {
                                    if( BlockIndentLevelChangedHandler == null || BlockIndentLevelChangedHandler( this, Caret.StartTextBlockIndex, 0 ) )
                                    {
                                        textBlock.BlockType = TextBlockType.Paragraph;
                                        textBlock.IndentLevel = 0;
                                        Caret.StartTextBlockIndex = Caret.StartTextBlockIndex; // Trigger block change handler
                                        mbScrollToCaret = true;
                                    }
                                }
                                break;
                            }
                        
                            TextBlockType newBlockType  = TextBlockType.Paragraph;
                            int iNewBlockIndentLevel    = 0;

                            if( textBlock.Text.Length != 0 )
                            {
                                switch( textBlock.BlockType )
                                {
                                    case TextBlockType.Paragraph:
                                    case TextBlockType.OrderedListItem:
                                    case TextBlockType.UnorderedListItem:
                                        newBlockType = textBlock.BlockType;
                                        iNewBlockIndentLevel = textBlock.IndentLevel;
                                        break;
                                }
                            }

                            if( BlockStartInsertedHandler == null || BlockStartInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, newBlockType, iNewBlockIndentLevel ) )
                            {
                                if( Caret.StartOffset == 0 && textBlock.Text.Length > 0 )
                                {
                                    TextBlocks.Insert( Caret.StartTextBlockIndex, new TextBlock( this, "", newBlockType, iNewBlockIndentLevel ) );

                                    Caret.Timer = 0f;
                                }
                                else
                                {
                                    string strNewBlockText = "";
                                    if( Caret.StartOffset < textBlock.Text.Length )
                                    {
                                        strNewBlockText = textBlock.Text.Substring( Caret.StartOffset, textBlock.Text.Length - Caret.StartOffset );
                                        textBlock.Text = textBlock.Text.Remove( Caret.StartOffset );
                                    }

                                    TextBlocks.Insert( Caret.StartTextBlockIndex + 1, new TextBlock( this, strNewBlockText, newBlockType, iNewBlockIndentLevel ) );
                                    Caret.StartOffset = 0;
                                }

                                Caret.StartTextBlockIndex++;
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case Keys.Back:
                    if( ! IsReadOnly )
                    {
                        if( Caret.HasSelection )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.StartOffset > 0 )
                        {
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset - 1, Caret.StartTextBlockIndex, Caret.StartOffset ) )
                            {
                                Caret.StartOffset--;
                                TextBlocks[ Caret.StartTextBlockIndex ].Text = TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        if( Caret.StartTextBlockIndex > 0 )
                        {
                            int iNewCaretOffset = TextBlocks[ Caret.StartTextBlockIndex - 1 ].Text.Length;

                            if( BlockStartRemovedHandler == null || BlockStartRemovedHandler( this, Caret.StartTextBlockIndex ) )
                            {
                                if( iNewCaretOffset > 0 )
                                {
                                    Caret.StartTextBlockIndex--;
                                    TextBlocks[ Caret.StartTextBlockIndex ].Text += TextBlocks[ Caret.StartTextBlockIndex + 1 ].Text;
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex + 1 );
                                }
                                else
                                {
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex - 1 );
                                    Caret.StartTextBlockIndex--;
                                }

                                Caret.StartOffset = iNewCaretOffset;
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case Keys.Delete:
                    if( ! IsReadOnly )
                    {
                        if( Caret.HasSelection )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.StartOffset < TextBlocks[ Caret.StartTextBlockIndex ].Text.Length )
                        {
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, Caret.StartTextBlockIndex, Caret.StartOffset + 1 ) )
                            {
                                TextBlocks[ Caret.StartTextBlockIndex ].Text = TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        if( Caret.StartTextBlockIndex < TextBlocks.Count - 1 )
                        {
                            if( BlockStartRemovedHandler == null || BlockStartRemovedHandler( this, Caret.StartTextBlockIndex + 1 ) )
                            {
                                TextBlocks[ Caret.StartTextBlockIndex ].Text += TextBlocks[ Caret.StartTextBlockIndex + 1 ].Text;
                                TextBlocks.RemoveAt( Caret.StartTextBlockIndex + 1 );

                                Caret.Timer = 0f;
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case Keys.Tab:
                    if( ! IsReadOnly )
                    {
                        int iNewIndentLevel = bShift ? Math.Max( 0, (int)TextBlocks[ Caret.EndTextBlockIndex ].IndentLevel - 1 ) : Math.Min( 4, (int)TextBlocks[ Caret.EndTextBlockIndex ].IndentLevel + 1 );
                        if( BlockIndentLevelChangedHandler == null || BlockIndentLevelChangedHandler( this, Caret.EndTextBlockIndex, iNewIndentLevel ) )
                        {
                            TextBlocks[ Caret.EndTextBlockIndex ].IndentLevel = iNewIndentLevel;
                            Caret.Timer = 0f;
                            mbScrollToCaret = true;
                        }
                    }
                    break;
                case Keys.Left: {
                    int iNewTextBlockIndex  = bShift || bCtrl ? Caret.EndTextBlockIndex : Caret.StartTextBlockIndex;
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset ) - 1;

                    if( bCtrl )
                    {
                        if( iNewOffset > 0 )
                        {
                            iNewOffset = TextBlocks[ iNewTextBlockIndex ].Text.LastIndexOf( ' ', iNewOffset - 1, iNewOffset - 1 ) + 1;
                        }
                    }
                    else
                    if( ! bShift && Caret.HasSelection )
                    {
                        if( Caret.HasBackwardSelection )
                        {
                            iNewOffset = Caret.EndOffset;
                            iNewTextBlockIndex = Caret.EndTextBlockIndex;
                        }
                        else
                        {
                            iNewOffset++;
                        }
                    }

                    if( iNewOffset < 0 && iNewTextBlockIndex > 0 )
                    {
                        iNewTextBlockIndex--;
                        iNewOffset = TextBlocks[ iNewTextBlockIndex ].Text.Length;
                    }

                    if( bShift )
                    {
                        Caret.EndTextBlockIndex     = iNewTextBlockIndex;
                        Caret.EndOffset             = iNewOffset;
                    }
                    else
                    {
                        Caret.StartTextBlockIndex   = iNewTextBlockIndex;
                        Caret.StartOffset           = iNewOffset;
                    }

                    mbScrollToCaret = true;
                    break;
                }
                case Keys.Right: {
                    int iNewTextBlockIndex  = bShift || bCtrl ? Caret.EndTextBlockIndex : Caret.StartTextBlockIndex;
                    int iNewOffset          = ( bShift || bCtrl ? Caret.EndOffset : Caret.StartOffset ) + 1;

                    if( bCtrl )
                    {
                        string strText = TextBlocks[ iNewTextBlockIndex ].Text;

                        if( iNewOffset < strText.Length )
                        {
                            iNewOffset = strText.IndexOf( ' ', iNewOffset, strText.Length - iNewOffset ) + 1;

                            if( iNewOffset == 0 )
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
                            iNewTextBlockIndex = Caret.EndTextBlockIndex;
                        }
                        else
                        {
                            iNewOffset--;
                        }
                    }

                    if( iNewOffset > TextBlocks[ iNewTextBlockIndex ].Text.Length && iNewTextBlockIndex < TextBlocks.Count - 1 )
                    {
                        iNewTextBlockIndex++;
                        iNewOffset = 0;
                    }

                    if( bShift )
                    {
                        Caret.EndTextBlockIndex     = iNewTextBlockIndex;
                        Caret.EndOffset             = iNewOffset;
                    }
                    else
                    {
                        Caret.StartTextBlockIndex   = iNewTextBlockIndex;
                        Caret.StartOffset           = iNewOffset;
                    }

                    mbScrollToCaret = true;
                    break;
                }
                case Keys.End:
                    Caret.MoveEnd( bShift );
                    mbScrollToCaret = true;
                    break;
                case Keys.Home:
                    Caret.MoveStart( bShift );
                    mbScrollToCaret = true;
                    break;
                case Keys.Up:
                    Caret.MoveUp( bShift );
                    mbScrollToCaret = true;
                    break;
                case Keys.Down:
                    Caret.MoveDown( bShift );
                    mbScrollToCaret = true;
                    break;
                case Keys.PageUp: {
                    Point target = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
                    target.Y -= LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                case Keys.PageDown: {
                    Point target = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
                    target.Y += LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                default:
                    base.OnKeyPress( _key );
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

            Stack<int> lListIndices = new Stack<int>();
            CurrentListIndex = 0;

            for( int iBlock = 0; iBlock < TextBlocks.Count; iBlock++ )
            {
                TextBlock block = TextBlocks[iBlock];

                if( block.BlockType == TextBlockType.OrderedListItem )
                {
                    int iIndentDiff = 0;
                    if( CurrentListIndex != 0 )
                    {
                        iIndentDiff = block.IndentLevel - TextBlocks[iBlock - 1].IndentLevel;
                    }

                    if( iIndentDiff == 0 )
                    {
                        CurrentListIndex++;
                    }
                    else
                    if( iIndentDiff > 0 )
                    {
                        lListIndices.Push( CurrentListIndex );
                        CurrentListIndex = 1;
                    }
                    else
                    {
                        CurrentListIndex = lListIndices.Pop() + 1;
                    }
                }
                else
                {
                    CurrentListIndex = 0;
                }

                block.Draw( iX, iY - (int)Scrollbar.LerpOffset, LayoutRect.Width - Padding.Horizontal );
                iY += block.TotalHeight;
            }

            //------------------------------------------------------------------
            // Draw caret & selection
            Caret.Draw();

            Screen.PopScissorRectangle();

            Scrollbar.Draw();
        }

        public void ReplaceContent( List<TextBlock> _blocks )
        {
            TextBlocks = _blocks;
        }
    }
}
