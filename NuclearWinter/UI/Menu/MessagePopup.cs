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
                        Panel.AddChild( mSpinningWheel );
                    }
                }
                else
                {
                    if( mSpinningWheel.Parent != null )
                    {
                        Panel.RemoveChild( mSpinningWheel );
                    }
                }
            }
        }

        //----------------------------------------------------------------------
        public MessagePopup( IMenuManager _manager )
        : base( _manager )
        {
            TitleLabel = new Label( Panel.Screen, "", Anchor.Start );
            TitleLabel.Font = Panel.Screen.Style.LargeFont;
            TitleLabel.AnchoredRect = AnchoredRect.CreateTopAnchored( 0, 0, 0, Panel.Screen.Style.DefaultButtonHeight );
            Panel.AddChild( TitleLabel );

            {
                mSpinningWheel = new SpinningWheel( Panel.Screen, Panel.Screen.Style.SpinningWheel );
                mSpinningWheel.AnchoredRect = AnchoredRect.CreateCentered( mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight );

                // Message label
                MessageLabel = new Label( Panel.Screen, "", Anchor.Start );
                MessageLabel.WrapText = true;
                MessageLabel.AnchoredRect = AnchoredRect.CreateFull( 0, Panel.Screen.Style.DefaultButtonHeight + 10, 0, Panel.Screen.Style.DefaultButtonHeight + 10 );
                Panel.AddChild( MessageLabel );

                // Actions
                BoxGroup actionsGroup = new BoxGroup( Panel.Screen, Orientation.Horizontal, 0, Anchor.End );
                actionsGroup.AnchoredRect = AnchoredRect.CreateBottomAnchored( 0, 0, 0, Panel.Screen.Style.DefaultButtonHeight );

                Panel.AddChild( actionsGroup );

                // Close
                mCloseButton = new Button( Panel.Screen, i18n.Common.Close );
                mCloseButton.BindPadButton( Buttons.A );
                actionsGroup.AddChild( mCloseButton );
            }
        }

        //----------------------------------------------------------------------
        public void Open()
        {
            Open( DefaultSize.X, DefaultSize.Y );
        }

        public void Open( int _iWidth, int _iHeight )
        {
            Panel.AnchoredRect.Width = _iWidth;
            Panel.AnchoredRect.Height = _iHeight;

            Manager.PushPopup( this );
            Panel.Screen.Focus( Panel.GetFirstFocusableDescendant( Direction.Down ) );

            mSpinningWheel.Reset();
        }

        public void Setup( string _strTitleText, string _strMessageText, string _strCloseButtonCaption, bool _bShowSpinningWheel, Action _closeCallback=null )
        {
            TitleLabel.Text     = _strTitleText;
            MessageLabel.Text   = _strMessageText;
            mCloseButton.Text    = _strCloseButtonCaption;
            mCloseButton.ClickHandler = delegate { ( _closeCallback ?? Close )(); };
            ShowSpinningWheel   = _bShowSpinningWheel;
        }

        //----------------------------------------------------------------------
        public void Close()
        {
            TitleLabel.Text = "";
            MessageLabel.Text = "";
            mCloseButton.ClickHandler = null;
            mCloseButton.Text = "Close";

            ShowSpinningWheel = false;
            Manager.PopPopup( this );
        }
    }
}
