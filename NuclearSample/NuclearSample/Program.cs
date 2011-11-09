using System;

namespace NuclearSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (NuclearSampleGame game = new NuclearSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

