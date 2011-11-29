using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        int     miCaretWidth = 3;

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

            int iScrollStep = LayoutRect.X / 3;

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
                }
            }
        }

        string                  mstrDisplayedText;
        int                     miScrollOffset;
        int                     miMaxScrollOffset;

        public Func<char,bool>  TextEnteredHandler;

        public static bool IntegerValidator( char _char )   { return ( _char >= '0' && _char <= '9' ) || _char == '-'; }
        public static bool FloatValidator( char _char )     { return ( _char >= '0' && _char <= '9' ) || _char == '.' || _char == '-'; }

        public Action<EditBox>  ValidateHandler;
        public Action<EditBox>  FocusHandler;
        public Action<EditBox>  BlurHandler;

        public Action<EditBox,int,string>   TextInsertedHandler;
        public Action<EditBox,int,int>      TextRemovedHandler;

        public bool             IsReadOnly;
        public int              MaxLength;

        //----------------------------------------------------------------------
        public EditBox( Screen _screen, string _strText = "", Func<char,bool> _textEnteredHandler = null )
        : base( _screen )
        {
            mstrText    = _strText;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 15 );
            TextEnteredHandler = _textEnteredHandler;

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
            string strPastedText = (string)System.Windows.Forms.Clipboard.GetData( typeof(string).FullName );
            if( strPastedText != null )
            {
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
        internal override void UpdateContentSize()
        {
            mstrDisplayedText = ( mPasswordChar == '\0' ) ? Text : "".PadLeft( Text.Length, mPasswordChar );

            miMaxScrollOffset = (int)Math.Max( 0, Font.MeasureString( mstrDisplayedText ).X - ( LayoutRect.Width - Padding.Horizontal ) + miCaretWidth );
            ContentWidth = 0; //(int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            mpTextPosition = new Point(
                LayoutRect.X + Padding.Left,
                LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                int iOffset = GetCaretOffsetAtX( Math.Max( 0, _hitPoint.X - ( LayoutRect.X + Padding.Left ) ) );
                SelectionOffset = iOffset - miCaretOffset;
            }
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

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

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

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
        internal override void OnTextEntered( char _char )
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
                    if( bCtrl && mPasswordChar == '\0' )
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case Keys.C:
                    if( bCtrl && mPasswordChar == '\0' )
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
                    if( ! IsReadOnly && ValidateHandler != null ) ValidateHandler( this );
                    break;
                case Keys.Back:
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
                case Keys.Delete:
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
                case Keys.Left:
                    if( bShift )
                    {
                        if( bCtrl )
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
                                iNewCaretOffset = Text.LastIndexOf( ' ', iNewCaretOffset - 1, iNewCaretOffset - 1 ) + 1;
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
                case Keys.Right:
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
                case Keys.End:
                    if( bShift )
                    {
                        SelectionOffset = Text.Length - CaretOffset;
                    }
                    else
                    {
                        CaretOffset = Text.Length;
                    }
                    break;
                case Keys.Home:
                    if( bShift )
                    {
                        SelectionOffset = -CaretOffset;
                    }
                    else
                    {
                        CaretOffset = 0;
                    }
                    break;
                default:
                    base.OnKeyPress( _key );
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnPadMove( Direction _direction )
        {
            if( _direction == Direction.Left || _direction == Direction.Right )
            {
                // Horizontal pad move are eaten since left and right are used to move the caret
                return;
            }

            base.OnPadMove( _direction );
        }

        //----------------------------------------------------------------------
        internal override void OnFocus()
        {
            if( FocusHandler != null ) FocusHandler( this );
        }

        internal override void OnBlur()
        {
            if( BlurHandler != null ) BlurHandler( this );
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
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
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.EditBoxFrame, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White );

            if( Screen.IsActive && mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.ButtonPress, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White );
            }

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical ) );

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X - miScrollOffset, mpTextPosition.Y + mFont.YOffset ), Color.White );

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
