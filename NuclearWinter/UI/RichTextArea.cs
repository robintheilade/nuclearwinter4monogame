using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

        public int EndOffset;

        //----------------------------------------------------------------------
        public bool HasSelection
        {
            get { return StartTextBlockIndex != EndTextBlockIndex || StartOffset != EndOffset; }
        }

        public bool HasLeftwardsSelection
        {
            get {
                return EndTextBlockIndex < StartTextBlockIndex || ( StartTextBlockIndex == EndTextBlockIndex && EndOffset < StartOffset );
            }
        }

        public bool HasRightwardsSelection
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
                /*Rectangle selectionRectangle;
                if( Caret.SelectionOffset > 0 )
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, LayoutRect.Y + Padding.Top, miSelectionX - miCaretX, LayoutRect.Height - Padding.Vertical );
                }
                else
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miSelectionX - miScrollOffset, LayoutRect.Y + Padding.Top, miCaretX - miSelectionX, LayoutRect.Height - Padding.Vertical );
                }

                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, selectionRectangle, Color.White * 0.3f );*/
            }
            else
            if( TextArea.Screen.IsActive && TextArea.HasFocus )
            {
                const float fBlinkInterval = 0.3f;
                const int iCaretWidth = 2;

                if( Timer % (fBlinkInterval * 2) < fBlinkInterval )
                {
                    Point caretPos = TextArea.TextBlocks[ StartTextBlockIndex ].GetXYForCaretOffset( StartOffset );
                    caretPos.X += TextArea.LayoutRect.X + TextArea.Padding.Left;
                    caretPos.Y += TextArea.LayoutRect.Y + TextArea.Padding.Top - (int)TextArea.Scrollbar.LerpOffset;

                    for( int iTextBlock = 0; iTextBlock < StartTextBlockIndex; iTextBlock++ )
                    {
                        TextBlock block = TextArea.TextBlocks[ iTextBlock ];
                        caretPos.Y += block.TotalHeight;
                    }

                    int iCaretHeight = TextArea.TextBlocks[ StartTextBlockIndex ].LineHeight;

                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, iCaretWidth, iCaretHeight ), TextArea.Screen.Style.DefaultTextColor );
                }
            }
        }

        internal void MoveTo( Point _point )
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
                    StartTextBlockIndex = iTextBlock;
                    break;
                }

                iTextBlock++;
            }

            if( iTextBlock == TextArea.TextBlocks.Count )
            {
                // Clicked after the last block
                // Setup caret to point to the last offset of the last block
                StartTextBlockIndex = TextArea.TextBlocks.Count - 1;
                StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].Text.Length;
                return;
            }

            MoveInsideBlock( _point.X, iBlockCaretY / TextArea.TextBlocks[ StartTextBlockIndex ].LineHeight );
        }

        internal void MoveInsideBlock( int _iX, int _iLine )
        {
            TextBlock caretBlock = TextArea.TextBlocks[ StartTextBlockIndex ];

            int iOffset = 0;
            int iBlockLineIndex = Math.Min( caretBlock.WrappedLines.Count - 1, _iLine );

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

            StartOffset = iOffset;
        }

        internal void MoveStart()
        {
            int iLine;
            TextArea.TextBlocks[ StartTextBlockIndex ].GetXYForCaretOffset( StartOffset, out iLine );
            MoveInsideBlock( 0, iLine );
        }

        internal void MoveEnd()
        {
            int iLine;
            TextArea.TextBlocks[ StartTextBlockIndex ].GetXYForCaretOffset( StartOffset, out iLine );
            MoveInsideBlock( int.MaxValue, iLine );
        }

        internal void MoveUp()
        {
            int iLine;
            Point caretPos = TextArea.TextBlocks[ StartTextBlockIndex ].GetXYForCaretOffset( StartOffset, out iLine );

            if( iLine > 0 )
            {
                MoveInsideBlock( caretPos.X, iLine- 1 );
            }
            else
            if( StartTextBlockIndex > 0 )
            {
                StartTextBlockIndex--;
                MoveInsideBlock( caretPos.X, TextArea.TextBlocks[ StartTextBlockIndex ].WrappedLines.Count - 1 );
            }
            else
            {
                StartOffset = 0;
            }
        }

        internal void MoveDown()
        {
            int iLine;
            Point caretPos = TextArea.TextBlocks[ StartTextBlockIndex ].GetXYForCaretOffset( StartOffset, out iLine );

            if( iLine < TextArea.TextBlocks[ StartTextBlockIndex ].WrappedLines.Count - 1 )
            {
                MoveInsideBlock( caretPos.X, iLine + 1 );
            }
            else
            if( StartTextBlockIndex < TextArea.TextBlocks.Count - 1 )
            {
                StartTextBlockIndex++;
                MoveInsideBlock( caretPos.X, 0 );
            }
            else
            {
                StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].Text.Length;
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
            int iActualWidth = _iWidth - IndentLevel * RichTextArea.IndentOffset;
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
                    return new Point( 0, iLine * LineHeight );
                }
                if( iOffset + strLine.Length < _iCaretOffset )
                {
                    iOffset += strLine.Length;
                }
                else
                {
                    _iLine = iLine;
                    return new Point( (int)Font.MeasureString( strLine.Substring( 0, _iCaretOffset - ( iOffset ) ) ).X + EffectiveIndentLevel * RichTextArea.IndentOffset, iLine * LineHeight );
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
        public Func<RichTextArea,int,int,int,bool>                  TextRemovedHandler;

        //----------------------------------------------------------------------
        public Scrollbar        Scrollbar           { get; private set; }

        bool                    mbIsDragging;

        //----------------------------------------------------------------------
        public RichTextArea( Screen _screen )
        : base( _screen )
        {
            Caret           = new Caret( this );
            TextBlocks       = new List<TextBlock>();
            TextBlocks.Add( new TextBlock( this, "" ) );

            Padding         = new Box(20);

            Scrollbar = new Scrollbar( this );
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

            Scrollbar.DoLayout();
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            DoScroll( -_iDelta / 120 * 50 );
        }

        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -Scrollbar.Offset, Math.Max( 0, Scrollbar.Max - Scrollbar.Offset ) );
            Scrollbar.Offset += iScrollChange;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

            mbIsDragging = true;

            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );
            if( HasFocus && bShift )
            {
                //int iOffset = GetCaretOffset( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
                //Caret.SelectionOffset = iOffset - Caret.Offset;
            }
            else
            {
                Caret.MoveTo( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset ) );
            }

            Screen.Focus( this );
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            // TODO: call handler to ensure it's okay!

            if( Caret.EndTextBlockIndex > Caret.StartTextBlockIndex )
            {
                // Remove text from start block
                TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( Caret.StartOffset );

                // Remove text from end block
                TextBlocks[ Caret.EndTextBlockIndex ].Text.Remove( 0, Caret.EndOffset );

                // Delete blocks in between
                for( int i = Caret.EndTextBlockIndex - 1; i > Caret.StartTextBlockIndex; i-- )
                {
                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex + 1 );
                }
            }
            else
            if( Caret.EndTextBlockIndex < Caret.StartTextBlockIndex )
            {
                // Remove text from start block
                TextBlocks[ Caret.EndTextBlockIndex ].Text.Remove( Caret.EndOffset );

                // Remove text from end block
                TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( 0, Caret.StartOffset );

                // Delete blocks in between
                for( int i = Caret.EndTextBlockIndex + 1; i < Caret.StartTextBlockIndex; i-- )
                {
                    TextBlocks.RemoveAt( Caret.EndTextBlockIndex + 1 );
                }
            }
            else
            if( Caret.EndOffset > Caret.StartOffset )
            {

            }
            else
            if( Caret.EndOffset > Caret.StartOffset )
            {

            }

            /*if( SelectionOffset > 0 )
            {
                Text = Text.Remove( CaretOffset, SelectionOffset );
                SelectionOffset = 0;
            }
            else
            {
                int iNewCaretOffset = CaretOffset + SelectionOffset;
                Text = Text.Remove( CaretOffset + SelectionOffset, -SelectionOffset );
                CaretOffset = iNewCaretOffset;
            }*/
        }

        public void CopySelectionToClipboard()
        {
            /*
            if( SelectionOffset != 0 )
            {
                string strText;
                if( SelectionOffset > 0 )
                {
                    strText = Text.Substring( CaretOffset, SelectionOffset );
                }
                else
                {
                    strText = Text.Substring( CaretOffset + SelectionOffset, -SelectionOffset );
                }

                // NOTE: For this to work, you must put [STAThread] before your Main()
                System.Windows.Forms.Clipboard.SetText( strText );
            }
            */
        }

        public void PasteFromClipboard()
        {
            /*
            // NOTE: For this to work, you must put [STAThread] before your Main()
            string strPastedText = (string)System.Windows.Forms.Clipboard.GetData( typeof(string).FullName );
            if( strPastedText != null )
            {
                DeleteSelectedText();

                if( MaxLength != 0 && strPastedText.Length > MaxLength - Text.Length )
                {
                    strPastedText = strPastedText.Substring( 0, MaxLength - Text.Length );
                }

                Text = Text.Insert( CaretOffset, strPastedText );
                CaretOffset += strPastedText.Length;
            }
            */
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            /*CaretOffset = 0;
            SelectionOffset = Text.Length;
            mbIsDragging = false;*/
        }

        public void ClearSelection()
        {
            //SelectionOffset = 0;
        }

        //----------------------------------------------------------------------
        internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ! char.IsControl( _char ) )
            {
                if( Caret.HasSelection )
                {
                    DeleteSelectedText();
                }

                string strAddedText = _char.ToString();
                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, strAddedText ) )
                {
                    TextBlock textBlock = TextBlocks[ Caret.StartTextBlockIndex ];
                    textBlock.Text = textBlock.Text.Insert( Caret.StartOffset, strAddedText );
                    Caret.StartOffset++;
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
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset - 1, 1 ) )
                            {
                                Caret.StartOffset--;
                                TextBlocks[ Caret.StartTextBlockIndex ].Text = TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( Caret.StartOffset, 1 );
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
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, 1 ) )
                            {
                                TextBlocks[ Caret.StartTextBlockIndex ].Text = TextBlocks[ Caret.StartTextBlockIndex ].Text.Remove( Caret.StartOffset, 1 );
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
                            }
                        }
                    }
                    break;
                case Keys.Tab:
                    if( ! IsReadOnly )
                    {
                        int iNewIndentLevel = bShift ? Math.Max( 0, (int)TextBlocks[ Caret.StartTextBlockIndex ].IndentLevel - 1 ) : Math.Min( 4, (int)TextBlocks[ Caret.StartTextBlockIndex ].IndentLevel + 1 );
                        if( BlockIndentLevelChangedHandler == null || BlockIndentLevelChangedHandler( this, Caret.StartTextBlockIndex, iNewIndentLevel ) )
                        {
                            TextBlocks[ Caret.StartTextBlockIndex ].IndentLevel = iNewIndentLevel;
                            Caret.Timer = 0f;
                        }
                    }
                    break;
                case Keys.Left:
                    if( bShift )
                    {
                    }
                    else
                    {
                        int iNewStartTextBlockIndex = Caret.StartTextBlockIndex;
                        int iNewStartOffset = Caret.StartOffset - 1;

                        if( bCtrl )
                        {
                            if( iNewStartOffset > 0 )
                            {
                                iNewStartOffset = TextBlocks[ Caret.StartTextBlockIndex ].Text.LastIndexOf( ' ', iNewStartOffset - 1, iNewStartOffset - 1 ) + 1;
                            }
                            else
                            {

                            }
                        }
                        else
                        if( Caret.HasSelection )
                        {
                            if( Caret.HasLeftwardsSelection )
                            {
                                iNewStartOffset = Caret.EndOffset;
                                iNewStartTextBlockIndex = Caret.EndTextBlockIndex;
                            }
                            else
                            {
                                iNewStartOffset = Caret.StartOffset;
                            }
                        }

                        if( iNewStartOffset < 0 && iNewStartTextBlockIndex > 0 )
                        {
                            iNewStartTextBlockIndex--;
                            iNewStartOffset = TextBlocks[ iNewStartTextBlockIndex ].Text.Length;
                        }

                        Caret.StartTextBlockIndex = iNewStartTextBlockIndex;
                        Caret.StartOffset = iNewStartOffset;
                    }
                    break;
                case Keys.Right:
                    if( bShift )
                    {
                        if( bCtrl )
                        {
                            /*int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if( iNewSelectionTarget < Text.Length )
                            {
                                iNewSelectionTarget = Text.IndexOf( ' ', iNewSelectionTarget, Text.Length - iNewSelectionTarget ) + 1;

                                if( iNewSelectionTarget == 0 )
                                {
                                    iNewSelectionTarget = Text.Length;
                                }

                                SelectionOffset = iNewSelectionTarget - CaretOffset;
                            }*/
                        }
                        else
                        {
                            Caret.EndOffset++;
                        }
                    }
                    else
                    {
                        int iNewStartTextBlockIndex = Caret.StartTextBlockIndex;
                        int iNewStartOffset = Caret.StartOffset + 1;

                        if( bCtrl )
                        {
                            string strText = TextBlocks[ iNewStartTextBlockIndex ].Text;

                            if( iNewStartOffset < strText.Length )
                            {
                                iNewStartOffset = strText.IndexOf( ' ', iNewStartOffset, strText.Length - iNewStartOffset ) + 1;

                                if( iNewStartOffset == 0 )
                                {
                                    iNewStartOffset = strText.Length;
                                }
                            }
                        }
                        else
                        if( Caret.HasSelection )
                        {
                            if( Caret.HasRightwardsSelection )
                            {
                                iNewStartOffset = Caret.EndOffset;
                                iNewStartTextBlockIndex = Caret.EndTextBlockIndex;
                            }
                            else
                            {
                                iNewStartOffset = Caret.StartOffset;
                            }
                        }

                        if( iNewStartOffset > TextBlocks[ iNewStartTextBlockIndex ].Text.Length && iNewStartTextBlockIndex < TextBlocks.Count - 1 )
                        {
                            iNewStartTextBlockIndex++;
                            iNewStartOffset = 0;
                        }

                        Caret.StartTextBlockIndex = iNewStartTextBlockIndex;
                        Caret.StartOffset = iNewStartOffset;
                    }
                    break;
                case Keys.End:
                    if( bShift )
                    {
                        //SelectionOffset = Text.Length - CaretOffset;
                    }
                    else
                    {
                        Caret.MoveEnd();
                    }
                    break;
                case Keys.Home:
                    if( bShift )
                    {
                        //SelectionOffset = -CaretOffset;
                    }
                    else
                    {
                        Caret.MoveStart();
                    }
                    break;
                case Keys.Up:
                    Caret.MoveUp();
                    break;
                case Keys.Down:
                    Caret.MoveDown();
                    break;
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
            Screen.DrawBox( Screen.Style.ListFrame, LayoutRect, Screen.Style.PanelCornerSize, Color.White );

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
