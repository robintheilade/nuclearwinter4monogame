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
            FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( panel, paneRect ) );

            TitleLabel = new NuclearWinter.UI.Label( Manager.MenuScreen, "", NuclearWinter.UI.Anchor.Start );
            TitleLabel.Font = Manager.MenuScreen.Style.BigFont;

            FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( TitleLabel, new Rectangle( paneRect.Left + iPadding, paneRect.Top + iPadding, paneRect.Width - iPadding * 2, 80 ) ) );

            {
                mSpinningWheel = new NuclearWinter.UI.SpinningWheel( FixedGroup.Screen, Manager.Content.Load<Texture2D>( "Sprites/Menu/SpinningWheel" ) );
                mSpinningWheelAnchor = new NuclearWinter.UI.FixedWidget( FixedGroup.Screen, new Rectangle( paneRect.Center.X - mSpinningWheel.ContentWidth / 2, paneRect.Center.Y - mSpinningWheel.ContentHeight / 2, mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight ) );
                FixedGroup.AddChild( mSpinningWheelAnchor );

                // Message label
                MessageLabel = new NuclearWinter.UI.Label( FixedGroup.Screen, "", NuclearWinter.UI.Anchor.Start );
                MessageLabel.WrapText = true;

                FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( MessageLabel, new Rectangle( paneRect.Left + iPadding, paneRect.Top + iPadding + 80, paneRect.Width - iPadding * 2, 100 ) ) );

                // Actions
                NuclearWinter.UI.BoxGroup actionsGroup = new NuclearWinter.UI.BoxGroup( FixedGroup.Screen, NuclearWinter.UI.Direction.Left, false, 10 );
                FixedGroup.AddChild( new NuclearWinter.UI.FixedWidget( actionsGroup, new Rectangle( paneRect.Left + iPadding, paneRect.Bottom - iPadding - FixedGroup.Screen.Style.DefaultButtonHeight, paneRect.Width - iPadding * 2, FixedGroup.Screen.Style.DefaultButtonHeight ) ) );

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
