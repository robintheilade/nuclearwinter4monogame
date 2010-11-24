using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.GameFlow
{

    /// <summary>
    /// This singleton class takes care of switching between game phases.
    /// </summary>
    public class GameStateMgr: DrawableGameComponent
    {
        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public GameStateMgr(Game game)
            : base(game)
        {
            
        }

        //---------------------------------------------------------------------
        public override void Initialize()
        {
            base.Initialize();


        }

        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            ContentManager content = Game.Content;
        }

        //---------------------------------------------------------------------
        public void Exit()
        {
            mbExit = true;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Switches current GameState.
        /// </summary>
        /// <param name="_newGameState"></param>
        public void SwitchState( GameState _newState )
        {
            //------------------------------
            // precondition
            System.Diagnostics.Debug.Assert( mNextState == null );
            //------------------------------

            mNextState = _newState;

            if( mCurrentState == null )
            {
                mNextState.Start();
            }

        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Updates the current GameState.
        /// </summary>
        /// <param name="_time">Provides a snapshot of timing values.</param>
        public override void Update( Microsoft.Xna.Framework.GameTime _time )
        {
            if( mbExit )
            {
                //---------------------------------------------------------
                // Fade out current state
                if( mCurrentState.UpdateFadeOut( _time ) )
                {
                    // We're done
                    mCurrentState.Stop();
                    Game.Exit();
                }
            }

            //-----------------------------------------------------------------
            // Handle state switching
            else
            if( mNextState != null )
            {
                if( mCurrentState != null )
                {
                    //---------------------------------------------------------
                    // Fade out current state
                    if( mCurrentState.UpdateFadeOut( _time ) )
                    {
                        // We're done fading out
                        mCurrentState.Stop();
                        mCurrentState = null;
                        mNextState.Start();
                    }
                }
                
                if( mCurrentState == null )
                {
                    //---------------------------------------------------------
                    // Fade in next state
                    if( mNextState.UpdateFadeIn( _time ) )
                    {
                        // We're done fading in
                        mCurrentState = mNextState;
                        mNextState = null;
                        
                        mCurrentState.Update( _time );
                    }
                }
            }

            //-----------------------------------------------------------------
            // Update current GameState
            else
            {
                mCurrentState.Update( _time );
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Draws the current GameState.
        /// </summary>
        /// <param name="_time"></param>
        public override void Draw( Microsoft.Xna.Framework.GameTime _time )
        {
            if( mbExit )
            {
                mCurrentState.DrawFadeOut( _time );
            }
            else
            if( mNextState != null )
            {
                if( mCurrentState != null )
                {
                    mCurrentState.DrawFadeOut( _time );
                }
                else
                {
                    mNextState.DrawFadeIn( _time );
                }
            }
            else
            {
                mCurrentState.Draw( _time );
            }
        }
        
        //---------------------------------------------------------------------
        public bool IsSwitching {
            get { return null != mNextState; }
        }

        //---------------------------------------------------------------------
        private GameState                       mCurrentState;                      // Current game state
        private GameState                       mNextState;                         // Next game state to be started, stored while the current state is fading out
        private bool                            mbExit;                             // Is the game exiting?
    }
}
