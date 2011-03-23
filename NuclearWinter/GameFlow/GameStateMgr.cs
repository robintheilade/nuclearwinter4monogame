using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

            if( CurrentState == null )
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
                if( CurrentState.UpdateFadeOut( _time ) )
                {
                    // We're done
                    CurrentState.Stop();
                    Game.Exit();
                }
            }

            //-----------------------------------------------------------------
            // Handle state switching
            else
            if( mNextState != null )
            {
                if( CurrentState != null )
                {
                    //---------------------------------------------------------
                    // Fade out current state
                    if( CurrentState.UpdateFadeOut( _time ) )
                    {
                        // We're done fading out
                        CurrentState.Stop();
                        CurrentState = null;

                        // Reset all vibrations when switching active GameState
                        GamePad.SetVibration( PlayerIndex.One,      0f, 0f );
                        GamePad.SetVibration( PlayerIndex.Two,      0f, 0f );
                        GamePad.SetVibration( PlayerIndex.Three,    0f, 0f );
                        GamePad.SetVibration( PlayerIndex.Four,     0f, 0f );

                        mNextState.Start();
                    }
                }
                
                if( CurrentState == null )
                {
                    //---------------------------------------------------------
                    // Fade in next state
                    if( mNextState.UpdateFadeIn( _time ) )
                    {
                        // We're done fading in
                        CurrentState = mNextState;
                        mNextState = null;
                        
                        CurrentState.Update( _time );
                    }
                }
            }

            //-----------------------------------------------------------------
            // Update current GameState
            else
            {
                CurrentState.Update( _time );
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
                CurrentState.DrawFadeOut( _time );
            }
            else
            if( mNextState != null )
            {
                if( CurrentState != null )
                {
                    CurrentState.DrawFadeOut( _time );
                }
                else
                {
                    mNextState.DrawFadeIn( _time );
                }
            }
            else
            {
                CurrentState.Draw( _time );
            }
        }
        
        //---------------------------------------------------------------------
        public bool IsSwitching {
            get { return null != mNextState; }
        }

        //---------------------------------------------------------------------
        public GameState                        CurrentState { get; private set; }  // Current game state
        private GameState                       mNextState;                         // Next game state to be started, stored while the current state is fading out
        private bool                            mbExit;                             // Is the game exiting?
    }
}
