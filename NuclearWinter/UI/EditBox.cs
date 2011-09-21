using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    /*
     * An EditBox to enter some text
     */
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
        int     CaretOffset {
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
        int     SelectionOffset {
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

            int iScrollStep = Size.X / 3;

            if( Size.X != 0 && iTarget > miScrollOffset + ( Size.X - Padding.Horizontal ) - miCaretWidth )
            {
                miScrollOffset = Math.Min( miMaxScrollOffset, ( iTarget - ( Size.X - Padding.Horizontal ) + miCaretWidth ) + iScrollStep );
            }
            else
            if( iTarget < miScrollOffset )
            {
                miScrollOffset = Math.Max( 0, iTarget - iScrollStep );
            }
        }

        bool    mbIsHovered;
        float   mfTimer;

        public const char DefaultPasswordChar = '●';

        char mPasswordCharacter = '\0';
        public char PasswordChar {
            get { return mPasswordCharacter; }
            set { mPasswordCharacter = value; UpdateContentSize(); }
        }

        public string Text
        {
            get { return mstrText; }
            set
            {
                mstrText = value;
                CaretOffset = Math.Min( miCaretOffset, mstrText.Length );

                UpdateContentSize();
            }
        }

        string                  mstrDisplayedText;
        int                     miScrollOffset;
        int                     miMaxScrollOffset;

        public Func<char,bool>  TextEnteredHandler;

        public static bool IntegerValidator( char _char )   { return ( _char >= '0' && _char <= '9' ) || _char == '-'; }
        public static bool FloatValidator( char _char )     { return ( _char >= '0' && _char <= '9' ) || _char == '.' || _char == '-'; }

        public Action<EditBox>  ValidateHandler;

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
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            if( SelectionOffset > 0 )
            {
                Text = Text.Remove( CaretOffset, SelectionOffset );
                SelectionOffset = 0;
            }
            else
            {
                int iNewCaretOffset = CaretOffset + SelectionOffset;
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

                Text = Text.Insert( CaretOffset, strPastedText );
                CaretOffset += strPastedText.Length;
            }
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            mstrDisplayedText = ( mPasswordCharacter == '\0' ) ? Text : "".PadLeft( Text.Length, mPasswordCharacter );

            miMaxScrollOffset = (int)Math.Max( 0, Font.MeasureString( mstrDisplayedText ).X - ( Size.X - Padding.Horizontal ) + miCaretWidth );
            ContentWidth = 0; //(int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            mpTextPosition = new Point(
                Position.X + Padding.Left,
                pCenter.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            Screen.Focus( this );
            OnActivateDown();
        }

        internal override void OnMouseUp( Point _hitPoint )
        {
            if( HitTest( _hitPoint ) == this )
            {
                OnActivateUp();
            }
            else
            {
            }
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

                Text = Text.Insert( CaretOffset, _char.ToString() );
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
            Screen.AddWidgetToUpdateList( this );
        }

        internal override bool Update( float _fElapsedTime )
        {
            if( ! HasFocus )
            {
                mfTimer = 0f;
                return false;
            }

            mfTimer += _fElapsedTime;

            return true;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Rectangle rect = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );

            Screen.DrawBox( Screen.Style.EditBoxFrame, rect, Screen.Style.EditBoxCornerSize, Color.White );

            if( Screen.IsActive && mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.ButtonPress, rect, Screen.Style.EditBoxCornerSize, Color.White );
            }

            Screen.PushScissorRectangle( new Rectangle( Position.X + Padding.Left, Position.Y + Padding.Top, Size.X - Padding.Horizontal, Size.Y - Padding.Vertical ) );

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X - miScrollOffset, mpTextPosition.Y + mFont.YOffset ), Color.White );

            const float fBlinkInterval = 0.3f;

            if( SelectionOffset != 0 )
            {
                Rectangle selectionRectangle;
                if( SelectionOffset > 0 )
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, Position.Y + Padding.Top, miSelectionX - miCaretX, Size.Y - Padding.Vertical );
                }
                else
                {
                    selectionRectangle = new Rectangle( mpTextPosition.X + miSelectionX - miScrollOffset, Position.Y + Padding.Top, miCaretX - miSelectionX, Size.Y - Padding.Vertical );
                }

                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, selectionRectangle, Color.White * 0.3f );
            }
            else
            if( Screen.IsActive && HasFocus && mfTimer % (fBlinkInterval * 2) < fBlinkInterval )
            {
                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, Position.Y + Padding.Top, miCaretWidth, Size.Y - Padding.Vertical ), Color.White );
            }

            Screen.PopScissorRectangle();
        }
    }
}
