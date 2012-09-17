using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
    // An EditBox to enter some text
    public class EditBox: Widget
    {
        UIFont mFont;
        public UIFont       Font
        {
            get { return mFont; }
            
            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        string  mstrText;
        Point   mpTextPosition;

        int     miCaretOffset;
        public int     CaretOffset {
            get { return miCaretOffset; }
            set {
                miCaretOffset = (int)MathHelper.Clamp( value, 0, mstrText.Length );
                miSelectionOffset = 0;
                ComputeCaretAndSelectionX();
                mfTimer = 0f;
            }
        }
        int     miCaretX;
        int     miCaretWidth = 2;

        int     miSelectionOffset;
        public int     SelectionOffset {
            get { return miSelectionOffset; }
            set {
                miSelectionOffset = (int)MathHelper.Clamp( value, -miCaretOffset, mstrText.Length - miCaretOffset );
                ComputeCaretAndSelectionX();
            }
        }
        int     miSelectionX;

        void ComputeCaretAndSelectionX()
        {
            miCaretX = miCaretOffset > 0 ? (int)mFont.MeasureString( mstrDisplayedText.Substring( 0, miCaretOffset ) ).X : 0;
            int iTarget = miCaretX;

            if( miSelectionOffset != 0 )
            {
                miSelectionX = ( miCaretOffset + miSelectionOffset ) > 0 ? (int)mFont.MeasureString( mstrDisplayedText.Substring( 0, miCaretOffset + miSelectionOffset ) ).X : 0;
                iTarget = miSelectionX;
            }

            int iScrollStep = LayoutRect.Width / 3;

            if( LayoutRect.Width != 0 && iTarget > miScrollOffset + ( LayoutRect.Width - Padding.Horizontal ) - miCaretWidth )
            {
                miScrollOffset = Math.Min( miMaxScrollOffset, ( iTarget - ( LayoutRect.Width - Padding.Horizontal ) + miCaretWidth ) + iScrollStep );
            }
            else
            if( iTarget < miScrollOffset )
            {
                miScrollOffset = Math.Max( 0, iTarget - iScrollStep );
            }
        }

        bool    mbIsDragging;
        bool    mbIsHovered;
        float   mfTimer;

        public const char DefaultPasswordChar = '●';

        char mPasswordChar = '\0';
        public char PasswordChar {
            get { return mPasswordChar; }
            set { mPasswordChar = value; UpdateContentSize(); }
        }

        public string Text
        {
            get { return mstrText; }
            set
            {
                if( mstrText != value )
                {
                    mstrText = value;
                    CaretOffset = Math.Min( miCaretOffset, mstrText.Length );

                    UpdateContentSize();
                    miScrollOffset = Math.Min( miScrollOffset, miMaxScrollOffset );
                }
            }
        }

        string                  mstrDisplayedText;
        int                     miScrollOffset;

        int                     miTextWidth;
        int                     miMaxScrollOffset {
            get {
                return (int)Math.Max( 0, miTextWidth - ( LayoutRect.Width - Padding.Horizontal ) + miCaretWidth );
            }
        }

        public Func<char,bool>      TextEnteredHandler;
        public Func<string,string>  LookupHandler;

        public static bool IntegerValidator( char _char )   { return ( _char >= '0' && _char <= '9' ) || _char == '-'; }
        public static bool FloatValidator( char _char )     { return ( _char >= '0' && _char <= '9' ) || _char == '.' || _char == '-'; }

        public Action<EditBox>  ValidateHandler;
        public Action<EditBox>  FocusHandler;
        public Action<EditBox>  BlurHandler;

        public Action<EditBox,int,string>   TextInsertedHandler;
        public Action<EditBox,int,int>      TextRemovedHandler;

        public bool             IsReadOnly;
        public int              MaxLength;

        public bool             CanBeEscapeCleared;

        public Texture2D        Frame;
        public int              FrameCornerSize;

        //----------------------------------------------------------------------
        public EditBox( Screen _screen, string _strText = "", Func<char,bool> _textEnteredHandler = null )
        : base( _screen )
        {
            mstrText    = _strText;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 15 );
            TextEnteredHandler = _textEnteredHandler;

            Frame               = Screen.Style.EditBoxFrame;
            FrameCornerSize     = Screen.Style.EditBoxCornerSize;


            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            CaretOffset = 0;
            SelectionOffset = Text.Length;
            mbIsDragging = false;
        }

        // (can be used as a FocusHandler)
        public static void SelectAll( EditBox _editBox )
        {
            _editBox.SelectAll();
        }

        public void ClearSelection()
        {
            SelectionOffset = 0;
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            if( SelectionOffset > 0 )
            {
                if( TextRemovedHandler != null ) TextRemovedHandler( this, CaretOffset, SelectionOffset );
                Text = Text.Remove( CaretOffset, SelectionOffset );
                SelectionOffset = 0;
            }
            else
            if( SelectionOffset < 0 )
            {
                int iNewCaretOffset = CaretOffset + SelectionOffset;

                if( TextRemovedHandler != null ) TextRemovedHandler( this, CaretOffset + SelectionOffset, -SelectionOffset );
                Text = Text.Remove( CaretOffset + SelectionOffset, -SelectionOffset );
                CaretOffset = iNewCaretOffset;
            }
        }

        public void CopySelectionToClipboard()
        {
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
        }

        public void PasteFromClipboard()
        {
            // NOTE: For this to work, you must put [STAThread] before your Main()
            string strPastedText = System.Windows.Forms.Clipboard.GetText();
            if( strPastedText != null )
            {
                strPastedText = strPastedText.Replace( "\r\n", " " ).Replace( "\n", " " ); 

                DeleteSelectedText();

                if( MaxLength != 0 && strPastedText.Length > MaxLength - Text.Length )
                {
                    strPastedText = strPastedText.Substring( 0, MaxLength - Text.Length );
                }

                if( TextInsertedHandler != null ) TextInsertedHandler( this, CaretOffset, strPastedText );
                Text = Text.Insert( CaretOffset, strPastedText );
                CaretOffset += strPastedText.Length;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            mstrDisplayedText = ( mPasswordChar == '\0' ) ? Text : "".PadLeft( Text.Length, mPasswordChar );

            miTextWidth = (int)Font.MeasureString( mstrDisplayedText ).X;
            ContentWidth = 0; //(int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            mpTextPosition = new Point(
                LayoutRect.X + Padding.Left,
                LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseEnter( Point _hitPoint )
        {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.IBeam;
#endif
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        protected internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                int iOffset = GetCaretOffsetAtX( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
                SelectionOffset = iOffset - miCaretOffset;
            }
        }

        protected internal override void OnMouseOut( Point _hitPoint )
        {
#if !MONOGAME
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.Default;
#endif
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            mbIsDragging = true;

            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.LeftShift, true ) || Screen.Game.InputMgr.KeyboardState.IsKeyDown( Keys.RightShift, true );
            if( HasFocus && bShift )
            {
                int iOffset = GetCaretOffsetAtX( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
                SelectionOffset = iOffset - miCaretOffset;
            }
            else
            {
                CaretOffset = GetCaretOffsetAtX( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
            }

            Screen.Focus( this );
            OnActivateDown();
        }

        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            if( mbIsDragging )
            {
                int iOffset = GetCaretOffsetAtX( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
                SelectionOffset = iOffset - miCaretOffset;
                mbIsDragging = false;
            }
            else
            if( HitTest( _hitPoint ) == this )
            {
                OnActivateUp();
            }
            else
            {
            }
        }

        int GetCaretOffsetAtX( int _x )
        {
            // FIXME: This does many calls to Font.MeasureString
            // We should do a binary search instead!

            _x += miScrollOffset;

            int iIndex = 0;

            float fPreviousX = 0f;

            while( iIndex < Text.Length )
            {
                iIndex++;

                float fX = Font.MeasureString( Text.Substring( 0, iIndex ) ).X;
                if( fX > _x )
                {
                    bool bAfter = ( fX - _x ) < ( ( fX - fPreviousX ) / 2f );
                    return bAfter ? iIndex : ( iIndex - 1 );
                }

                fPreviousX = fX;
            }

            return iIndex;
        }

        //----------------------------------------------------------------------
        protected internal override void OnTextEntered( char _char )
        {
            if( ! IsReadOnly && ( MaxLength == 0 || Text.Length < MaxLength || SelectionOffset != 0 ) && ! char.IsControl( _char ) && ( TextEnteredHandler == null || TextEnteredHandler( _char ) ) )
            {
                if( SelectionOffset != 0 )
                {
                    DeleteSelectedText();
                }

                string strAddedText = _char.ToString();
                if( TextInsertedHandler != null ) TextInsertedHandler( this, CaretOffset, strAddedText );

                Text = Text.Insert( CaretOffset, strAddedText );

                CaretOffset++;
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
                    if( bShortcutKey && mPasswordChar == '\0' )
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case OSKey.C:
                    if( bShortcutKey && mPasswordChar == '\0' )
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
                    if( ! IsReadOnly && ValidateHandler != null ) ValidateHandler( this );
                    break;
#if !MONOMAC
                case OSKey.Back:
#else
                case OSKey.ForwardDelete:
#endif
                    if( ! IsReadOnly && Text.Length > 0 )
                    {
                        if( SelectionOffset != 0 )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( CaretOffset > 0 )
                        {
                            CaretOffset--;

                            if( TextRemovedHandler != null ) TextRemovedHandler( this, CaretOffset, 1 );
                            Text = Text.Remove( CaretOffset, 1 );
                        }
                    }
                    break;
                case OSKey.Delete:
                    if( ! IsReadOnly && Text.Length > 0 )
                    {
                        if( SelectionOffset != 0 )
                        {
                            DeleteSelectedText();
                        }
                        else
                        if( CaretOffset < Text.Length )
                        {
                            if( TextRemovedHandler != null ) TextRemovedHandler( this, CaretOffset, 1 );
                            Text = Text.Remove( CaretOffset, 1 );
                        }
                    }
                    break;
                case OSKey.Escape:
                    if( ! IsReadOnly && CanBeEscapeCleared && Text.Length > 0 )
                    {
                        SelectAll();
                        DeleteSelectedText();
                    }
                    break;
#if !MONOMAC
                case OSKey.Left:
#else
                case OSKey.LeftArrow:
#endif
                    if( bShift )
                    {
                        if( bCtrl )
                        {
                            int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if( iNewSelectionTarget > 0 )
                            {
                                iNewSelectionTarget = Text.LastIndexOf( ' ', Math.Max( iNewSelectionTarget - 2, 0 ) ) + 1;
                            }

                            SelectionOffset = iNewSelectionTarget - CaretOffset;
                        }
                        else
                        {
                            SelectionOffset--;
                        }
                    }
                    else
                    {
                        int iNewCaretOffset = CaretOffset - 1;

                        if( bCtrl )
                        {
                            SelectionOffset = 0;

                            if( iNewCaretOffset > 0 )
                            {
                                iNewCaretOffset = Text.LastIndexOf( ' ', iNewCaretOffset - 1 ) + 1;
                            }
                        }
                        else
                        if( SelectionOffset != 0 )
                        {
                            iNewCaretOffset = ( SelectionOffset > 0 ) ? CaretOffset : CaretOffset + SelectionOffset;
                            SelectionOffset = 0;
                        }
                        CaretOffset = iNewCaretOffset;
                    }
                    break;
#if !MONOMAC
                case OSKey.Right:
#else
                case OSKey.RightArrow:
#endif
                    if( bShift )
                    {
                        if( bCtrl )
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
                        }
                    }
                    else
                    {
                        int iNewCaretOffset = CaretOffset + 1;

                        if( bCtrl )
                        {
                            if( iNewCaretOffset < Text.Length )
                            {
                                iNewCaretOffset = Text.IndexOf( ' ', iNewCaretOffset, Text.Length - iNewCaretOffset ) + 1;

                                if( iNewCaretOffset == 0 )
                                {
                                    iNewCaretOffset = Text.Length;
                                }
                            }
                        }
                        else
                        if( SelectionOffset != 0 )
                        {
                            iNewCaretOffset = ( SelectionOffset < 0 ) ? CaretOffset : CaretOffset + SelectionOffset;
                        }
                        CaretOffset = iNewCaretOffset;
                    }
                    break;
                case OSKey.End:
                    if( bShift )
                    {
                        SelectionOffset = Text.Length - CaretOffset;
                    }
                    else
                    {
                        CaretOffset = Text.Length;
                    }
                    break;
                case OSKey.Home:
                    if( bShift )
                    {
                        SelectionOffset = -CaretOffset;
                    }
                    else
                    {
                        CaretOffset = 0;
                    }
                    break;
                case OSKey.Tab:
                    if( LookupHandler != null )
                    {
                        if( CaretOffset > 0 && ( CaretOffset == Text.Length || Text[CaretOffset] == ' ' ) )
                        {
                            int iOffset = Text.LastIndexOf( ' ', CaretOffset - 1 );

                            iOffset++;

                            string strLookup = Text.Substring( iOffset, CaretOffset - iOffset );
                            if( strLookup != "" )
                            {
                                string strResult = LookupHandler( strLookup );
                                if( ! string.IsNullOrEmpty( strResult ) )
                                {
                                    Text = Text.Substring( 0, iOffset ) + strResult + Text.Substring( CaretOffset );
                                    CaretOffset = iOffset + strResult.Length;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        base.OnOSKeyPress( _key );
                    }
                    break;
                default:
                    base.OnOSKeyPress( _key );
                    break;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove( Direction _direction )
        {
            if( _direction == Direction.Left || _direction == Direction.Right )
            {
                // Horizontal pad move are eaten since left and right are used to move the caret
                return;
            }

            base.OnPadMove( _direction );
        }

        //----------------------------------------------------------------------
        protected internal override void OnFocus()
        {
            if( FocusHandler != null ) FocusHandler( this );
        }

        protected internal override void OnBlur()
        {
            if( BlurHandler != null ) BlurHandler( this );
        }

        //----------------------------------------------------------------------
        protected internal override void Update( float _fElapsedTime )
        {
            if( ! HasFocus )
            {
                mfTimer = 0f;
            }
            else
            {
                mfTimer += _fElapsedTime;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void Draw()
        {
            Screen.DrawBox( Frame, LayoutRect, FrameCornerSize, Color.White );

            if( Screen.IsActive && mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.ButtonPress, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White );
            }

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height ) );

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X - miScrollOffset, mpTextPosition.Y + mFont.YOffset ), Color.White );

            Screen.PopScissorRectangle();

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ) );

            const float fBlinkInterval = 0.3f;

            if( SelectionOffset != 0 )
            {
                Rectangle selectionRectangle;
                if( SelectionOffset > 0 )
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, LayoutRect.Y + Padding.Top, miSelectionX - miCaretX, LayoutRect.Height - Padding.Vertical );
                }
                else
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miSelectionX - miScrollOffset, LayoutRect.Y + Padding.Top, miCaretX - miSelectionX, LayoutRect.Height - Padding.Vertical );
                }

                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, selectionRectangle, Color.White * 0.3f );
            }
            else
            if( Screen.IsActive && HasFocus && mfTimer % (fBlinkInterval * 2) < fBlinkInterval )
            {
                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, LayoutRect.Y + Padding.Top, miCaretWidth, LayoutRect.Height - Padding.Vertical ), Color.White );
            }

            Screen.PopScissorRectangle();
        }
    }
}
