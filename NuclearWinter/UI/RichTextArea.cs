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
        public int          TextBlockIndex;

        public int          Offset
        {
            get { return miOffset; }
            set 
            {
                miOffset = (int)MathHelper.Clamp( value, 0, mTextArea.TextBlocks[ TextBlockIndex ].Text.Length );
                //miSelectionOffset = 0;
                mfTimer = 0f;
            }
        }

        public int          SelectionOffset;

        //----------------------------------------------------------------------
        RichTextArea        mTextArea;

        float               mfTimer;
        int                 miOffset;

        //----------------------------------------------------------------------
        public Caret( RichTextArea _textArea )
        {
            mTextArea = _textArea;
        }

        //----------------------------------------------------------------------
        internal void Update( float _fElapsedTime )
        {
            mfTimer = mTextArea.HasFocus ? ( mfTimer + _fElapsedTime ) : 0f;
        }

        //----------------------------------------------------------------------
        internal void Draw()
        {
            const float fBlinkInterval = 0.3f;
            const int iCaretWidth = 2;

            if( mfTimer % (fBlinkInterval * 2) < fBlinkInterval )
            {
                Point caretPos = mTextArea.TextBlocks[ TextBlockIndex ].GetXYForCaretOffset( Offset );
                caretPos.X += mTextArea.LayoutRect.X + mTextArea.Padding.Left;
                caretPos.Y += mTextArea.LayoutRect.Y + mTextArea.Padding.Top;

                for( int iTextBlock = 0; iTextBlock < TextBlockIndex; iTextBlock++ )
                {
                    TextBlock block = mTextArea.TextBlocks[ iTextBlock ];
                    caretPos.Y += block.TotalHeight;
                }

                int iCaretHeight = mTextArea.TextBlocks[ TextBlockIndex ].LineHeight;

                mTextArea.Screen.Game.SpriteBatch.Draw( mTextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, iCaretWidth, iCaretHeight ), mTextArea.Screen.Style.DefaultTextColor );
            }
        }

        internal void MoveTo( Point _point )
        {
            int iTextBlock      = 0;
            int iBlockY         = 0;
            int iBlockCaretY    = 0;

            while( iTextBlock < mTextArea.TextBlocks.Count )
            {
                TextBlock block = mTextArea.TextBlocks[ iTextBlock ];

                iBlockCaretY = _point.Y - iBlockY;
                iBlockY += block.TotalHeight;

                if( _point.Y < iBlockY )
                {
                    TextBlockIndex = iTextBlock;
                    break;
                }

                iTextBlock++;
            }

            if( iTextBlock == mTextArea.TextBlocks.Count )
            {
                // Clicked after the last block
                // Setup caret to point to the last offset of the last block
                TextBlockIndex = mTextArea.TextBlocks.Count - 1;
                Offset = mTextArea.TextBlocks[ TextBlockIndex ].Text.Length;
                return;
            }

            MoveInsideBlock( _point.X, iBlockCaretY / mTextArea.TextBlocks[ TextBlockIndex ].LineHeight );
        }

        internal void MoveInsideBlock( int _iX, int _iLine )
        {
            TextBlock caretBlock = mTextArea.TextBlocks[ TextBlockIndex ];

            int iOffset = 0;
            int iBlockLineIndex = Math.Min( caretBlock.WrappedLines.Count - 1, _iLine );

            for( int iLine = 0; iLine < iBlockLineIndex; iLine++ )
            {
                iOffset += caretBlock.WrappedLines[ iLine ].Length;
            }

            iOffset += ComputeCaretOffsetAtX( _iX - caretBlock.IndentLevel * RichTextArea.IndentOffset, caretBlock.WrappedLines[ iBlockLineIndex ], caretBlock.Font );
            if( _iLine < caretBlock.WrappedLines.Count - 1 && iOffset == caretBlock.WrappedLines[ iBlockLineIndex ].Length )
            {
                iOffset--;
            }

            Offset = iOffset;
        }

        internal void MoveStart()
        {
            int iLine;
            mTextArea.TextBlocks[ TextBlockIndex ].GetXYForCaretOffset( Offset, out iLine );
            MoveInsideBlock( 0, iLine );
        }

        internal void MoveEnd()
        {
            int iLine;
            mTextArea.TextBlocks[ TextBlockIndex ].GetXYForCaretOffset( Offset, out iLine );
            MoveInsideBlock( int.MaxValue, iLine );
        }

        internal void MoveUp()
        {
            int iLine;
            Point caretPos = mTextArea.TextBlocks[ TextBlockIndex ].GetXYForCaretOffset( Offset, out iLine );

            if( iLine > 0 )
            {
                MoveInsideBlock( caretPos.X, iLine- 1 );
            }
            else
            if( TextBlockIndex > 0 )
            {
                TextBlockIndex--;
                MoveInsideBlock( caretPos.X, mTextArea.TextBlocks[ TextBlockIndex ].WrappedLines.Count - 1 );
            }
            else
            {
                Offset = 0;
            }
        }

        internal void MoveDown()
        {
            int iLine;
            Point caretPos = mTextArea.TextBlocks[ TextBlockIndex ].GetXYForCaretOffset( Offset, out iLine );

            if( iLine < mTextArea.TextBlocks[ TextBlockIndex ].WrappedLines.Count - 1 )
            {
                MoveInsideBlock( caretPos.X, iLine + 1 );
            }
            else
            if( TextBlockIndex < mTextArea.TextBlocks.Count - 1 )
            {
                TextBlockIndex++;
                MoveInsideBlock( caretPos.X, 0 );
            }
            else
            {
                Offset = mTextArea.TextBlocks[ TextBlockIndex ].Text.Length;
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
        Header,
        SubHeader,
        Paragraph,
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
            }
        }

        public int                  IndentLevel;

        bool                        mbWrapNeeded;
        string                      mstrText;

        public string               Text
        {
            get { return mstrText; }
            set {
                mstrText = value;
                mbWrapNeeded = true;
            }
        }

        public List<string>         WrappedLines        { get; private set; }

        public UIFont               Font        { get; private set; }
        public int                  LineHeight  { get { return Font.LineSpacing; } }
        public int                  TotalHeight { get { return LineHeight * WrappedLines.Count + LineHeight / 2; } }

        RichTextArea                mTextArea;

        public TextBlock( RichTextArea _textArea, string _strText, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            mTextArea = _textArea;
            Text        = _strText;
            BlockType    = _lineType;
            IndentLevel = _iIndentLevel;
        }

        public void DoWrap( int _iWidth )
        {
            int iActualWidth = _iWidth - IndentLevel * RichTextArea.IndentOffset;

            WrappedLines = mTextArea.Screen.Game.WrapText( Font, Text, iActualWidth );
            mbWrapNeeded = false;
        }

        public void Draw( int _iX, int _iY, int _iWidth )
        {
            if( mbWrapNeeded )
            {
                DoWrap( _iWidth );
            }

            UIFont font;

            switch( BlockType )
            {
                case TextBlockType.Header:
                    font = mTextArea.Screen.Style.ExtraLargeFont;
                    break;
                case TextBlockType.SubHeader:
                    font = mTextArea.Screen.Style.LargeFont;
                    break;
                case TextBlockType.Paragraph:
                case TextBlockType.OrderedListItem:
                case TextBlockType.UnorderedListItem:
                    font = mTextArea.Screen.Style.MediumFont;
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            int iTextY = _iY;
            foreach( string strText in WrappedLines )
            {
                mTextArea.Screen.Game.SpriteBatch.DrawString( font, strText, new Vector2( _iX, iTextY + font.YOffset ), mTextArea.Screen.Style.DefaultTextColor );
                iTextY += font.LineSpacing;
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
                    return new Point( (int)Font.MeasureString( strLine.Substring( 0, _iCaretOffset - ( iOffset ) ) ).X + IndentLevel * RichTextArea.IndentOffset, iLine * LineHeight );
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

        //----------------------------------------------------------------------
        Vector2                 mvViewOffset;
        float                   mfZoom              = 1f;

        bool                    mbIsDragging;

        //----------------------------------------------------------------------
        public RichTextArea( Screen _screen )
        : base( _screen )
        {
            Caret           = new Caret( this );
            TextBlocks       = new List<TextBlock>();
            TextBlocks.Add( new TextBlock( this, "" ) );

            Padding         = new Box(20);
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle  _rect )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            bool bWrapTextNeeded = ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height );

            if( bWrapTextNeeded )
            {
                foreach( TextBlock line in TextBlocks )
                {
                    line.DoWrap( LayoutRect.Width - Padding.Horizontal );
                }
            }

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
                Caret.MoveTo( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) ) );
            }

            Screen.Focus( this );
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
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
                if( Caret.SelectionOffset != 0 )
                {
                    //DeleteSelectedText();
                }

                TextBlock line = TextBlocks[ Caret.TextBlockIndex ];
                line.Text = line.Text.Insert( Caret.Offset, _char.ToString() );
                Caret.Offset++;
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
                        TextBlocks[ Caret.TextBlockIndex ].BlockType = (TextBlockType)( ( (int)( TextBlocks[ Caret.TextBlockIndex ].BlockType ) + 1 ) % (int)TextBlockType.Count );
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
                        Caret.TextBlockIndex++;
                        TextBlocks.Insert( Caret.TextBlockIndex, new TextBlock( this, "" ) );
                        Caret.Offset = 0;
                    }
                    break;
                case Keys.Back:
                    if( ! IsReadOnly && TextBlocks[ Caret.TextBlockIndex ].Text.Length > 0 )
                    {
                        if( Caret.SelectionOffset != 0 )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.Offset > 0 )
                        {
                            Caret.Offset--;
                            TextBlocks[ Caret.TextBlockIndex ].Text = TextBlocks[ Caret.TextBlockIndex ].Text.Remove( Caret.Offset, 1 );
                        }
                    }
                    break;
                case Keys.Delete:
                    if( ! IsReadOnly && TextBlocks[ Caret.TextBlockIndex ].Text.Length > 0 )
                    {
                        if( Caret.SelectionOffset != 0 )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( Caret.Offset < TextBlocks[ Caret.TextBlockIndex ].Text.Length )
                        {
                            TextBlocks[ Caret.TextBlockIndex ].Text = TextBlocks[ Caret.TextBlockIndex ].Text.Remove( Caret.Offset, 1 );
                        }
                    }
                    break;
                case Keys.Tab:
                    if( ! IsReadOnly )
                    {
                        if( bShift )
                        {
                            TextBlocks[ Caret.TextBlockIndex ].IndentLevel = Math.Max( 0, TextBlocks[ Caret.TextBlockIndex ].IndentLevel - 1 );
                        }
                        else
                        {
                            TextBlocks[ Caret.TextBlockIndex ].IndentLevel = Math.Min( 4, TextBlocks[ Caret.TextBlockIndex ].IndentLevel + 1 );
                        }
                    }
                    break;
                case Keys.Left:
                    if( bShift )
                    {
                        /*if( bCtrl )
                        {
                            int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if( iNewSelectionTarget > 0 )
                            {
                                iNewSelectionTarget = Text.LastIndexOf( ' ', iNewSelectionTarget - 2, iNewSelectionTarget - 2 ) + 1;
                            }

                            SelectionOffset = iNewSelectionTarget - CaretOffset;
                        }
                        else
                        {
                            SelectionOffset--;
                        }*/
                    }
                    else
                    {
                        int iNewCaretOffset = Caret.Offset - 1;

                        if( bCtrl )
                        {
                            Caret.SelectionOffset = 0;

                            if( iNewCaretOffset > 0 )
                            {
                                iNewCaretOffset = TextBlocks[ Caret.TextBlockIndex ].Text.LastIndexOf( ' ', iNewCaretOffset - 1, iNewCaretOffset - 1 ) + 1;
                            }
                        }
                        else
                        if( Caret.SelectionOffset != 0 )
                        {
                            iNewCaretOffset = ( Caret.SelectionOffset > 0 ) ? Caret.Offset : ( Caret.Offset + Caret.SelectionOffset );
                            Caret.SelectionOffset = 0;
                        }
                        Caret.Offset = iNewCaretOffset;
                    }
                    break;
                case Keys.Right:
                    if( bShift )
                    {
                        /*if( bCtrl )
                        {
                            int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if( iNewSelectionTarget < Text.Length )
                            {
                                iNewSelectionTarget = Text.IndexOf( ' ', iNewSelectionTarget, Text.Length - iNewSelectionTarget ) + 1;

                                if( iNewSelectionTarget == 0 )
                                {
                                    iNewSelectionTarget = Text.Length;
                                }

                                SelectionOffset = iNewSelectionTarget - CaretOffset;
                            }
                        }
                        else
                        {
                            SelectionOffset++;
                        }*/
                    }
                    else
                    {
                        int iNewCaretOffset = Caret.Offset + 1;

                        if( bCtrl )
                        {
                            string strText = TextBlocks[ Caret.TextBlockIndex ].Text;

                            if( iNewCaretOffset < strText.Length )
                            {
                                iNewCaretOffset = strText.IndexOf( ' ', iNewCaretOffset, strText.Length - iNewCaretOffset ) + 1;

                                if( iNewCaretOffset == 0 )
                                {
                                    iNewCaretOffset = strText.Length;
                                }
                            }
                        }
                        else
                        if( Caret.SelectionOffset != 0 )
                        {
                            iNewCaretOffset = ( Caret.SelectionOffset < 0 ) ? Caret.Offset : ( Caret.Offset + Caret.SelectionOffset );
                        }
                        Caret.Offset = iNewCaretOffset;
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
        internal override void Update( float _fElapsedTime )
        {
            Caret.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.ListFrame, LayoutRect, Screen.Style.PanelCornerSize, Color.White );

            //------------------------------------------------------------------
            // Text
            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ) );

            int iX = LayoutRect.X + Padding.Left;
            int iY = LayoutRect.Y + Padding.Top;

            foreach( TextBlock block in TextBlocks )
            {
                block.Draw( iX + block.IndentLevel * RichTextArea.IndentOffset, iY, LayoutRect.Width - Padding.Horizontal );
                iY += block.TotalHeight;
            }

            Screen.PopScissorRectangle();

            //------------------------------------------------------------------
            // Selection & cursor
            if( Caret.SelectionOffset != 0 )
            {
                Rectangle selectionRectangle;
                /*if( Caret.SelectionOffset > 0 )
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
            if( Screen.IsActive && HasFocus )
            {
                Caret.Draw();
            }
        }
    }
}
