using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NuclearWinter.UI
{
    /*
     * A Image to be displayed
     */
    public class Image : Widget
    {
        protected bool mbStretch;
        public bool Stretch
        {
            get { return mbStretch; }
            set { mbStretch = value; }
        }

        public Color Color = Color.White;

        public Action<Image> ClickHandler;
        public Action<Image> MouseEnterHandler;
        public Action<Image> MouseOutHandler;
        public Action<Image> MouseDownHandler;
        public Action<Image, float> UpdateHandler;

        //----------------------------------------------------------------------
        protected Texture2D mTexture;
        public Texture2D Texture
        {
            get { return mTexture; }
            set
            {
                mTexture = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant(Direction direction)
        {
            return null;
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            int iWidth = mTexture != null ? mTexture.Width : 0;
            int iHeight = mTexture != null ? mTexture.Height : 0;

            ContentWidth = iWidth + Padding.Horizontal;
            ContentHeight = iHeight + Padding.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public Image(Screen screen, Texture2D texture = null, bool stretch = false)
        : base(screen)
        {
            mTexture = texture;
            mbStretch = stretch;
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            if (ClickHandler != null)
            {
                Screen.Game.SetCursor(MouseCursor.Hand);
            }

            if (MouseEnterHandler != null) MouseEnterHandler(this);
        }

        public override void OnMouseOut(Point hitPoint)
        {
            if (ClickHandler != null)
            {
                Screen.Game.SetCursor(MouseCursor.Default);
            }

            if (MouseOutHandler != null) MouseOutHandler(this);
        }

        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (MouseDownHandler != null)
            {
                MouseDownHandler(this);
                return true;
            }

            return ClickHandler != null;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            if (ClickHandler != null)
            {
                ClickHandler(this);
            }
        }

        public override void Update(float elapsedTime)
        {
            if (UpdateHandler != null)
            {
                UpdateHandler(this, elapsedTime);
            }

            base.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            Point pCenter = LayoutRect.Center;

            HitBox = mbStretch ? LayoutRect : new Rectangle(
                pCenter.X - ContentWidth / 2,
                pCenter.Y - ContentHeight / 2,
                ContentWidth,
                ContentHeight
            );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if (mTexture == null) return;

            if (!mbStretch)
            {
                Screen.Game.SpriteBatch.Draw(mTexture, new Vector2(LayoutRect.Center.X - ContentWidth / 2 + Padding.Left, LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top), Color);
            }
            else
            {
                Screen.Game.SpriteBatch.Draw(mTexture, new Rectangle(LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical), Color);
            }
        }
    }
}
