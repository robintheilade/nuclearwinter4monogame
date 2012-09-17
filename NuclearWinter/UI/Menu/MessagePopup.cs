using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;
using System;

namespace NuclearWinter.UI
{
    public class MessagePopup: Popup<IMenuManager>
    {
        public Label                TitleLabel      { get; private set; }
        public Label                MessageLabel    { get; private set; }

        Button                      mCloseButton;

        SpinningWheel               mSpinningWheel;

        public bool ShowSpinningWheel {
            set
            {
                if( value )
                {
                    if( mSpinningWheel.Parent == null )
                    {
                        AddChild( mSpinningWheel );
                    }
                }
                else
                {
                    if( mSpinningWheel.Parent != null )
                    {
                        RemoveChild( mSpinningWheel );
                    }
                }
            }
        }

        Action                  mCloseCallback;

        //----------------------------------------------------------------------
        public MessagePopup( IMenuManager _manager )
        : base( _manager )
        {
            TitleLabel = new Label( Screen, "", Anchor.Start );
            TitleLabel.Font = Screen.Style.LargeFont;
            TitleLabel.AnchoredRect = AnchoredRect.CreateTopAnchored( 0, 0, 0, Screen.Style.DefaultButtonHeight );
            AddChild( TitleLabel );

            {
                mSpinningWheel = new SpinningWheel( Screen, Screen.Style.SpinningWheel );
                mSpinningWheel.AnchoredRect = AnchoredRect.CreateCentered( mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight );

                // Message label
                MessageLabel = new Label( Screen, "", Anchor.Start );
                MessageLabel.WrapText = true;
                MessageLabel.AnchoredRect = AnchoredRect.CreateFull( 0, Screen.Style.DefaultButtonHeight + 10, 0, Screen.Style.DefaultButtonHeight + 10 );
                AddChild( MessageLabel );

                // Actions
                BoxGroup actionsGroup = new BoxGroup( Screen, Orientation.Horizontal, 0, Anchor.End );
                actionsGroup.AnchoredRect = AnchoredRect.CreateBottomAnchored( 0, 0, 0, Screen.Style.DefaultButtonHeight );

                AddChild( actionsGroup );

                // Close
                mCloseButton = new Button( Screen, i18n.Common.Close );
                mCloseButton.BindPadButton( Buttons.A );
                actionsGroup.AddChild( mCloseButton );
            }
        }

        //----------------------------------------------------------------------
        public void Open()
        {
            Open( DefaultSize.X, DefaultSize.Y );
        }

        //----------------------------------------------------------------------
        public void Open( int _iWidth, int _iHeight )
        {
            AnchoredRect.Width = _iWidth;
            AnchoredRect.Height = _iHeight;

            Manager.PushPopup( this );
            Screen.Focus( GetFirstFocusableDescendant( Direction.Down ) );

            mSpinningWheel.Reset();
        }

        //----------------------------------------------------------------------
        public void Setup( string _strTitleText, string _strMessageText, string _strCloseButtonCaption, bool _bShowSpinningWheel, Action _closeCallback=null )
        {
            TitleLabel.Text     = _strTitleText;
            MessageLabel.Text   = _strMessageText;
            mCloseButton.Text    = _strCloseButtonCaption;
            mCloseButton.ClickHandler = delegate { Dismiss(); };
            ShowSpinningWheel   = _bShowSpinningWheel;

            mCloseCallback = _closeCallback;
        }

        //----------------------------------------------------------------------
        public override void Close()
        {
            TitleLabel.Text = "";
            MessageLabel.Text = "";
            mCloseButton.ClickHandler = null;
            mCloseButton.Text = "Close";

            ShowSpinningWheel = false;
            mCloseCallback = null;

            base.Close();
        }

        //----------------------------------------------------------------------
        protected override void Dismiss()
        {
            Action closeCallback = mCloseCallback;
            base.Dismiss();
            if( closeCallback != null ) closeCallback();
        }
    }
}
