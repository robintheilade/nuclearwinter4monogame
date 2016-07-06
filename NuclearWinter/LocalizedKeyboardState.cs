﻿// Based on http://www.luminance.org/gruedorf/2009/08/14/supporting-alternate-keyboard-layouts-in-xna-games

using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace NuclearWinter
{
    public struct LocalizedKeyboardState
    {

        internal enum MAPVK : uint
        {
            VK_TO_VSC = 0,
            VSC_TO_VK = 1,
            VK_TO_CHAR = 2
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static uint MapVirtualKeyEx(uint key, MAPVK mappingType, IntPtr keyboardLayout);
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static IntPtr LoadKeyboardLayout(string keyboardLayoutID, uint flags);
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static bool UnloadKeyboardLayout(IntPtr handle);
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static IntPtr GetKeyboardLayout(IntPtr threadId);

        internal const uint KLF_NOTELLSHELL = 0x00000080;

        public struct KeyboardLayout : IDisposable
        {
            public readonly IntPtr Handle;

            public KeyboardLayout(IntPtr handle) : this()
            {
                Handle = handle;
            }

            public KeyboardLayout(string keyboardLayoutID)
                : this(LoadKeyboardLayout(keyboardLayoutID, KLF_NOTELLSHELL))
            {
            }

            public bool IsDisposed
            {
                get;
                private set;
            }

            public void Dispose()
            {
                if (IsDisposed)
                    return;

                UnloadKeyboardLayout(Handle);
                IsDisposed = true;
            }

            public static KeyboardLayout US_English = new KeyboardLayout("00000409");

            public static KeyboardLayout Active
            {
                get
                {
                    return new KeyboardLayout(GetKeyboardLayout(IntPtr.Zero));
                }
            }
        }

        public readonly KeyboardState Native;

#if FNA
        static bool isWindows = SDL2.SDL.SDL_GetPlatform() == "Windows";
#endif

        public LocalizedKeyboardState(KeyboardState keyboardState)
        {
            Native = keyboardState;
        }

        public bool IsKeyDown(Keys key, bool isLocalKey)
        {
            if (!isLocalKey) key = USEnglishToLocal(key);
            return Native.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key, bool isLocalKey)
        {
            if (!isLocalKey)
                key = USEnglishToLocal(key);

            return Native.IsKeyUp(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return IsKeyDown(key, false);
        }

        public bool IsKeyUp(Keys key)
        {
            return IsKeyUp(key, false);
        }

        // Maps a localized character like 'S' to the virtual scan code
        //  for that key on the user's keyboard ('O' in dvorak, for example)
        public static Keys USEnglishToLocal(Keys key)
        {
            if (key > Keys.Z && (key < Keys.OemSemicolon || (key > Keys.OemTilde && (key < Keys.OemOpenBrackets || key > Keys.OemBackslash)))) return key;

#if FNA
            if( isWindows )
#endif
            {
                return Windows_USEnglishToLocal(key);
            }

#if FNA
            return _key;
#endif
        }

        public static Keys Windows_USEnglishToLocal(Keys key)
        {
            var activeScanCode = MapVirtualKeyEx((uint)key, MAPVK.VK_TO_VSC, KeyboardLayout.US_English.Handle);
            var nativeVirtualCode = MapVirtualKeyEx(activeScanCode, MAPVK.VSC_TO_VK, KeyboardLayout.Active.Handle);

            return (Keys)nativeVirtualCode;
        }

        public static Keys LocalToUSEnglish(Keys key)
        {
            if (key > Keys.Z) return key;

#if FNA
            if( isWindows )
#endif
            {
                return Windows_LocalToUSEnglish(key);
            }

#if FNA
            return _key;
#endif
        }

        public static Keys Windows_LocalToUSEnglish(Keys key)
        {
            var activeScanCode = MapVirtualKeyEx((uint)key, MAPVK.VK_TO_VSC, KeyboardLayout.Active.Handle);
            var nativeVirtualCode = MapVirtualKeyEx(activeScanCode, MAPVK.VSC_TO_VK, KeyboardLayout.US_English.Handle);

            return (Keys)nativeVirtualCode;
        }
    }
}
