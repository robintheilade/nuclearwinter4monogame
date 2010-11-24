using System;
using System.Collections.Generic;
using System.Text;

namespace NuclearWinter.GameFlow
{
    /// <summary>
    /// Each GameState handles part of the game : menu, in-game, settings, etc.
    /// </summary>
    public abstract class GameState
    {
        /// <summary>
        /// Starts the GameState, called when it becomes the current one.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the GameState, called when switching to another one.
        /// </summary>
        public abstract void Stop();


        /// <summary>
        /// Called repeatedly when starting the GameState, until it returns true
        /// </summary>
        /// <returns>false while fading in, true when done</returns>
        public virtual bool UpdateFadeIn( Microsoft.Xna.Framework.GameTime _time )
        {
            return true;
        }

        /// <summary>
        /// Draws while fading in.
        /// </summary>
        /// <param name="_time"></param>
        public virtual void DrawFadeIn( Microsoft.Xna.Framework.GameTime _time )
        {

        }


        /// <summary>
        /// Called repeatedly when stopping the GameState, until it returns true
        /// </summary>
        /// <returns>false while fading out, true when done</returns>
        public virtual bool UpdateFadeOut( Microsoft.Xna.Framework.GameTime _time )
        {
            return true;
        }

        /// <summary>
        /// Draws while fading out.
        /// </summary>
        /// <param name="_time"></param>
        public virtual void DrawFadeOut( Microsoft.Xna.Framework.GameTime _time )
        {

        }


        /// <summary>
        /// Updates the GameState.
        /// </summary>
        /// <param name="_elapsedTime"></param>
        public abstract void Update( Microsoft.Xna.Framework.GameTime _time );
        
        /// <summary>
        /// Draws the GameState.
        /// </summary>
        /// <param name="_time"></param>
        public abstract void Draw( Microsoft.Xna.Framework.GameTime _time );
    }
}
