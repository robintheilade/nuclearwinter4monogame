﻿using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#if !FNA
using OSKey = System.Windows.Forms.Keys;
#endif

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    // An EditBox to enter some text
    public class EditBox : Widget
    {
        public UIFont Font
        {
            get { return mFont; }

            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        public int CaretOffset
        {
            get { return miCaretOffset; }
            set
            {
                miCaretOffset = (int)MathHelper.Clamp(value, 0, mstrText.Length);
                miSelectionOffset = 0;
                ComputeCaretAndSelectionX();
                mfCaretTimer = 0f;
            }
        }

        public int SelectionOffset
        {
            get { return miSelectionOffset; }
            set
            {
                miSelectionOffset = (int)MathHelper.Clamp(value, -miCaretOffset, mstrText.Length - miCaretOffset);
                ComputeCaretAndSelectionX();
            }
        }

        void ComputeCaretAndSelectionX()
        {
            miCaretX = miCaretOffset > 0 ? (int)mFont.MeasureString(mstrDisplayedText.Substring(0, miCaretOffset)).X : 0;
            int iTarget = miCaretX;

            if (miSelectionOffset != 0)
            {
                miSelectionX = (miCaretOffset + miSelectionOffset) > 0 ? (int)mFont.MeasureString(mstrDisplayedText.Substring(0, miCaretOffset + miSelectionOffset)).X : 0;
                iTarget = miSelectionX;
            }

            int iScrollStep = LayoutRect.Width / 3;

            if (EnableScrolling)
            {
                if (LayoutRect.Width != 0 && iTarget > miScrollOffset + (LayoutRect.Width - Padding.Horizontal) - Screen.Style.CaretWidth)
                {
                    miScrollOffset = Math.Min(miMaxScrollOffset, (iTarget - (LayoutRect.Width - Padding.Horizontal) + Screen.Style.CaretWidth) + iScrollStep);
                }
                else
                if (iTarget < miScrollOffset)
                {
                    miScrollOffset = Math.Max(0, iTarget - iScrollStep);
                }
            }
        }

        public const char DefaultPasswordChar = '●';

        public char PasswordChar
        {
            get { return mPasswordChar; }
            set { mPasswordChar = value; UpdateContentSize(); }
        }

        public string Text
        {
            get { return mstrText; }
            set
            {
                if (mstrText != value)
                {
                    mstrText = value;
                    CaretOffset = Math.Min(miCaretOffset, mstrText.Length);

                    UpdateContentSize();
                    miScrollOffset = Math.Min(miScrollOffset, miMaxScrollOffset);
                }
            }
        }


        public bool EnableScrolling = true;

        public Color TextColor;
        public int TextWidth { get; private set; }

        public Func<char, bool> TextEnteredHandler;
        public Func<string, string> LookupHandler;

        public static bool IntegerValidator(char _char) { return (_char >= '0' && _char <= '9') || _char == '-'; }
        public static bool FloatValidator(char _char) { return (_char >= '0' && _char <= '9') || _char == '.' || _char == '-'; }

        public Action<EditBox> ValidateHandler;
        public Action<EditBox> FocusHandler;
        public Action<EditBox> BlurHandler;

        public Action<EditBox, int, string> TextInsertedHandler;
        public Action<EditBox, int, int> TextRemovedHandler;

        public bool IsReadOnly;
        public int MaxLength;

        public string PlaceholderText
        {
            get { return mstrPlaceholderText; }
            set { mstrPlaceholderText = value; UpdateContentSize(); }
        }

        public bool CanBeEscapeCleared;

        public Texture2D Frame;
        public int FrameCornerSize;

        //----------------------------------------------------------------------
        UIFont mFont;
        string mstrText;
        string mstrDisplayedText;

        Point mpTextPosition;

        int miCaretX;
        int miSelectionOffset;
        int miSelectionX;
        int miCaretOffset;
        float mfCaretTimer;

        char mPasswordChar = '\0';
        string mstrPlaceholderText = "";

        int miScrollOffset;
        int miMaxScrollOffset { get { return (int)Math.Max(0, TextWidth - (LayoutRect.Width - Padding.Horizontal) + Screen.Style.CaretWidth); } }

        bool mbIsDragging;
        bool mbIsHovered;

        //----------------------------------------------------------------------
        public EditBox(Screen screen, string text = "", Func<char, bool> textEnteredHandler = null)
        : base(screen)
        {
            TextColor = Screen.Style.EditBoxTextColor;

            mstrText = text;
            mFont = screen.Style.MediumFont;
            mPadding = screen.Style.EditBoxPadding;
            TextEnteredHandler = textEnteredHandler;

            Frame = Screen.Style.EditBoxFrame;
            FrameCornerSize = Screen.Style.EditBoxCornerSize;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public void SelectAll()
        {
            CaretOffset = 0;
            SelectionOffset = Text.Length;
            mbIsDragging = false;
        }

        // (can be used as a FocusHandler)
        public static void SelectAll(EditBox editBox)
        {
            editBox.SelectAll();
        }

        public void ClearSelection()
        {
            SelectionOffset = 0;
        }

        //----------------------------------------------------------------------
        public void DeleteSelectedText()
        {
            if (SelectionOffset > 0)
            {
                if (TextRemovedHandler != null) TextRemovedHandler(this, CaretOffset, SelectionOffset);
                Text = Text.Remove(CaretOffset, SelectionOffset);
                SelectionOffset = 0;
            }
            else
            if (SelectionOffset < 0)
            {
                int iNewCaretOffset = CaretOffset + SelectionOffset;

                if (TextRemovedHandler != null) TextRemovedHandler(this, CaretOffset + SelectionOffset, -SelectionOffset);
                Text = Text.Remove(CaretOffset + SelectionOffset, -SelectionOffset);
                CaretOffset = iNewCaretOffset;
            }
        }

        public void CopySelectionToClipboard()
        {
            if (SelectionOffset != 0)
            {
                string strText;
                if (SelectionOffset > 0)
                {
                    strText = Text.Substring(CaretOffset, SelectionOffset);
                }
                else
                {
                    strText = Text.Substring(CaretOffset + SelectionOffset, -SelectionOffset);
                }

#if !FNA
                // NOTE: For this to work, you must put [STAThread] before your Main()
                try
                {
                    System.Windows.Forms.Clipboard.SetText(strText);
                }
                catch { }
#else
                SDL2.SDL.SDL_SetClipboardText( strText );
#endif
            }
        }

        public void PasteFromClipboard()
        {
#if !FNA
            // NOTE: For this to work, you must put [STAThread] before your Main()
            string strPastedText = null;
            try
            {
                strPastedText = System.Windows.Forms.Clipboard.GetText();
            }
            catch { }
#else
            string strPastedText = SDL2.SDL.SDL_GetClipboardText();
#endif
            if (strPastedText != null)
            {
                strPastedText = strPastedText.Replace("\r\n", " ").Replace("\n", " ");

                DeleteSelectedText();

                if (MaxLength != 0 && strPastedText.Length > MaxLength - Text.Length)
                {
                    strPastedText = strPastedText.Substring(0, MaxLength - Text.Length);
                }

                if (TextInsertedHandler != null) TextInsertedHandler(this, CaretOffset, strPastedText);
                Text = Text.Insert(CaretOffset, strPastedText);
                CaretOffset += strPastedText.Length;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            mstrDisplayedText = (mPasswordChar == '\0') ? Text : "".PadLeft(Text.Length, mPasswordChar);

            TextWidth = mstrDisplayedText != "" ? (int)Font.MeasureString(mstrDisplayedText).X : (int)Font.MeasureString(PlaceholderText).X;
            ContentWidth = 0; //(int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)(Font.LineSpacing * 0.9f) + Padding.Top + Padding.Bottom;

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
            Screen.Game.SetCursor(MouseCursor.IBeam);
            base.OnMouseEnter(hitPoint);
            mbIsHovered = true;
        }

        public override void OnMouseMove(Point hitPoint)
        {
            if (mbIsDragging)
            {
                int iOffset = GetCaretOffsetAtX(Math.Max(0, hitPoint.X - (LayoutRect.X + Padding.Left)));
                SelectionOffset = iOffset - miCaretOffset;
            }
        }

        public override void OnMouseOut(Point hitPoint)
        {
            Screen.Game.SetCursor(MouseCursor.Default);
            base.OnMouseOut(hitPoint);
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            mbIsDragging = true;

            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftShift, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightShift, true);
            if (HasFocus && bShift)
            {
                int iOffset = GetCaretOffsetAtX(Math.Max(0, hitPoint.X - (LayoutRect.X + Padding.Left)));
                SelectionOffset = iOffset - miCaretOffset;
            }
            else
            {
                CaretOffset = GetCaretOffsetAtX(Math.Max(0, hitPoint.X - (LayoutRect.X + Padding.Left)));
            }

            Screen.Focus(this);

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            if (mbIsDragging)
            {
                int iOffset = GetCaretOffsetAtX(Math.Max(0, hitPoint.X - (LayoutRect.X + Padding.Left)));
                SelectionOffset = iOffset - miCaretOffset;
                mbIsDragging = false;
            }
        }

        int GetCaretOffsetAtX(int x)
        {
            // FIXME: This does many calls to Font.MeasureString
            // We should do a binary search instead!

            x += miScrollOffset;

            int iIndex = 0;

            float fPreviousX = 0f;

            while (iIndex < mstrDisplayedText.Length)
            {
                iIndex++;

                float fX = Font.MeasureString(mstrDisplayedText.Substring(0, iIndex)).X;
                if (fX > x)
                {
                    bool bAfter = (fX - x) < ((fX - fPreviousX) / 2f);
                    return bAfter ? iIndex : (iIndex - 1);
                }

                fPreviousX = fX;
            }

            return iIndex;
        }

        //----------------------------------------------------------------------
        protected internal override void OnTextEntered(char @char)
        {
            if (!IsReadOnly && (MaxLength == 0 || Text.Length < MaxLength || SelectionOffset != 0) && !char.IsControl(@char) && (TextEnteredHandler == null || TextEnteredHandler(@char)))
            {
                if (SelectionOffset != 0)
                {
                    DeleteSelectedText();
                }

                string strAddedText = @char.ToString();
                if (TextInsertedHandler != null) TextInsertedHandler(this, CaretOffset, strAddedText);

                Text = Text.Insert(CaretOffset, strAddedText);

                CaretOffset++;
            }
        }

        protected internal override bool OnActivateDown()
        {
            // Ignore if Space was pressed
            return !Screen.Game.InputMgr.JustPressedOSKeys.Contains(OSKey.Space);
        }

        protected internal override void OnActivateUp()
        {
            if (!IsReadOnly && ValidateHandler != null) ValidateHandler(this);
        }

        protected internal override void OnOSKeyPress(OSKey key)
        {
            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftControl, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightControl, true);
            bool bShortcutKey = Screen.Game.InputMgr.IsShortcutKeyDown();
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftShift, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightShift, true);

            switch (key)
            {
                case OSKey.A:
                    if (bShortcutKey)
                    {
                        SelectAll();
                    }
                    break;
                case OSKey.X:
                    if (bShortcutKey && mPasswordChar == '\0')
                    {
                        CopySelectionToClipboard();
                        DeleteSelectedText();
                    }
                    break;
                case OSKey.C:
                    if (bShortcutKey && mPasswordChar == '\0')
                    {
                        CopySelectionToClipboard();
                    }
                    break;
                case OSKey.V:
                    if (bShortcutKey)
                    {
                        PasteFromClipboard();
                    }
                    break;
                case OSKey.Back:
                    if (!IsReadOnly && Text.Length > 0)
                    {
                        if (SelectionOffset != 0)
                        {
                            DeleteSelectedText();
                        }
                        else
                        if (CaretOffset > 0)
                        {
                            CaretOffset--;

                            if (TextRemovedHandler != null) TextRemovedHandler(this, CaretOffset, 1);
                            Text = Text.Remove(CaretOffset, 1);
                        }
                    }
                    break;
                case OSKey.Delete:
                    if (!IsReadOnly && Text.Length > 0)
                    {
                        if (SelectionOffset != 0)
                        {
                            DeleteSelectedText();
                        }
                        else
                        if (CaretOffset < Text.Length)
                        {
                            if (TextRemovedHandler != null) TextRemovedHandler(this, CaretOffset, 1);
                            Text = Text.Remove(CaretOffset, 1);
                        }
                    }
                    break;
                case OSKey.Escape:
                    if (!IsReadOnly && CanBeEscapeCleared && Text.Length > 0)
                    {
                        SelectAll();
                        DeleteSelectedText();
                    }
                    break;
                case OSKey.Left:
                    if (bShift)
                    {
                        if (bCtrl)
                        {
                            int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if (iNewSelectionTarget > 0)
                            {
                                iNewSelectionTarget = Text.LastIndexOf(' ', Math.Max(iNewSelectionTarget - 2, 0)) + 1;
                            }

                            SelectionOffset = iNewSelectionTarget - CaretOffset;
                        }
                        else
                        {
                            SelectionOffset--;
                        }
                    }
                    else
                    {
                        int iNewCaretOffset = CaretOffset - 1;

                        if (bCtrl)
                        {
                            SelectionOffset = 0;

                            if (iNewCaretOffset > 0)
                            {
                                iNewCaretOffset = Text.LastIndexOf(' ', iNewCaretOffset - 1) + 1;
                            }
                        }
                        else
                        if (SelectionOffset != 0)
                        {
                            iNewCaretOffset = (SelectionOffset > 0) ? CaretOffset : CaretOffset + SelectionOffset;
                            SelectionOffset = 0;
                        }
                        CaretOffset = iNewCaretOffset;
                    }
                    break;
                case OSKey.Right:
                    if (bShift)
                    {
                        if (bCtrl)
                        {
                            int iNewSelectionTarget = CaretOffset + SelectionOffset;

                            if (iNewSelectionTarget < Text.Length)
                            {
                                iNewSelectionTarget = Text.IndexOf(' ', iNewSelectionTarget, Text.Length - iNewSelectionTarget) + 1;

                                if (iNewSelectionTarget == 0)
                                {
                                    iNewSelectionTarget = Text.Length;
                                }

                                SelectionOffset = iNewSelectionTarget - CaretOffset;
                            }
                        }
                        else
                        {
                            SelectionOffset++;
                        }
                    }
                    else
                    {
                        int iNewCaretOffset = CaretOffset + 1;

                        if (bCtrl)
                        {
                            if (iNewCaretOffset < Text.Length)
                            {
                                iNewCaretOffset = Text.IndexOf(' ', iNewCaretOffset, Text.Length - iNewCaretOffset) + 1;

                                if (iNewCaretOffset == 0)
                                {
                                    iNewCaretOffset = Text.Length;
                                }
                            }
                        }
                        else
                        if (SelectionOffset != 0)
                        {
                            iNewCaretOffset = (SelectionOffset < 0) ? CaretOffset : CaretOffset + SelectionOffset;
                        }
                        CaretOffset = iNewCaretOffset;
                    }
                    break;
                case OSKey.End:
                    if (bShift)
                    {
                        SelectionOffset = Text.Length - CaretOffset;
                    }
                    else
                    {
                        CaretOffset = Text.Length;
                    }
                    break;
                case OSKey.Home:
                    if (bShift)
                    {
                        SelectionOffset = -CaretOffset;
                    }
                    else
                    {
                        CaretOffset = 0;
                    }
                    break;
                case OSKey.Tab:
                    if (LookupHandler != null)
                    {
                        if (CaretOffset > 0 && (CaretOffset == Text.Length || Text[CaretOffset] == ' '))
                        {
                            int iOffset = Text.LastIndexOf(' ', CaretOffset - 1);

                            iOffset++;

                            string strLookup = Text.Substring(iOffset, CaretOffset - iOffset);
                            if (strLookup != "")
                            {
                                string strResult = LookupHandler(strLookup);
                                if (!string.IsNullOrEmpty(strResult))
                                {
                                    Text = Text.Substring(0, iOffset) + strResult + Text.Substring(CaretOffset);
                                    CaretOffset = iOffset + strResult.Length;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        base.OnOSKeyPress(key);
                    }
                    break;
                default:
                    base.OnOSKeyPress(key);
                    break;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove(Direction direction)
        {
            if (direction == Direction.Left || direction == Direction.Right)
            {
                // Horizontal pad move are eaten since left and right are used to move the caret
                return;
            }

            base.OnPadMove(direction);
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

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            if (!HasFocus)
            {
                mfCaretTimer = 0f;
            }
            else
            {
                mfCaretTimer += elapsedTime;
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            DrawWithOffset(Point.Zero);
        }

        //----------------------------------------------------------------------
        public void DrawWithOffset(Point offset)
        {
            var rect = LayoutRect;
            rect.Offset(offset);

            if (Frame != null)
            {
                Screen.DrawBox(Frame, rect, FrameCornerSize, Color.White);
            }

            if (Screen.IsActive && mbIsHovered)
            {
                Screen.DrawBox(Screen.Style.EditBoxHoverOverlay, rect, Screen.Style.EditBoxCornerSize, Color.White);
            }

            Screen.PushScissorRectangle(new Rectangle(rect.X + Padding.Left, rect.Y, rect.Width - Padding.Horizontal, rect.Height));

            if (mstrDisplayedText != "")
            {
                Screen.Game.SpriteBatch.DrawString(mFont, mstrDisplayedText, new Vector2(mpTextPosition.X - miScrollOffset + offset.X, mpTextPosition.Y + mFont.YOffset + offset.Y), TextColor);
            }
            else
            {
                Screen.Game.SpriteBatch.DrawString(mFont, PlaceholderText, new Vector2(mpTextPosition.X - miScrollOffset + offset.X, mpTextPosition.Y + mFont.YOffset + offset.Y), TextColor * 0.5f);
            }

            Screen.PopScissorRectangle();

            const float fBlinkInterval = 0.3f;

            if (SelectionOffset != 0)
            {
                Screen.PushScissorRectangle(new Rectangle(rect.X + Padding.Left, rect.Y + Padding.Top, rect.Width - Padding.Horizontal, rect.Height - Padding.Vertical));

                Rectangle selectionRectangle;
                if (SelectionOffset > 0)
                {
                    selectionRectangle = new Rectangle(mpTextPosition.X + miCaretX - miScrollOffset, rect.Y + Padding.Top, miSelectionX - miCaretX, rect.Height - Padding.Vertical);
                }
                else
                {
                    selectionRectangle = new Rectangle(mpTextPosition.X + miSelectionX - miScrollOffset, rect.Y + Padding.Top, miCaretX - miSelectionX, rect.Height - Padding.Vertical);
                }

                Screen.Game.SpriteBatch.Draw(Screen.Game.WhitePixelTex, selectionRectangle, TextColor * 0.3f);

                Screen.PopScissorRectangle();
            }
            else if (Screen.IsActive && HasFocus && mfCaretTimer % (fBlinkInterval * 2) < fBlinkInterval)
            {
                Screen.Game.SpriteBatch.Draw(Screen.Game.WhitePixelTex, new Rectangle(mpTextPosition.X + miCaretX - miScrollOffset, rect.Y + Padding.Top, Screen.Style.CaretWidth, rect.Height - Padding.Vertical), TextColor);
            }
        }
    }
}
