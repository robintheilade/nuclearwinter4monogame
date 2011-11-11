using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public class MessagePopup: Popup<IMenuManager>
    {
        public Label                TitleLabel      { get; private set; }
        public Label                MessageLabel    { get; private set; }
        public Button               CloseButton     { get; private set; }

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
                MessageLabel.AnchoredRect = AnchoredRect.CreateTopAnchored( 0, 80, 0, 200 );
                Panel.AddChild( MessageLabel );

                // Actions
                BoxGroup actionsGroup = new BoxGroup( Panel.Screen, Orientation.Horizontal, 0, Anchor.End );
                actionsGroup.AnchoredRect = AnchoredRect.CreateBottomAnchored( 0, 0, 0, Panel.Screen.Style.DefaultButtonHeight );

                Panel.AddChild( actionsGroup );

                // Close
                CloseButton = new Button( Panel.Screen, i18n.Common.Close );
                CloseButton.BindPadButton( Buttons.A );
                actionsGroup.AddChild( CloseButton );
            }
        }

        //----------------------------------------------------------------------
        public override void Open()
        {
            Open( DefaultSize.X, DefaultSize.Y );
        }

        public void Open( int _iWidth, int _iHeight )
        {
            Panel.AnchoredRect.Width = _iWidth;
            Panel.AnchoredRect.Height = _iHeight;

            Manager.PopupGroup = Panel;
            Panel.Screen.Focus( Panel.GetFirstFocusableDescendant( Direction.Down ) );
        }

        public void Setup( string _strTitleText, string _strMessageText, string _strCloseButtonCaption, bool _bShowSpinningWheel )
        {
            TitleLabel.Text     = _strTitleText;
            MessageLabel.Text   = _strMessageText;
            CloseButton.Text    = _strCloseButtonCaption;
            ShowSpinningWheel   = _bShowSpinningWheel;
        }

        //----------------------------------------------------------------------
        void Close()
        {
            TitleLabel.Text = "";
            MessageLabel.Text = "";
            CloseButton.ClickHandler = null;
            CloseButton.Text = "Close";

            ShowSpinningWheel = false;
            Manager.PopupGroup = null;
        }
    }
}
