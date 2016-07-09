using System;
using System.Windows.Forms;
using OSKey = System.Windows.Forms.Keys;
using XNAKey = Microsoft.Xna.Framework.Input.Keys;

namespace NuclearWinter
{
    public class KeyPressEventArgs : EventArgs
    {
        private LocalizedKeyboardState keyboardState;

        public OSKey Key
        {
            get;
            private set;
        }

        public bool IsLeftControl
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.LeftControl);
            }
        }

        public bool IsRightControl
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.RightControl);
            }
        }

        public bool IsLeftShift
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.LeftShift);
            }
        }

        public bool IsRightShift
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.RightShift);
            }
        }

        public bool IsLeftAlt
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.LeftAlt);
            }
        }

        public bool IsRightAlt
        {
            get
            {
                return this.keyboardState.IsKeyDown(XNAKey.RightAlt);
            }
        }

        public bool Handled
        {
            get;
            set;
        }

        public KeyPressEventArgs(OSKey key, LocalizedKeyboardState keyboardState)
        {
            this.Key = key;
            this.keyboardState = keyboardState;
        }
    }
}
