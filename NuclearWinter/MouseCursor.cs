using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter
{
        public enum MouseCursor {
#if ! FNA
            Default,
            SizeWE,
            SizeNS,
            SizeAll,

            Hand,
            IBeam,
            Cross
#else
            Default = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            SizeWE = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
            SizeNS = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
            SizeAll = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,

            Hand = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
            IBeam = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
            Cross = SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR
#endif
        }
}
