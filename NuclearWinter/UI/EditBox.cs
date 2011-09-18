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
                miCaretX = miCaretOffset > 0 ? (int)mFont.MeasureString( mstrDisplayedText.Substring( 0, miCaretOffset ) ).X : 0;

                int iScrollStep = Size.X / 3;

                if( Size.X != 0 && miCaretX > miScrollOffset + ( Size.X - Padding.Horizontal ) - miCaretWidth )
                {
                    miScrollOffset = Math.Min( miMaxScrollOffset, ( miCaretX - ( Size.X - Padding.Horizontal ) + miCaretWidth ) + iScrollStep );
                }
                else
                if( miCaretX < miScrollOffset )
                {
                    miScrollOffset = Math.Max( 0, miCaretX - iScrollStep );
                }
                mfTimer = 0f;
            }
        }
        int     miCaretX;
        int     miCaretWidth = 3;

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
            if( ! IsReadOnly && ( MaxLength == 0 || Text.Length < MaxLength ) && ! char.IsControl( _char ) && ( TextEnteredHandler == null || TextEnteredHandler( _char ) ) )
            {
                Text = Text.Insert( CaretOffset, _char.ToString() );
                CaretOffset++;
            }
        }
        
        internal override void OnKeyPress( Keys _key )
        {
            switch( _key )
            {
                case Keys.Enter:
                    if( ! IsReadOnly && ValidateHandler != null ) ValidateHandler( this );
                    break;
                case Keys.Back:
                    if( ! IsReadOnly && Text.Length > 0 && CaretOffset > 0 )
                    {
                        CaretOffset--;
                        Text = Text.Remove( CaretOffset, 1 );
                    }
                    break;
                case Keys.Delete:
                    if( ! IsReadOnly && Text.Length > 0 && CaretOffset < Text.Length )
                    {
                        Text = Text.Remove( CaretOffset, 1 );
                    }
                    break;
                case Keys.Left:
                    CaretOffset--;
                    break;
                case Keys.Right:
                    CaretOffset++;
                    break;
                case Keys.End:
                    CaretOffset = Text.Length;
                    break;
                case Keys.Home:
                    CaretOffset = 0;
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

            if( Screen.IsActive && HasFocus && mfTimer % (fBlinkInterval * 2) < fBlinkInterval )
            {
                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, new Rectangle( mpTextPosition.X + miCaretX - miScrollOffset, Position.Y + Padding.Top, miCaretWidth, Size.Y - Padding.Vertical ), Color.White );
            }

            Screen.PopScissorRectangle();
        }
    }
}
