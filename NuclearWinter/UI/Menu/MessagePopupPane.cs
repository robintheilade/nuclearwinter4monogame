using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public class MessagePopupPane: PopupPane<IMenuManager>
    {
        public const int        Width   = 800;
        public const int        Height  = 450;

        public string           Hostname;

        public NuclearWinter.UI.Label         TitleLabel      { get; private set; }
        public NuclearWinter.UI.Label         MessageLabel    { get; private set; }
        public NuclearWinter.UI.Button        CloseButton     { get; private set; }

        NuclearWinter.UI.FixedWidget          mSpinningWheelAnchor;
        NuclearWinter.UI.SpinningWheel        mSpinningWheel;

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
            FixedGroup = new NuclearWinter.UI.FixedGroup( Manager.PopupScreen );

            int iPadding = FixedGroup.Screen.Style.PopupFrameCornerSize;

            Rectangle paneRect = new Rectangle(
                Manager.PopupScreen.Width / 2 - Width / 2 - iPadding,
                Manager.PopupScreen.Height / 2 - Height / 2 - iPadding, 
                Width + iPadding * 2, Height + iPadding * 2 );

            NuclearWinter.UI.Panel panel = new NuclearWinter.UI.Panel( FixedGroup.Screen, FixedGroup.Screen.Style.PopupFrame, FixedGroup.Screen.Style.PopupFrameCornerSize );
            FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( panel, AnchoredRect.CreateCentered( paneRect.Width, paneRect.Height ) ) );

            FixedGroup page = new FixedGroup( FixedGroup.Screen );
            FixedGroup.AddChild( new FixedWidget( page, AnchoredRect.CreateCentered( paneRect.Width, paneRect.Height ) ) );

            TitleLabel = new NuclearWinter.UI.Label( Manager.MenuScreen, "", NuclearWinter.UI.Anchor.Start );
            TitleLabel.Font = Manager.MenuScreen.Style.BigFont;
            page.AddChild( new FixedWidget( TitleLabel, AnchoredRect.CreateTopAnchored( iPadding, iPadding, iPadding, 50 ) ) );

            {
                mSpinningWheel = new NuclearWinter.UI.SpinningWheel( FixedGroup.Screen, Manager.Content.Load<Texture2D>( "Sprites/Menu/SpinningWheel" ) );
                mSpinningWheelAnchor = new NuclearWinter.UI.FixedWidget( FixedGroup.Screen, AnchoredRect.CreateCentered( mSpinningWheel.ContentWidth , mSpinningWheel.ContentHeight ) );
                page.AddChild( mSpinningWheelAnchor );

                // Message label
                MessageLabel = new NuclearWinter.UI.Label( FixedGroup.Screen, "", NuclearWinter.UI.Anchor.Start );
                MessageLabel.WrapText = true;
                page.AddChild( new NuclearWinter.UI.FixedWidget( MessageLabel, AnchoredRect.CreateTopAnchored( iPadding, iPadding + 80, iPadding, 100 ) ) );

                // Actions
                NuclearWinter.UI.BoxGroup actionsGroup = new NuclearWinter.UI.BoxGroup( FixedGroup.Screen, NuclearWinter.UI.Direction.Left, 0 );
                page.AddChild( new FixedWidget( actionsGroup, AnchoredRect.CreateBottomAnchored( iPadding, iPadding, iPadding, 60 ), Anchor.End ) );

                // Close
                // FIXME: i18n
                CloseButton = new NuclearWinter.UI.Button( FixedGroup.Screen, "Close" );
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
            ShowSpinningWheel = false;
            Manager.ClosePopup();
        }
    }
}
