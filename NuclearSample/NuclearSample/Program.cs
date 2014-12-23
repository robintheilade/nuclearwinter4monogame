using System;

namespace NuclearSample
{
    static class Program
    {
        static void Main( string[] args )
        {
            // This sample uses an ApplicationMutex to prevent running the game multiple times at once.
            // Useful for games where progress or settings might be overwritten if two instances are running at the same time.
            using( var mutex = new NuclearWinter.ApplicationMutex() )
            {
                if( mutex.HasHandle )
                {
                    using( var game = new NuclearSampleGame() )
                    {
                        game.Run();
                    }
                }
                else
                {
#if !FNA
                    System.Windows.Forms.MessageBox.Show( "An instance of NuclearSample is already running.", "Could not start NuclearSample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop );
#else
                    SDL2.SDL.SDL_ShowSimpleMessageBox( SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Could not start NuclearSample", "An instance of NuclearSample is already running.", IntPtr.Zero );
#endif
                }
            }
        }
    }
}

