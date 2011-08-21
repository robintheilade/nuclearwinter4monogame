using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#endif

namespace NuclearWinter
{
    public class NuclearGame: Game
    {
        //----------------------------------------------------------------------
        public Texture2D        WhitePixelTex { get; protected set; }

        //----------------------------------------------------------------------
        public NuclearGame()
        {
#if WINDOWS_PHONE
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>( OnDeactivated );
            PhoneApplicationService.Current.Closing     += new EventHandler<ClosingEventArgs>( OnClosing );
#endif

#if WINDOWS || XBOX
            StorageDevice.DeviceChanged                 += new EventHandler<EventArgs>( OnStorageDeviceChange );
#endif


            Graphics = new GraphicsDeviceManager(this);
        }

        //----------------------------------------------------------------------
        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch( GraphicsDevice );

            GameStateMgr = new GameFlow.GameStateMgr<NuclearGame>( this );
            Components.Add( GameStateMgr );

#if WINDOWS || XBOX
            InputMgr                = new Input.InputManager( this );
            Components.Add( InputMgr );
#endif

#if WINDOWS_PHONE
            TouchMgr                = new Input.TouchManager( this );
            Components.Add( TouchMgr );
#endif

            base.Initialize();
        }

#if WINDOWS || XBOX
        protected override void Update( GameTime _time )
        {
#if GAMERSERVICES || XBOX
            if( ! Guide.IsVisible )
            {
#endif
                if( mbShouldDisplayStorageSelector && PlayerInCharge.HasValue )
                {
                    StorageDevice.BeginShowSelector( PlayerInCharge.Value, new AsyncCallback( StorageDeviceCallback ), null );
                    mbShouldDisplayStorageSelector= false;
                }
#if GAMERSERVICES || XBOX
            }
#endif

            base.Update( _time );
        }
#endif

#if WINDOWS_PHONE
        //----------------------------------------------------------------------
        protected virtual void OnDeactivated( object _sender, DeactivatedEventArgs _args )
        {
            if( NuclearSaveHandler != null )
            {
                NuclearSaveHandler.Save();
            }

            if( MediaPlayer.GameHasControl )
            {
                MediaPlayer.Stop();
            }

            // FIXME: We might want to handle tombstoning through PhoneApplicationService.Current.State
        }

        //----------------------------------------------------------------------
        void OnClosing( object _sender, ClosingEventArgs _args )
        {
            if( NuclearSaveHandler != null )
            {
                NuclearSaveHandler.Save();
            }

            if( MediaPlayer.GameHasControl )
            {
                MediaPlayer.Stop();
            }
        }
#endif

        //----------------------------------------------------------------------
        protected override void LoadContent()
        {
            WhitePixelTex           = Content.Load<Texture2D>( "Sprites/WhitePixel" );
        }

#if WINDOWS || XBOX
        //----------------------------------------------------------------------
        public void ShowDeviceSelector( Action _deviceSelectedCallback )
        {
            SaveGameStorageDevice = null;
            mbShouldDisplayStorageSelector = true;

            DeviceSelectedCallback = _deviceSelectedCallback;
        }

        //----------------------------------------------------------------------
        void StorageDeviceCallback( IAsyncResult _result )
        {
            SaveGameStorageDevice = StorageDevice.EndShowSelector( _result );
            
            if( SaveGameStorageDevice == null )
            {
                // The dialog was cancelled, we're not taking no as an answer!
                // Let's ask again
                mbShouldDisplayStorageSelector = true;
            }
            else
            {
                DeviceSelectedCallback();
            }
        }
        
        //----------------------------------------------------------------------
        protected void OnStorageDeviceChange( object _sender, EventArgs _args )
        {
            if( SaveGameStorageDevice != null )
            {
                mbShouldDisplayStorageSelector = ! SaveGameStorageDevice.IsConnected;
            }
        }
#endif

        //----------------------------------------------------------------------
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

