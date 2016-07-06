using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#if !FNA
using OSKey = System.Windows.Forms.Keys;
#endif

namespace NuclearWinter.UI
{
    public class KeyBox : Widget
    {
        UIFont mFont;
        public UIFont Font
        {
            get { return mFont; }

            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        public bool StoreKeyAsUSEnglish = false;

        Keys mKey;
        public Keys Key
        {
            get { return mKey; }

            set
            {
                mKey = value;
                UpdateContentSize();
            }
        }

        public string DisplayedKey
        {
            get
            {
                return (StoreKeyAsUSEnglish ? NuclearWinter.LocalizedKeyboardState.USEnglishToLocal(Key) : Key).ToString();
            }
        }

        public Color TextColor;

        Point mpTextPosition;
        int miTextWidth;

        public Func<Keys, bool> ChangeHandler;
        public Action<KeyBox> FocusHandler;
        public Action<KeyBox> BlurHandler;

        bool mbIsHovered;

        //----------------------------------------------------------------------
        public KeyBox(Screen screen, Keys key)
        : base(screen)
        {
            mKey = key;
            mFont = screen.Style.MediumFont;
            mPadding = screen.Style.EditBoxPadding;

            TextColor = Screen.Style.EditBoxTextColor;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentHeight = (int)(Font.LineSpacing * 0.9f) + Padding.Top + Padding.Bottom;
            ContentWidth = 0;
            miTextWidth = (int)Font.MeasureString(DisplayedKey).X;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;

            mpTextPosition = new Point(
                LayoutRect.X + Padding.Left,
                LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            base.OnMouseEnter(hitPoint);
            mbIsHovered = true;
        }

        public override void OnMouseMove(Point hitPoint)
        {
        }

        public override void OnMouseOut(Point hitPoint)
        {
            base.OnMouseOut(hitPoint);
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            Screen.Focus(this);

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            if (HitTest(hitPoint) == this)
            {
                OnActivateUp();
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnFocus()
        {
            if (FocusHandler != null) FocusHandler(this);
        }

        protected internal override void OnBlur()
        {
            if (BlurHandler != null) BlurHandler(this);
        }

        protected internal override void OnPadMove(Direction direction)
        {
            // Nothing
        }

        protected internal override void OnOSKeyPress(OSKey key)
        {
            if (key == OSKey.Tab) return;

            base.OnOSKeyPress(key);
        }

        //----------------------------------------------------------------------
        protected internal override void OnKeyPress(Keys key)
        {
            Keys newKey = (key != Keys.Back) ? (StoreKeyAsUSEnglish ? NuclearWinter.LocalizedKeyboardState.LocalToUSEnglish(key) : key) : Keys.None;

            if (ChangeHandler == null || ChangeHandler(newKey))
            {
                Key = newKey;
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox(Screen.Style.EditBoxFrame, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White);

            if (Screen.IsActive && mbIsHovered)
            {
                Screen.DrawBox(Screen.Style.EditBoxHoverOverlay, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White);
            }

            Screen.PushScissorRectangle(new Rectangle(LayoutRect.X + Padding.Left, LayoutRect.Y, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height));

            Screen.Game.SpriteBatch.DrawString(mFont, DisplayedKey, new Vector2(mpTextPosition.X, mpTextPosition.Y + mFont.YOffset), TextColor);

            Screen.PopScissorRectangle();

            if (HasFocus)
            {
                Rectangle selectionRectangle = new Rectangle(mpTextPosition.X, LayoutRect.Y + Padding.Top, miTextWidth, LayoutRect.Height - Padding.Vertical);
                Screen.Game.SpriteBatch.Draw(Screen.Game.WhitePixelTex, selectionRectangle, TextColor * 0.3f);
            }
        }
    }
}
