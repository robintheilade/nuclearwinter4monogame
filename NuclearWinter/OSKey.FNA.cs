#if FNA
using System;

namespace NuclearWinter
{
    public enum OSKey
    {
        A           = SDL2.SDL.SDL_Keycode.SDLK_a,
        X           = SDL2.SDL.SDL_Keycode.SDLK_x,
        C           = SDL2.SDL.SDL_Keycode.SDLK_c,
        V           = SDL2.SDL.SDL_Keycode.SDLK_v,

        Escape      = SDL2.SDL.SDL_Keycode.SDLK_ESCAPE,
        Tab         = SDL2.SDL.SDL_Keycode.SDLK_TAB,
        Enter       = SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER,
        Return      = SDL2.SDL.SDL_Keycode.SDLK_RETURN,
        Space       = SDL2.SDL.SDL_Keycode.SDLK_SPACE,
        Back        = SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE,
        Delete      = SDL2.SDL.SDL_Keycode.SDLK_DELETE,
        Home        = SDL2.SDL.SDL_Keycode.SDLK_HOME,
        End         = SDL2.SDL.SDL_Keycode.SDLK_END,
        PageUp      = SDL2.SDL.SDL_Keycode.SDLK_PAGEUP,
        PageDown    = SDL2.SDL.SDL_Keycode.SDLK_PAGEDOWN,

        Up          = SDL2.SDL.SDL_Keycode.SDLK_UP,
        Right       = SDL2.SDL.SDL_Keycode.SDLK_RIGHT,
        Down        = SDL2.SDL.SDL_Keycode.SDLK_DOWN,
        Left        = SDL2.SDL.SDL_Keycode.SDLK_LEFT,
    }
}
#endif
