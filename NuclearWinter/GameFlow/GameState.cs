using System;
using System.Collections.Generic;
using System.Text;

namespace NuclearWinter.GameFlow
{
    //--------------------------------------------------------------------------
    /// Each GameState handles part of the game : menu, in-game, settings, etc.
    public abstract class GameState
    {
        //----------------------------------------------------------------------
        public GameState( NuclearGame _game )
        {
            Game = _game;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Starts the GameState, called when it becomes the current one.
        /// </summary>
        public abstract void Start();

        //----------------------------------------------------------------------
        /// <summary>
        /// Stops the GameState, called when switching to another one.
        /// </summary>
        public abstract void Stop();

        //----------------------------------------------------------------------
        public virtual void OnActivated() {}

        //----------------------------------------------------------------------
        /// <summary>
        /// Called repeatedly when starting the GameState, until it returns true
        /// </summary>
        /// <returns>false while fading in, true when done</returns>
        public virtual bool UpdateFadeIn( Microsoft.Xna.Framework.GameTime _time )
        {
            Update( _time );
            return true;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Draws while fading in.
        /// </summary>
        /// <param name="_time"></param>
        public virtual void DrawFadeIn( Microsoft.Xna.Framework.GameTime _time )
        {
            Draw( _time );
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Called repeatedly when stopping the GameState, until it returns true
        /// </summary>
        /// <returns>false while fading out, true when done</returns>
        public virtual bool UpdateFadeOut( Microsoft.Xna.Framework.GameTime _time )
        {
            Update( _time );

            return true;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Draws while fading out.
        /// </summary>
        /// <param name="_time"></param>
        public virtual void DrawFadeOut( Microsoft.Xna.Framework.GameTime _time )
        {
            Draw( _time );
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Updates the GameState.
        /// </summary>
        /// <param name="_elapsedTime"></param>
        public abstract void Update( Microsoft.Xna.Framework.GameTime _time );
        
        //----------------------------------------------------------------------
        /// <summary>
        /// Draws the GameState.
        /// </summary>
        /// <param name="_time"></param>
        public abstract void Draw( Microsoft.Xna.Framework.GameTime _time );

        //----------------------------------------------------------------------
        public readonly NuclearGame         Game;
    }
}
