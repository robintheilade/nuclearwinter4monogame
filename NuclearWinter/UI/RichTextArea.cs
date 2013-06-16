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
    public class RichTextCaret
    {
        //----------------------------------------------------------------------
        public int StartTextBlockIndex
        {
            get {
                if( TextArea.TextBlocks.Count > 0 )
                {
                    return Math.Min( TextArea.TextBlocks.Count - 1, miStartTextBlockIndex );
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                miStartTextBlockIndex = Math.Max( 0, Math.Min( TextArea.TextBlocks.Count - 1, value ) );
                EndTextBlockIndex = miStartTextBlockIndex;
            }
        }

        public int StartOffset
        {
            get {
                if( TextArea.TextBlocks.Count > 0 )
                {
                    return (int)MathHelper.Clamp( miStartOffset, 0, TextArea.TextBlocks[ StartTextBlockIndex ].TextLength );
                }
                else
                {
                    return 0;
                }
            }
            set 
            {
                if( TextArea.TextBlocks.Count > 0 )
                {
                    miStartOffset = (int)MathHelper.Clamp( value, 0, TextArea.TextBlocks[ StartTextBlockIndex ].TextLength );
                    EndTextBlockIndex   = StartTextBlockIndex;
                    EndOffset           = StartOffset;
                }
                else
                {
                    miStartOffset = 0;
                    EndTextBlockIndex = 0;
                    EndOffset = 0;
                }

                Timer = 0f;
            }
        }

        public int EndTextBlockIndex
        {
            get {
                if( TextArea.TextBlocks.Count > 0 )
                {
                    return Math.Min( TextArea.TextBlocks.Count - 1, miEndTextBlockIndex );
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                Timer = 0f;

                if( TextArea.TextBlocks.Count > 0 )
                {
                    miEndTextBlockIndex = Math.Min( TextArea.TextBlocks.Count - 1, value );

                    if( TextBlockChangedHandler != null )
                    {
                        TextBlockChangedHandler( this );
                    }
                }
                else
                {
                    miEndTextBlockIndex = 0;
                }
            }
        }

        public int EndOffset
        {
            get { 
                if( TextArea.TextBlocks.Count > 0 )
                {
                    return (int)MathHelper.Clamp( miEndOffset, 0, TextArea.TextBlocks[ EndTextBlockIndex ].TextLength );
                }
                else
                {
                    return 0;
                }
            }
            set 
            {
                if( TextArea.TextBlocks.Count > 0 )
                {
                    miEndOffset = (int)MathHelper.Clamp( value, 0, TextArea.TextBlocks[ EndTextBlockIndex ].TextLength );
                }
                else
                {
                    miEndOffset = 0;
                }
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
        public Action<RichTextCaret>    TextBlockChangedHandler;

        internal float          Timer;

        //----------------------------------------------------------------------
        int                     miStartTextBlockIndex;
        int                     miStartOffset;

        int                     miEndTextBlockIndex;
        int                     miEndOffset;

        //----------------------------------------------------------------------
        public RichTextCaret( RichTextArea _textArea )
        {
            TextArea = _textArea;
        }

        //----------------------------------------------------------------------
        internal virtual void Update( float _fElapsedTime )
        {
            Timer = TextArea.HasFocus ? ( Timer + _fElapsedTime ) : 0f;
        }

        protected void DrawSelection( Rectangle _rect, Color _color )
        {
            if( HasSelection )
            {
                Point origin = new Point( _rect.X, _rect.Y - (int)TextArea.Scrollbar.LerpOffset );

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
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, _color );
                }
                else
                {
                    Rectangle startSelectionRectangle = new Rectangle( startPos.X, startPos.Y, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal - startPos.X, iStartCaretHeight );
                    startSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, startSelectionRectangle, _color );

                    if( iStartBlockIndex == iEndBlockIndex )
                    {
                        if( iEndBlockLine > iStartBlockLine + 1 )
                        {
                            Rectangle selectionRectangle = new Rectangle( 0, startPos.Y + iStartCaretHeight, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal, iStartCaretHeight * ( iEndBlockLine - iStartBlockLine - 1 ) );
                            selectionRectangle.Offset( origin );
                            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, _color );
                        }
                    }
                    else
                    {
                        int iBlockSelectionStartY   = startPos.Y + iStartCaretHeight;
                        int iBlockSelectionEndY     = endPos.Y;

                        Rectangle selectionRectangle = new Rectangle( 0, iBlockSelectionStartY, TextArea.LayoutRect.Width - TextArea.Padding.Horizontal, iBlockSelectionEndY - iBlockSelectionStartY );
                        selectionRectangle.Offset( origin );
                        TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, selectionRectangle, _color );
                    }

                    int iEndCaretHeight = TextArea.TextBlocks[ iEndBlockIndex ].LineHeight;

                    Rectangle endSelectionRectangle = new Rectangle( 0, endPos.Y, endPos.X, iEndCaretHeight );
                    endSelectionRectangle.Offset( origin );
                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, endSelectionRectangle, _color );
                }
            }
        }

        //----------------------------------------------------------------------
        internal virtual void Draw( Rectangle _rect )
        {
            if( TextArea.TextBlocks.Count == 0 ) return;

            DrawSelection( _rect, TextArea.Screen.Style.DefaultTextColor * 0.3f );

            if( TextArea.Screen.IsActive && TextArea.HasFocus )
            {
                const float fBlinkInterval = 0.3f;

                if( Timer % (fBlinkInterval * 2) < fBlinkInterval )
                {
                    Point caretPos = TextArea.GetPositionForCaret( EndTextBlockIndex, EndOffset );
                    int iCaretHeight = TextArea.TextBlocks[ EndTextBlockIndex ].LineHeight;

                    TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, TextArea.Screen.Style.CaretWidth, iCaretHeight ), TextArea.Screen.Style.DefaultTextColor );
                }
            }
        }

        internal void SetCaretAt( Point _point, bool _bSelect )
        {
            if( TextArea.TextBlocks.Count == 0 ) return;

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
                    EndOffset = TextArea.TextBlocks[ EndTextBlockIndex ].TextLength;
                }
                else
                {
                    StartTextBlockIndex = TextArea.TextBlocks.Count - 1;
                    StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].TextLength;
                }
                return;
            }

            MoveInsideBlock( _point.X, iBlockCaretY / TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ].LineHeight, _bSelect );
        }

        internal void MoveInsideBlock( int _iX, int _iLine, bool _bSelect )
        {
            TextBlock caretBlock = TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ];

            int iOffset = 0;
            int iBlockLineIndex = Math.Max( 0, Math.Min( caretBlock.WrappedTextSpanLines.Count - 1, _iLine ) );

            for( int iLine = 0; iLine < iBlockLineIndex; iLine++ )
            {
                iOffset += caretBlock.WrappedTextSpanLines[ iLine ].Length;
            }

            int iOffsetInLine = ComputeCaretOffsetAtX( _iX - caretBlock.EffectiveIndentLevel * TextArea.Screen.Style.RichTextAreaIndentOffset, caretBlock.WrappedTextSpanLines[ iBlockLineIndex ].SimpleText, caretBlock.Font );
            iOffset += iOffsetInLine;
            if( _iLine < caretBlock.WrappedTextSpanLines.Count - 1 && iOffsetInLine == caretBlock.WrappedTextSpanLines[ iBlockLineIndex ].Length )
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
            TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );
            MoveInsideBlock( 0, iLine, _bSelect );
        }

        internal void MoveEnd( bool _bSelect )
        {
            int iLine;
            TextArea.TextBlocks[ EndTextBlockIndex ].GetXYForCaretOffset( EndOffset, out iLine );
            MoveInsideBlock( int.MaxValue, iLine, _bSelect );
        }

        internal void MoveLeft( bool _bSelect, bool _bWord )
        {
            // Left
            int iNewTextBlockIndex  = _bSelect || _bWord ? EndTextBlockIndex : StartTextBlockIndex;
            int iNewOffset          = ( _bSelect || _bWord ? EndOffset : StartOffset ) - 1;

            if( _bWord )
            {
                if( iNewOffset > 0 )
                {
                    iNewOffset = TextArea.TextBlocks[ iNewTextBlockIndex ].Content.SimpleText.LastIndexOf( ' ', iNewOffset - 1, iNewOffset - 1 ) + 1;
                }
            }
            else
            if( ! _bSelect && HasSelection )
            {
                if( HasBackwardSelection )
                {
                    iNewOffset = EndOffset;
                    iNewTextBlockIndex = EndTextBlockIndex;
                }
                else
                {
                    iNewOffset++;
                }
            }

            if( iNewOffset < 0 && iNewTextBlockIndex > 0 )
            {
                iNewTextBlockIndex--;
                iNewOffset = TextArea.TextBlocks[ iNewTextBlockIndex ].TextLength;
            }

            if( _bSelect )
            {
                SetSelection( StartOffset, iNewOffset, _iEndTextBlockIndex: iNewTextBlockIndex );
            }
            else
            {
                SetSelection( iNewOffset, _iStartTextBlockIndex: iNewTextBlockIndex );
            }
        }

        internal void MoveRight( bool _bSelect, bool _bWord )
        {
            // Right
            int iNewTextBlockIndex  = _bSelect || _bWord ? EndTextBlockIndex : StartTextBlockIndex;
            int iNewOffset          = ( _bSelect || _bWord ? EndOffset : StartOffset ) + 1;

            if( _bWord )
            {
                string strText = TextArea.TextBlocks[ iNewTextBlockIndex ].Content.SimpleText;

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
            if( ! _bSelect && HasSelection )
            {
                if( HasForwardSelection )
                {
                    iNewOffset = EndOffset;
                    iNewTextBlockIndex = EndTextBlockIndex;
                }
                else
                {
                    iNewOffset--;
                }
            }

            if( iNewOffset > TextArea.TextBlocks[ iNewTextBlockIndex ].TextLength && iNewTextBlockIndex < TextArea.TextBlocks.Count - 1 )
            {
                iNewTextBlockIndex++;
                iNewOffset = 0;
            }

            if( _bSelect )
            {
                SetSelection( StartOffset, iNewOffset, _iEndTextBlockIndex: iNewTextBlockIndex );
            }
            else
            {
                SetSelection( iNewOffset, _iStartTextBlockIndex: iNewTextBlockIndex );
            }
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
                        MoveInsideBlock( caretPos.X, TextArea.TextBlocks[ EndTextBlockIndex ].WrappedTextSpanLines.Count - 1, _bSelect );
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
                        MoveInsideBlock( caretPos.X, TextArea.TextBlocks[ StartTextBlockIndex ].WrappedTextSpanLines.Count - 1, _bSelect );
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

            if( iLine < TextArea.TextBlocks[ _bSelect ? EndTextBlockIndex : StartTextBlockIndex ].WrappedTextSpanLines.Count - 1 )
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
                        EndOffset = TextArea.TextBlocks[ EndTextBlockIndex ].TextLength;
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
                        StartOffset = TextArea.TextBlocks[ StartTextBlockIndex ].TextLength;
                    }
                }
            }
        }

        public void SetSelection( int _iStartOffset, int? _iEndOffset=null, int? _iStartTextBlockIndex=null, int? _iEndTextBlockIndex=null )
        {
            if( _iStartTextBlockIndex.HasValue )
            {
                StartTextBlockIndex = _iStartTextBlockIndex.Value;
            }

            StartOffset = _iStartOffset;

            if( _iEndTextBlockIndex.HasValue )
            {
                EndTextBlockIndex = _iEndTextBlockIndex.Value;
            }

            if( _iEndOffset.HasValue )
            {
                EndOffset = _iEndOffset.Value;
            }

            if( TextArea.CaretMovedHandler != null )
            {
                TextArea.CaretMovedHandler( this );
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
    public class RemoteRichTextCaret: RichTextCaret
    {
        public Color CaretColor = Color.Black;

        string mstrLabelString;
        Vector2 mvLabelSize;

        bool mbIsHovered;
        float mfLabelAlphaTimer = 0f;
        float mfLabelAlpha = 0f;

        //----------------------------------------------------------------------
        public RemoteRichTextCaret( string _strLabel, RichTextArea _textArea ): base ( _textArea )
        {
            mstrLabelString = _strLabel;
            
            mvLabelSize = TextArea.Screen.Style.SmallFont.MeasureString( mstrLabelString );
        }

        //----------------------------------------------------------------------
        public void OnMouseMove( Point _point )
        {
            Point caretPos = TextArea.GetPositionForCaret( EndTextBlockIndex, EndOffset );
            int iCaretHeight = TextArea.TextBlocks[ EndTextBlockIndex ].LineHeight;

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

            Point caretPos = TextArea.GetPositionForCaret( EndTextBlockIndex, EndOffset );
            int iCaretHeight = TextArea.TextBlocks[ EndTextBlockIndex ].LineHeight;

            // Caret
            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X, caretPos.Y, TextArea.Screen.Style.CaretWidth, iCaretHeight ), CaretColor );

            // Label
            TextArea.Screen.Game.SpriteBatch.Draw( TextArea.Screen.Game.WhitePixelTex, new Rectangle( caretPos.X - 4, caretPos.Y - (int)mvLabelSize.Y, (int)mvLabelSize.X + 6, (int)mvLabelSize.Y ), CaretColor * mfLabelAlpha );
            TextArea.Screen.Game.SpriteBatch.DrawString( TextArea.Screen.Style.SmallFont, mstrLabelString, new Vector2( caretPos.X - 1, caretPos.Y - mvLabelSize.Y ), Color.White * mfLabelAlpha );
        }

    }

    public enum TextSpanType
    {
        Default,
        Link
    }

    //--------------------------------------------------------------------------
    public class TextSpan
    {
        public string       Text;
        public TextSpanType SpanType;

        public string       LinkTarget;

        public TextSpan( string _strText, TextSpanType _type=TextSpanType.Default )
        {
            Text = _strText;
            SpanType = _type;
        }

        public TextSpan( string _strText, string _strLinkTarget )
        {
            Text = _strText;
            SpanType = UI.TextSpanType.Link;
            LinkTarget = _strLinkTarget;
        }
    
        internal static TextSpan FromTextSpan( TextSpan textSpan, string _strText )
        {
 	        var newTextSpan = new TextSpan( _strText, textSpan.SpanType );
            newTextSpan.LinkTarget = textSpan.LinkTarget;

            return newTextSpan;
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
    public class TextSoup
    {
        //----------------------------------------------------------------------
        public int Length {
            get {
                if( mbDirty ) UpdateCache();
                return miCachedLength;
            }
        }

        public string SimpleText
        {
            get {
                if( mbDirty ) UpdateCache();
                return mstrCachedText;
            }
        }

        public List<TextSpan> Spans { get { return mlSpans; } }

        //----------------------------------------------------------------------
        List<TextSpan>      mlSpans;

        string              mstrCachedText;
        int                 miCachedLength;
        bool                mbDirty;

        //----------------------------------------------------------------------
        public TextSoup( List<TextSpan> _lTextSpans )
        {
            mlSpans = _lTextSpans;
            if( mlSpans.Count == 0 )
            {
                mlSpans.Add( new TextSpan( "" ) );
            }

            mbDirty = true;
        }

        public TextSoup()
        {
            mlSpans = new List<TextSpan>();
            mbDirty = true;
        }

        //----------------------------------------------------------------------
        void MarkDirty()
        {
            mbDirty = true;
        }

        void UpdateCache()
        {
            miCachedLength = mlSpans.Sum( x => x.Text.Length );
            mstrCachedText = string.Join( "", mlSpans.Select( x => x.Text ) );
            mbDirty = false;
        }

        //----------------------------------------------------------------------
        public List<TextSoup> Wrap( int _iWidth, SpriteFont _font )
        {
            var lLines = new List<TextSoup>();
            var currentLine = new TextSoup();

            float fLineOffset = 0f;
            foreach( var textSpan in mlSpans )
            {
                int iChunkIndex = 0;

                foreach( var chunk in textSpan.Text.Split( '\n' ) )
                {
                    if( iChunkIndex > 0 )
                    {
                        // There was a line break
                        fLineOffset = 0;
                        lLines.Add( currentLine );
                        currentLine.Spans[ currentLine.Spans.Count - 1 ].Text += " ";
                        currentLine = new TextSoup();
                    }

                    var newTextSpan = TextSpan.FromTextSpan( textSpan, "" );
                    string newTextSpanContent = "";

                    if( chunk != "" )
                    {
                        var words = chunk.Split( ' ' );

                        bool bFirst = true;
                        foreach( var word in words )
                        {
                            if( bFirst ) bFirst = false;
                            else newTextSpanContent += " ";

                            if( newTextSpanContent != "" && fLineOffset + _font.MeasureString( newTextSpanContent + word ).X > _iWidth )
                            {
                                newTextSpan.Text = newTextSpanContent;
                                currentLine.mlSpans.Add( newTextSpan );

                                fLineOffset = 0f;
                                newTextSpanContent = "";
                                newTextSpan = TextSpan.FromTextSpan( textSpan, "" );

                                lLines.Add( currentLine );
                                currentLine = new TextSoup();
                            }

                            newTextSpanContent += word;
                        }
                    }

                    newTextSpan.Text = newTextSpanContent;
                    fLineOffset += _font.MeasureString( newTextSpanContent ).X;
                    currentLine.mlSpans.Add( newTextSpan );

                    iChunkIndex++;
                }
            }

            lLines.Add( currentLine );

            return lLines;
        }

        //----------------------------------------------------------------------
        public List<TextSpan> GetSubset( int _iStartOffset, int _iLength )
        {
            if( _iLength == 0 ) return new List<TextSpan>();
            if( _iLength < 0 ) _iLength = Length - _iStartOffset;
            int iEndOffset = _iStartOffset + _iLength;

            var lSpans = new List<TextSpan>();

            int iOffset = 0;
            foreach( var span in mlSpans )
            {
                int iCutStart = 0;
                int iCutEnd = span.Text.Length;

                if( _iStartOffset > iOffset )
                {
                    iCutStart = _iStartOffset - iOffset;
                }

                if( iEndOffset < iOffset + span.Text.Length )
                {
                    iCutEnd = iEndOffset - iOffset;
                }

                if( iCutStart < iCutEnd )
                {
                    lSpans.Add( TextSpan.FromTextSpan( span, span.Text.Substring( iCutStart, iCutEnd - iCutStart ) ) );
                }

                iOffset += span.Text.Length;
            }

            return lSpans;
        }

        //----------------------------------------------------------------------
        internal List<TextSpan> CopySpans()
        {
            var spans = new List<TextSpan>();

            foreach( var span in mlSpans )
            {
                spans.Add( TextSpan.FromTextSpan( span, span.Text ) );
            }

            return spans;
        }

        //----------------------------------------------------------------------
        internal void Insert(int _iOffset, string _strText)
        {
            if( mlSpans.Count == 0 ) throw new InvalidOperationException();

            int iPos = 0;
            foreach( var span in mlSpans )
            {
                if( _iOffset <= iPos + span.Text.Length )
                {
                    span.Text = span.Text.Insert( _iOffset - iPos, _strText );
                    break;
                }

                iPos += span.Text.Length;
            }

            MarkDirty();
        }

        //----------------------------------------------------------------------
        internal void Remove( int _iStartOffset, int _iLength )
        {
            if( _iLength == 0 ) return;
            if( _iLength < 0 ) _iLength = Length - _iStartOffset;
            int iEndOffset = _iStartOffset + _iLength;

            var lSpans = new List<TextSpan>();

            int iOffset = 0;
            foreach( var span in mlSpans )
            {
                int iCutStart = 0;
                int iCutEnd = span.Text.Length;

                if( _iStartOffset > iOffset )
                {
                    iCutStart = _iStartOffset - iOffset;
                }

                if( iEndOffset < iOffset + span.Text.Length )
                {
                    iCutEnd = iEndOffset - iOffset;
                }

                if( iCutStart < iCutEnd )
                {
                    span.Text = span.Text.Remove( iCutStart, iCutEnd - iCutStart );
                }

                iOffset += span.Text.Length;
            }

            MarkDirty();
        }

        //----------------------------------------------------------------------
        public void Append( List<TextSpan> _lSpans )
        {
            mlSpans.AddRange( _lSpans );
            MarkDirty();
        }

        //----------------------------------------------------------------------
        internal float MeasureSubset( UIFont _font, int _iStartOffset, int _iLength )
        {
            float fWidth = 0f;

            int iEndOffset = _iStartOffset + _iLength;

            int iOffset = 0;
            foreach( var span in mlSpans )
            {
                if( iOffset >= _iStartOffset )
                {
                    if( iOffset + span.Text.Length < iEndOffset )
                    {
                        // Fully included
                        fWidth += _font.MeasureString( span.Text ).X;
                    }
                    else
                    {
                        // Partially included
                        fWidth += _font.MeasureString( span.Text.Substring( 0, iEndOffset - iOffset ) ).X;
                        break;
                    }
                }
                else
                {
                    // Excluded
                }

                iOffset += span.Text.Length;
            }

            return fWidth;
        }

        //----------------------------------------------------------------------
        internal void Draw( Screen _screen, UIFont _font, Vector2 _vPosition, Color _color )
        {
            foreach( var span in mlSpans )
            {
                if( span.SpanType == TextSpanType.Link )
                {
                    _screen.Game.SpriteBatch.DrawString( _font, span.Text, _vPosition, _screen.Style.DefaultLinkColor );
                    _screen.Game.DrawLine( _vPosition + new Vector2( 0, _font.LineSpacing ), _vPosition + new Vector2( _font.MeasureString( span.Text ).X, _font.LineSpacing ), _screen.Style.DefaultLinkColor );
                }
                else
                {
                    _screen.Game.SpriteBatch.DrawString( _font, span.Text, _vPosition, _color );
                }

                _vPosition.X += _font.MeasureString( span.Text ).X;
            }
        }
    }

    //--------------------------------------------------------------------------
    public class TextBlockData
    {
        public TextBlockType        BlockType;
        public int                  IndentLevel;
        public TextSoup             Content;

        public TextBlockData( string _strText, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            Content = new TextSoup( new List<TextSpan> { new TextSpan( _strText ) } );
            BlockType   = _lineType;
            IndentLevel = _iIndentLevel;
        }

        public TextBlockData( List<TextSpan> _lTextSpans, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            Content = new TextSoup( _lTextSpans );
            BlockType   = _lineType;
            IndentLevel = _iIndentLevel;
        }
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
                        Font = mTextArea.Screen.Style.ParagraphFont;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                mlWrappedTextSpans = null;
            }
        }

        public int                  IndentLevel;
        public int                  EffectiveIndentLevel
        {
            get { return IndentLevel + ( ( BlockType == TextBlockType.OrderedListItem || BlockType == TextBlockType.UnorderedListItem ) ? 1 : 0 ); }
        }

        TextSoup mContent;
        public TextSoup Content { get { return mContent; } }

        List<TextSoup> mlWrappedTextSpans = null;
        public List<TextSoup> WrappedTextSpanLines
        {
            get {
                if( mlWrappedTextSpans == null )
                {
                    DoWrap( mTextArea.LayoutRect.Width - mTextArea.Padding.Horizontal );
                }
                return mlWrappedTextSpans;
            }
        }

        public UIFont               Font        { get; private set; }
        public int                  LineHeight  { get { return Font.LineSpacing; } }
        public int                  TotalHeight { get { return LineHeight * WrappedTextSpanLines.Count + LineHeight / 2; } }
        public int                  TotalHeightWithoutBottomPadding { get { return LineHeight * WrappedTextSpanLines.Count; } }
        public Color                Color;

        RichTextArea                mTextArea;

        public int                  TextLength
        {
            get { return mContent.Length; }
        }

        //----------------------------------------------------------------------
        public TextBlock( RichTextArea _textArea, List<TextSpan> _lTextSpans, Color _color, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            mTextArea   = _textArea;
            mContent = new TextSoup( _lTextSpans );
            BlockType   = _lineType;
            IndentLevel = _iIndentLevel;
            Color       = _color;
        }

        public TextBlock( RichTextArea _textArea, List<TextSpan> _lTextSpans, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        : this( _textArea, _lTextSpans, _textArea.Screen.Style.DefaultTextColor, _lineType, _iIndentLevel )
        {
        }

        public TextBlock( RichTextArea _textArea, string _strText, Color _color, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        {
            mTextArea   = _textArea;
            mContent = new TextSoup( new List<TextSpan> { new TextSpan( _strText ) } );
            BlockType   = _lineType;
            IndentLevel = _iIndentLevel;
            Color       = _color;
        }

        public TextBlock( RichTextArea _textArea, string _strText, TextBlockType _lineType=TextBlockType.Paragraph, int _iIndentLevel=0 )
        : this( _textArea, _strText, _textArea.Screen.Style.DefaultTextColor, _lineType, _iIndentLevel )
        {
        }

        //----------------------------------------------------------------------
        public void DoWrap( int _iWidth )
        {
            int iActualWidth = _iWidth - EffectiveIndentLevel * mTextArea.Screen.Style.RichTextAreaIndentOffset;
            mlWrappedTextSpans = mContent.Wrap( iActualWidth, Font );
        }


        public void Draw( int _iX, int _iY, int _iWidth )
        {
            int iIndentedX = _iX + EffectiveIndentLevel * mTextArea.Screen.Style.RichTextAreaIndentOffset;

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
                    mTextArea.Screen.Game.SpriteBatch.DrawString( Font, strItemPrefix, new Vector2( iIndentedX - Font.MeasureString( strItemPrefix ).X, _iY + Font.YOffset ), Color  );
                    break;
                case TextBlockType.UnorderedListItem:
                    mTextArea.Screen.Game.SpriteBatch.DrawString( Font, "•", new Vector2( iIndentedX - Font.MeasureString( "• " ).X, _iY + Font.YOffset ), Color );
                    break;
                default:
                    throw new NotSupportedException();
            }

            int iTextY = _iY;
            foreach( var line in WrappedTextSpanLines )
            {
                line.Draw( mTextArea.Screen, Font, new Vector2( iIndentedX, iTextY + Font.YOffset ), Color );
                iTextY += Font.LineSpacing;
            }
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
            foreach( var line in WrappedTextSpanLines )
            {
                if( iOffset + line.Length == _iCaretOffset && iLine < WrappedTextSpanLines.Count - 1 )
                {
                    iLine++;
                    _iLine = iLine;
                    return new Point( EffectiveIndentLevel * mTextArea.Screen.Style.RichTextAreaIndentOffset, iLine * LineHeight );
                }

                if( iOffset + line.Length < _iCaretOffset )
                {
                    iOffset += line.Length;
                }
                else
                {
                    _iLine = iLine;
                    return new Point( (int)line.MeasureSubset( Font, 0, _iCaretOffset - iOffset ) + EffectiveIndentLevel * mTextArea.Screen.Style.RichTextAreaIndentOffset, iLine * LineHeight );
                }

                iLine++;
            }

            _iLine = iLine;
            return Point.Zero;
        }

        internal TextSpan GetTextSpanAtXY( Point _point )
        {
            int iLine = _point.Y / LineHeight;

            float fOffset = EffectiveIndentLevel * mTextArea.Screen.Style.RichTextAreaIndentOffset;

            if( _point.X < fOffset ) return null;

            foreach( var span in WrappedTextSpanLines[iLine].Spans )
            {
                float fSpanWidth = (float)Font.MeasureString( span.Text ).X;

                if( _point.X < fOffset + fSpanWidth )
                {
                    return span;
                }

                fOffset += fSpanWidth;
            }

            return null;
        }

        internal List<TextSpan> GetSubSpans( int _iStartOffset, int _iLength=-1 )
        {
            return mContent.GetSubset( _iStartOffset, _iLength );
        }

        internal List<TextSpan> CopySpans()
        {
            return mContent.CopySpans();
        }

        internal void SetSpans( List<TextSpan> _lSpans )
        {
            mContent = new TextSoup( _lSpans );
            mlWrappedTextSpans = null;
        }

        internal void InsertText( int _iOffset, string _strText )
        {
            mContent.Insert( _iOffset, _strText );
            mlWrappedTextSpans = null;
        }

        internal void RemoveText( int _iStartOffset, int _iLength=-1 )
        {
            mContent.Remove( _iStartOffset, _iLength );
            mlWrappedTextSpans = null;
        }

        internal void Append( List<TextSpan> _spans )
        {
            mContent.Append( _spans );
            mlWrappedTextSpans = null;
        }
    }

    //--------------------------------------------------------------------------
    public class RichTextArea: Widget
    {
        //----------------------------------------------------------------------
        public List<TextBlock>  TextBlocks          { get; private set; }
        public RichTextCaret    Caret               { get; private set; }
        public Dictionary<UInt16,RemoteRichTextCaret>
                                RemoteCaretsById    { get; private set; }

        public bool             IsReadOnly;

        internal int            CurrentListIndex;

        // FIXME: Use EventHandler for those?
        public Func<RichTextArea,int,int,TextBlockType,int,bool>    BlockStartInsertedHandler;
        public Func<RichTextArea,int,bool>                          BlockStartRemovedHandler;
        public Func<RichTextArea,int,TextBlockType,bool>            BlockTypeChangedHandler;
        public Func<RichTextArea,int,int,bool>                      BlockIndentLevelChangedHandler;

        public Func<RichTextArea,int,int,string,bool>               TextInsertedHandler;
        public Func<RichTextArea,int,int,int,int,bool>              TextRemovedHandler;

        public Action<RichTextCaret>                                CaretMovedHandler;

        //----------------------------------------------------------------------
        public Scrollbar        Scrollbar           { get; private set; }

        bool                    mbIsDragging;
        bool                    mbScrollToCaret;

        public Texture2D        PanelTex;

        //----------------------------------------------------------------------
        public RichTextArea( Screen _screen )
        : base( _screen )
        {
            Caret           = new RichTextCaret( this );
            TextBlocks       = new List<TextBlock>();
            TextBlocks.Add( new TextBlock( this, "" ) );

            RemoteCaretsById = new Dictionary<UInt16,RemoteRichTextCaret>();

            Padding         = Screen.Style.RichTextAreaPadding;

            Scrollbar       = new Scrollbar( _screen );
            Scrollbar.Parent = this;

            PanelTex        = Screen.Style.RichTextAreaFrame;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            bool bWrapTextNeeded = ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height );

            ContentHeight = Padding.Vertical;

            foreach( TextBlock textBlock in TextBlocks )
            {
                if( bWrapTextNeeded )
                {
                    textBlock.DoWrap( LayoutRect.Width - Padding.Horizontal );
                }
                ContentHeight += textBlock.TotalHeight;
            }
            
            if( TextBlocks.Count > 0 )
            {
                ContentHeight -= TextBlocks[ TextBlocks.Count - 1 ].LineHeight / 2;
            }

            if( mbScrollToCaret )
            {
                Point caretPos = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
                int iCaretHeight = TextBlocks[ Caret.EndTextBlockIndex ].LineHeight;

                if( caretPos.Y < LayoutRect.Top + Padding.Top )
                {
                    Scrollbar.Offset += caretPos.Y - ( LayoutRect.Top + Padding.Top );
                }
                else
                if( caretPos.Y > LayoutRect.Bottom - Padding.Bottom - iCaretHeight )
                {
                    Scrollbar.Offset += caretPos.Y - ( LayoutRect.Bottom - Padding.Bottom - iCaretHeight );
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
            Caret.SetCaretAt( new Point( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ), _hitPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset ), _bSelect );
        }

        //----------------------------------------------------------------------
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
        protected internal override bool OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return false;

            bool bShortcutKey = Screen.Game.InputMgr.IsShortcutKeyDown();
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );

            TextSpan span = ! bShortcutKey ? GetTextSpanAtPosition( _hitPoint ) : null;

            if( span != null && span.SpanType == TextSpanType.Link )
            {
                Process.Start( span.LinkTarget );
            }
            else
            {
                mbIsDragging = true;
                SetCaretPosition( _hitPoint, bShift );
            }

            Screen.Focus( this );

            return true;
        }

        public override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                SetCaretPosition( _hitPoint, true );
            }
            else
            {
                bool bShortcutKey = Screen.Game.InputMgr.IsShortcutKeyDown();

                var textSpan = ! bShortcutKey ? GetTextSpanAtPosition( _hitPoint ) : null;
                if( textSpan != null && textSpan.SpanType == TextSpanType.Link )
                {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.Hand;
#endif
                }
                else
                {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.IBeam;
#endif
                }

                foreach( var remoteCaret in RemoteCaretsById.Values )
                {
                    remoteCaret.OnMouseMove( _hitPoint );
                }
            }
        }

        TextSpan GetTextSpanAtPosition( Point _point )
        {
            if( TextBlocks.Count == 0 ) return null;

            var insidePoint = new Point( _point.X - ( LayoutRect.X + Padding.Left ), _point.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset );

            if( insidePoint.Y < 0 )
            {
                return null;
            }

            int iTextBlock      = 0;
            int iBlockY         = 0;

            while( iTextBlock < TextBlocks.Count )
            {
                var block = TextBlocks[ iTextBlock ];

                if( insidePoint.Y < iBlockY + block.TotalHeightWithoutBottomPadding )
                {
                    break;
                }

                iBlockY += block.TotalHeight;
                iTextBlock++;
            }

            if( iTextBlock == TextBlocks.Count )
            {
                return null;
            }

            return TextBlocks[ iTextBlock ].GetTextSpanAtXY( new Point( insidePoint.X, insidePoint.Y - iBlockY ) );
        }

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
        public void DeleteSelectedText()
        {
            if( ! Caret.HasSelection || IsReadOnly ) return;

            bool bHasForwardSelection = Caret.HasForwardSelection;
            int iStartBlockIndex    = bHasForwardSelection ? Caret.StartTextBlockIndex : Caret.EndTextBlockIndex;
            int iStartOffset        = bHasForwardSelection ? Caret.StartOffset : Caret.EndOffset;
            int iEndBlockIndex      = bHasForwardSelection ? Caret.EndTextBlockIndex : Caret.StartTextBlockIndex;
            int iEndOffset          = bHasForwardSelection ? Caret.EndOffset : Caret.StartOffset;

            if( TextRemovedHandler == null || TextRemovedHandler( this, iStartBlockIndex, iStartOffset, iEndBlockIndex, iEndOffset ) )
            {
                // Merge start and end block
                var spans = ( iStartOffset < TextBlocks[ iStartBlockIndex ].TextLength ? TextBlocks[ iStartBlockIndex ].GetSubSpans( 0, iStartOffset ) : TextBlocks[ iStartBlockIndex ].CopySpans() );
                spans.AddRange( TextBlocks[ iEndBlockIndex ].GetSubSpans( iEndOffset ) );
                TextBlocks[ iStartBlockIndex ].SetSpans( spans );

                // Delete blocks in between
                for( int i = iEndBlockIndex; i > iStartBlockIndex; i-- )
                {
                    TextBlocks.RemoveAt( iStartBlockIndex + 1 );
                }

                // Clear selection
                Caret.SetSelection( iStartOffset, _iStartTextBlockIndex: iStartBlockIndex );

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
                    strText += TextBlocks[ iStartBlockIndex ].Content.SimpleText.Substring( iStartOffset ) + "\n\n";

                    for( int i = iStartBlockIndex + 1; i < iEndBlockIndex; i++ )
                    {
                        strText += TextBlocks[ i ].Content.SimpleText + "\n\n";
                    }

                    strText += TextBlocks[ iEndBlockIndex ].Content.SimpleText.Substring( 0, iEndOffset );
                }
                else
                {
                    strText += TextBlocks[ iStartBlockIndex ].Content.SimpleText.Substring( iStartOffset, iEndOffset - iStartOffset );
                }
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
                strPastedText = strPastedText.Replace( "\r\n", "\n" );
                DeleteSelectedText();

                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, strPastedText ) )
                {
                    TextBlocks[ Caret.StartTextBlockIndex ].InsertText( Caret.StartOffset, strPastedText );
                    Caret.SetSelection( Caret.StartOffset + strPastedText.Length );
                    mbScrollToCaret = true;
                }
            }
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            Caret.SetSelection( 0, TextBlocks[ TextBlocks.Count - 1 ].TextLength, 0, TextBlocks.Count - 1 );

            mbIsDragging = false;
        }

        public void ClearSelection()
        {
            Caret.SetSelection( Caret.EndOffset, _iStartTextBlockIndex: Caret.EndTextBlockIndex );
        }

        //----------------------------------------------------------------------
        protected internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ! char.IsControl( _char ) )
            {
                DeleteSelectedText();

                string strAddedText = _char.ToString();
                if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, strAddedText ) )
                {
                    TextBlock textBlock = TextBlocks[ Caret.StartTextBlockIndex ];
                    textBlock.InsertText( Caret.StartOffset, strAddedText );
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
                        TextBlock textBlock = TextBlocks[ Caret.StartTextBlockIndex ];

                        if( bShift )
                        {
                            if( TextInsertedHandler == null || TextInsertedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, "\n" ) )
                            {
                                textBlock.InsertText( Caret.StartOffset, "\n" );
                                Caret.SetSelection( Caret.StartOffset + 1 );
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        {
                            if( textBlock.TextLength == 0 && textBlock.BlockType != TextBlockType.Paragraph )
                            {
                                if( BlockTypeChangedHandler == null || BlockTypeChangedHandler( this, Caret.StartTextBlockIndex, TextBlockType.Paragraph ) )
                                {
                                    if( BlockIndentLevelChangedHandler == null || BlockIndentLevelChangedHandler( this, Caret.StartTextBlockIndex, 0 ) )
                                    {
                                        textBlock.BlockType = TextBlockType.Paragraph;
                                        textBlock.IndentLevel = 0;
                                        Caret.SetSelection( Caret.StartOffset, _iStartTextBlockIndex: Caret.StartTextBlockIndex ); // Trigger block change handler
                                        mbScrollToCaret = true;
                                    }
                                }
                                break;
                            }
                        
                            TextBlockType newBlockType  = TextBlockType.Paragraph;
                            int iNewBlockIndentLevel    = 0;

                            if( textBlock.TextLength != 0 )
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
                                if( Caret.StartOffset == 0 && textBlock.TextLength > 0 )
                                {
                                    TextBlocks.Insert( Caret.StartTextBlockIndex, new TextBlock( this, "", newBlockType, iNewBlockIndentLevel ) );
                                    Caret.Timer = 0f;
                                }
                                else
                                {
                                    var newSpans = new List<TextSpan>();
                                    if( Caret.StartOffset < textBlock.TextLength )
                                    {
                                        newSpans = textBlock.GetSubSpans( Caret.StartOffset );
                                        textBlock.RemoveText( Caret.StartOffset );
                                    }

                                    TextBlocks.Insert( Caret.StartTextBlockIndex + 1, new TextBlock( this, newSpans, newBlockType, iNewBlockIndentLevel ) );
                                    Caret.SetSelection( 0 );
                                }

                                Caret.SetSelection( Caret.StartOffset, _iStartTextBlockIndex: Caret.StartTextBlockIndex + 1 );
                                mbScrollToCaret = true;
                            }
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
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset - 1, Caret.StartTextBlockIndex, Caret.StartOffset ) )
                            {
                                Caret.SetSelection( Caret.StartOffset - 1 );
                                TextBlocks[ Caret.StartTextBlockIndex ].RemoveText( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        if( Caret.StartTextBlockIndex > 0 )
                        {
                            int iNewCaretOffset = TextBlocks[ Caret.StartTextBlockIndex - 1 ].TextLength;

                            if( BlockStartRemovedHandler == null || BlockStartRemovedHandler( this, Caret.StartTextBlockIndex ) )
                            {
                                Caret.SetSelection( iNewCaretOffset, _iStartTextBlockIndex: Caret.StartTextBlockIndex - 1 );

                                if( iNewCaretOffset > 0 )
                                {
                                    // NOTE: Stealing the old block's spans without copying since the block will be discarded
                                    TextBlocks[ Caret.StartTextBlockIndex ].Append( TextBlocks[ Caret.StartTextBlockIndex + 1 ].Content.Spans );
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex + 1 );
                                }
                                else
                                {
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex );
                                }

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
                        if( Caret.StartOffset < TextBlocks[ Caret.StartTextBlockIndex ].TextLength )
                        {
                            if( TextRemovedHandler == null || TextRemovedHandler( this, Caret.StartTextBlockIndex, Caret.StartOffset, Caret.StartTextBlockIndex, Caret.StartOffset + 1 ) )
                            {
                                TextBlocks[ Caret.StartTextBlockIndex ].RemoveText( Caret.StartOffset, 1 );
                                mbScrollToCaret = true;
                            }
                        }
                        else
                        if( Caret.StartTextBlockIndex < TextBlocks.Count - 1 )
                        {
                            if( BlockStartRemovedHandler == null || BlockStartRemovedHandler( this, Caret.StartTextBlockIndex + 1 ) )
                            {
                                if( Caret.StartOffset == 0 && TextBlocks[ Caret.StartTextBlockIndex ].TextLength == 0 )
                                {
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex );
                                }
                                else
                                {
                                    // NOTE: Stealing the old block's spans without copying since the block will be discarded
                                    TextBlocks[ Caret.StartTextBlockIndex ].Append( TextBlocks[ Caret.StartTextBlockIndex + 1 ].Content.Spans );
                                    TextBlocks.RemoveAt( Caret.StartTextBlockIndex + 1 );
                                }

                                Caret.Timer = 0f;
                                mbScrollToCaret = true;
                            }
                        }
                    }
                    break;
                case OSKey.Tab:
                    if( ! IsReadOnly && ! bCtrl )
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
#if !MONOMAC
                case OSKey.Left: {
#else
                case OSKey.LeftArrow: {
#endif
                    Caret.MoveLeft( bShift, bCtrl );
                    mbScrollToCaret = true;
                    break;
                }
#if !MONOMAC
                case OSKey.Right: {
#else
                case OSKey.RightArrow: {
#endif
                    Caret.MoveRight( bShift, bCtrl );
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
                    Point target = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
                    target.Y -= LayoutRect.Height;
                    SetCaretPosition( target, bShift );
                    mbScrollToCaret = true;
                    break;
                }
                case OSKey.PageDown: {
                    Point target = GetPositionForCaret( Caret.EndTextBlockIndex, Caret.EndOffset );
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

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if( PanelTex != null )
            {
                Screen.DrawBox( PanelTex, LayoutRect, Screen.Style.RichTextAreaFrameCornerSize, Color.White );
            }

            //------------------------------------------------------------------
            // Text
            var rect = LayoutRect; rect.Inflate( -Screen.Style.RichTextAreaScissorOffset, -Screen.Style.RichTextAreaScissorOffset );
            Screen.PushScissorRectangle( rect );

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
                        CurrentListIndex = ( lListIndices.Count > 0 ? lListIndices.Pop() : 0 ) + 1;
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
            // Draw carets & selections
            foreach( var caret in RemoteCaretsById.Values )
            {
                caret.Draw( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ) );
            }

            Caret.Draw( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ) );


            Screen.PopScissorRectangle();

            Scrollbar.Draw();
        }

        public void ReplaceContent( List<TextBlock> _blocks )
        {
            int iStartBlockIndex    = Caret.StartTextBlockIndex;
            int iStartOffset        = Caret.StartOffset;

            int iEndBlockIndex      = Caret.EndTextBlockIndex;
            int iEndOffset          = Caret.EndOffset;

            TextBlocks = _blocks;

            // Make sure caret is valid
            Caret.StartTextBlockIndex   = iStartBlockIndex;
            Caret.StartOffset           = iStartOffset;
            Caret.EndTextBlockIndex     = iEndBlockIndex;
            Caret.EndOffset             = iEndOffset;
        }
    }
}
