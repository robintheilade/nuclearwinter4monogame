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

        NuclearWinter.UI.WidgetProxy          mSpinningWheelAnchor;
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

            Rectangle paneRect = new Rectangle( Resolution.InternalMode.Width / 2 - Width / 2, Resolution.InternalMode.Height / 2 - Height / 2, Width, Height );

            NuclearWinter.UI.Panel panel = new NuclearWinter.UI.Panel( FixedGroup.Screen, Manager.Content.Load<Texture2D>( "Sprites/Menu/PanelFrame" ), 20 );
            FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( panel, paneRect ) );

            TitleLabel = new NuclearWinter.UI.Label( Manager.MenuScreen, "", NuclearWinter.UI.Anchor.Start );
            TitleLabel.Font = Manager.MenuScreen.Style.BigFont;
            FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( TitleLabel, new Rectangle( paneRect.Left + 20, paneRect.Top + 20, paneRect.Width - 40, 80 ) ) );

            {
                mSpinningWheel = new NuclearWinter.UI.SpinningWheel( FixedGroup.Screen, Manager.Content.Load<Texture2D>( "Sprites/Menu/SpinningWheel" ) );
                mSpinningWheelAnchor = new NuclearWinter.UI.WidgetProxy( FixedGroup.Screen, null );
                FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( mSpinningWheelAnchor, new Rectangle( paneRect.Center.X - mSpinningWheel.ContentWidth / 2, paneRect.Center.Y - mSpinningWheel.ContentHeight / 2, mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight ) ) );

                // Hostname label
                MessageLabel = new NuclearWinter.UI.Label( FixedGroup.Screen, "", NuclearWinter.UI.Anchor.Start );
                MessageLabel.WrapText = true;

                FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( MessageLabel, new Rectangle( paneRect.Left + 20, paneRect.Top + 100, paneRect.Width - 40, 100 ) ) );

                // Actions
                NuclearWinter.UI.BoxGroup actionsGroup = new NuclearWinter.UI.BoxGroup( FixedGroup.Screen, NuclearWinter.UI.Direction.Left, false, 10 );
                FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( actionsGroup, new Rectangle( paneRect.Left + 20, paneRect.Bottom - 20 - 80, paneRect.Width - 40, 80 ) ) );

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
