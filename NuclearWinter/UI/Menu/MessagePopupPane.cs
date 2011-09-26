using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public class MessagePopupPane: PopupPane<IMenuManager>
    {
        public Label                TitleLabel      { get; private set; }
        public Label                MessageLabel    { get; private set; }
        public Button               CloseButton     { get; private set; }

        FixedWidget                 mSpinningWheelAnchor;
        SpinningWheel               mSpinningWheel;

        public bool ShowSpinningWheel {
            set
            {
                if( value )
                {
                    mSpinningWheelAnchor.Child = mSpinningWheel;
                }
                else
                {
                    mSpinningWheelAnchor.Child = null;
                }
            }
        }

        //----------------------------------------------------------------------
        public MessagePopupPane( IMenuManager _manager )
        : base( _manager )
        {
            int iPadding = FixedGroup.Screen.Style.PopupFrameCornerSize;

            TitleLabel = new Label( Manager.MenuScreen, "", Anchor.Start );
            TitleLabel.Font = Manager.MenuScreen.Style.LargeFont;

            FixedGroup page = new FixedGroup( FixedGroup.Screen );
            mPageContainer.Child = page;

            page.AddChild( new FixedWidget( TitleLabel, AnchoredRect.CreateTopAnchored( iPadding, iPadding, iPadding, FixedGroup.Screen.Style.DefaultButtonHeight ) ) );

            {
                mSpinningWheel = new SpinningWheel( FixedGroup.Screen, FixedGroup.Screen.Style.SpinningWheel );
                mSpinningWheelAnchor = new FixedWidget( FixedGroup.Screen, AnchoredRect.CreateCentered( mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight ) );
                page.AddChild( mSpinningWheelAnchor );

                // Message label
                MessageLabel = new Label( FixedGroup.Screen, "", Anchor.Start );
                MessageLabel.WrapText = true;
                page.AddChild( new FixedWidget( MessageLabel, AnchoredRect.CreateTopAnchored( iPadding, iPadding + 80, iPadding, 200 ) ) );

                // Actions
                BoxGroup actionsGroup = new BoxGroup( FixedGroup.Screen, Orientation.Horizontal, 0 );
                page.AddChild( new FixedWidget( actionsGroup, AnchoredRect.CreateBottomAnchored( iPadding, iPadding, iPadding, FixedGroup.Screen.Style.DefaultButtonHeight ), Anchor.End ) );

                // Close
                // FIXME: i18n
                CloseButton = new Button( FixedGroup.Screen, "Close" );
                CloseButton.BindPadButton( Buttons.A );
                actionsGroup.AddChild( CloseButton );
            }
        }

        //----------------------------------------------------------------------
        public override void Open()
        {
            Manager.DisplayPopup( this );
            Focus();
        }

        //----------------------------------------------------------------------
        public override void Close()
        {
            TitleLabel.Text = "";
            MessageLabel.Text = "";
            CloseButton.ClickHandler = null;
            CloseButton.Text = "Close";

            ShowSpinningWheel = false;
            Manager.ClosePopup();
        }
    }
}