            if( GameStateMgr != null && GameStateMgr.CurrentState != null )
            {
                GameStateMgr.CurrentState.OnActivated();
            }
        }

        //----------------------------------------------------------------------
        protected override void OnExiting(object sender, EventArgs args)
        {
            if( NuclearSaveHandler != null )
            {
                NuclearSaveHandler.SaveGameSettings();
                NuclearSaveHandler.SaveGameData();
            }

            base.OnExiting(sender, args);
        }

        //----------------------------------------------------------------------
        public virtual string GetUIString( string _strId ) { return _strId; }

        //----------------------------------------------------------------------
        public List<string> WrapText( SpriteFont _font, string _strText, float _fLineWidth )
        {
            List<string> lText = new List<string>();

            string strLine = string.Empty;
            string returnString = String.Empty;
            string[] aWords = _strText.Split(' ');

            foreach( string strWord in aWords )
            {
                if( _font.MeasureString(strLine + strWord).Length() > _fLineWidth && strLine != "" )
                {
                    lText.Add( strLine );
                    strLine = string.Empty;
                }

                strLine += strWord + ' ';
            }

            lText.Add( strLine );

            return lText;
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, string _strLabel, Vector2 _vPosition )
        {
            DrawBlurredText( _fBlurRadius, _font, _strLabel, _vPosition, Color.White, Color.Black, Vector2.Zero, 1f  );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color )
        {
            DrawBlurredText( _fBlurRadius, _font, _strLabel, _vPosition, _color, Color.Black, Vector2.Zero, 1f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Vector2 _vOrigin, float _fScale )
        {
            DrawBlurredText( _fBlurRadius, _font, _strLabel, _vPosition, _color, Color.Black, _vOrigin, _fScale );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Color _blurColor )
        {
            DrawBlurredText( _fBlurRadius, _font, _strLabel, _vPosition, _color, _blurColor, Vector2.Zero, 1f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Color _blurColor, Vector2 _vOrigin, float _fScale )
        {
            if( _fBlurRadius > 0f )
            {
                Color blurColor = _blurColor * 0.1f * (_color.A / 255f);

                SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X - _fBlurRadius, _vPosition.Y - _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X + _fBlurRadius, _vPosition.Y - _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X + _fBlurRadius, _vPosition.Y + _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X - _fBlurRadius, _vPosition.Y + _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            }

            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X, _vPosition.Y ), _color, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition )
        {
            DrawBlurredText( _fBlurRadius, _font, _strbLabel, _vPosition, Color.White, Color.Black, Vector2.Zero, 1f  );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color )
        {
            DrawBlurredText( _fBlurRadius, _font, _strbLabel, _vPosition, _color, Color.Black, Vector2.Zero, 1f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Vector2 _vOrigin, float _fScale )
        {
            DrawBlurredText( _fBlurRadius, _font, _strbLabel, _vPosition, _color, Color.Black, _vOrigin, _fScale );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Color _blurColor )
        {
            DrawBlurredText( _fBlurRadius, _font, _strbLabel, _vPosition, _color, _blurColor, Vector2.Zero, 1f );
        }


        //----------------------------------------------------------------------
        public void DrawBlurredText( float _fBlurRadius, SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Color _blurColor, Vector2 _vOrigin, float _fScale )
        {
            if( _fBlurRadius > 0f )
            {
                Color blurColor = _blurColor * 0.1f * (_color.A / 255f);

                SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X - _fBlurRadius, _vPosition.Y - _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X + _fBlurRadius, _vPosition.Y - _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X + _fBlurRadius, _vPosition.Y + _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
                SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X - _fBlurRadius, _vPosition.Y + _fBlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            }

            SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X, _vPosition.Y ), _color, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        public void PlayMusic( Song _song )
        {
            if( Song != _song && MediaPlayer.GameHasControl )
            {
                MediaPlayer.IsRepeating = true;

                if( _song != null )
                {
                    MediaPlayer.Play( _song );
                }
                else
                {
                    MediaPlayer.Stop();
                }

                Song = _song;
            }
        }

        //----------------------------------------------------------------------
        public GraphicsDeviceManager                        Graphics;
        public SpriteBatch                                  SpriteBatch;
        
#if WINDOWS || XBOX
        // Index of the player responsible for menu navigation (or null if none yet)
        public PlayerIndex?                                 PlayerInCharge;
#endif

        public SaveHandler                                  NuclearSaveHandler;
#if WINDOWS || XBOX
        public StorageDevice                                SaveGameStorageDevice;
        bool                                                mbShouldDisplayStorageSelector;
        Action                                              DeviceSelectedCallback;
#endif

        //----------------------------------------------------------------------
        // Sound & Music
        public Song                                         Song;

        //----------------------------------------------------------------------
        // Game States
        public GameFlow.GameStateMgr<NuclearGame>           GameStateMgr        { get; private set; }

#if WINDOWS || XBOX
        public Input.InputManager                         InputMgr          { get; private set; }
#endif

#if WINDOWS_PHONE
        public Input.TouchManager                           TouchMgr            { get; private set; }
#endif
    }
}
