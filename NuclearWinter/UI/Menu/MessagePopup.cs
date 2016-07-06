using System;

namespace NuclearWinter.UI
{
    public class MessagePopup : Popup<IMenuManager>
    {
        public Label TitleLabel { get; private set; }
        public Label MessageLabel { get; private set; }

        public Group ContentGroup { get; private set; }

        BoxGroup mActionsGroup;

        Button mCloseButton;
        Button mConfirmButton;

        Action mCloseCallback;
        Action<bool> mConfirmCallback;

        SpinningWheel mSpinningWheel;

        public bool ShowSpinningWheel
        {
            set
            {
                if (value)
                {
                    if (mSpinningWheel.Parent == null)
                    {
                        AddChild(mSpinningWheel);
                    }
                }
                else
                {
                    if (mSpinningWheel.Parent != null)
                    {
                        RemoveChild(mSpinningWheel);
                    }
                }
            }
        }

        //----------------------------------------------------------------------
        public MessagePopup(IMenuManager manager)
        : base(manager)
        {
            TitleLabel = new Label(Screen, "", Anchor.Start);
            TitleLabel.Font = Screen.Style.LargeFont;
            TitleLabel.AnchoredRect = AnchoredRect.CreateTopAnchored(0, 0, 0, Screen.Style.DefaultButtonHeight);
            AddChild(TitleLabel);

            {
                mSpinningWheel = new SpinningWheel(Screen, Screen.Style.SpinningWheel);
                mSpinningWheel.AnchoredRect = AnchoredRect.CreateCentered(mSpinningWheel.ContentWidth, mSpinningWheel.ContentHeight);

                // Message label
                ContentGroup = new Group(Screen);
                ContentGroup.AnchoredRect = AnchoredRect.CreateFull(0, Screen.Style.DefaultButtonHeight + 10, 0, Screen.Style.DefaultButtonHeight + 10);
                AddChild(ContentGroup);

                MessageLabel = new Label(Screen, "", Anchor.Start);
                MessageLabel.WrapText = true;

                // Actions
                mActionsGroup = new BoxGroup(Screen, Orientation.Horizontal, 0, Anchor.End);
                mActionsGroup.AnchoredRect = AnchoredRect.CreateBottomAnchored(0, 0, 0, Screen.Style.DefaultButtonHeight);

                AddChild(mActionsGroup);

                // Close / Cancel
                mCloseButton = new Button(Screen, i18n.Common.Close);
                mCloseButton.ClickHandler = delegate { Dismiss(); };

                // Confirm
                mConfirmButton = new Button(Screen, i18n.Common.Confirm);
                mConfirmButton.ClickHandler = delegate { Confirm(); };
                mActionsGroup.AddChild(mConfirmButton);
            }
        }

        //----------------------------------------------------------------------
        public void Open(int width, int height)
        {
            AnchoredRect.Width = width;
            AnchoredRect.Height = height;

            Manager.PushPopup(this);
            Screen.Focus(GetFirstFocusableDescendant(Direction.Down));

            mSpinningWheel.Reset();
        }

        //----------------------------------------------------------------------
        public void Setup(string titleText, string messageText, string closeButtonCaption, bool showSpinningWheel = false, Action closeCallback = null)
        {
            TitleLabel.Text = titleText;

            if (messageText != null)
            {
                MessageLabel.Text = messageText;
                ContentGroup.Clear();
                ContentGroup.AddChild(MessageLabel);
            }

            mCloseButton.Text = closeButtonCaption;
            ShowSpinningWheel = showSpinningWheel;

            mActionsGroup.Clear();
            mActionsGroup.AddChild(mCloseButton);
            mCloseCallback = closeCallback;
        }

        //----------------------------------------------------------------------
        public void Setup(string titleText, string messageText, string confirmButtonCaption, string closeButtonCaption, Action<bool> confirmCallback = null)
        {
            TitleLabel.Text = titleText;

            if (messageText != null)
            {
                MessageLabel.Text = messageText;
                ContentGroup.Clear();
                ContentGroup.AddChild(MessageLabel);
            }

            mConfirmButton.Text = confirmButtonCaption;
            mCloseButton.Text = closeButtonCaption;

            mActionsGroup.Clear();
            mActionsGroup.AddChild(mConfirmButton);
            mActionsGroup.AddChild(mCloseButton);

            mConfirmCallback = confirmCallback;
        }

        //----------------------------------------------------------------------
        public override void Close()
        {
            TitleLabel.Text = "";
            MessageLabel.Text = "";
            mConfirmButton.Text = i18n.Common.Confirm;
            mCloseButton.Text = i18n.Common.Close;

            ShowSpinningWheel = false;
            mCloseCallback = null;
            mConfirmCallback = null;

            ContentGroup.Clear();

            base.Close();
        }

        //----------------------------------------------------------------------
        protected override void Dismiss()
        {
            var closeCallback = mCloseCallback;
            var confirmCallback = mConfirmCallback;
            base.Dismiss();
            if (closeCallback != null) closeCallback();
            if (confirmCallback != null) confirmCallback(false);
        }

        //----------------------------------------------------------------------
        protected void Confirm()
        {
            var confirmCallback = mConfirmCallback;
            Close();
            confirmCallback(true);
        }
    }
}
