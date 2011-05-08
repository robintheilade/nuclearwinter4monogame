using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#endif

namespace NuclearWinter
{
    public enum TargetPlatform
    {
        Windows,
        Xbox360,
        WindowsPhone
    }

    public class NuclearGame: Game
    {
        //----------------------------------------------------------------------
        public NuclearGame( TargetPlatform _platform )
        {
#if WINDOWS_PHONE
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(OnDeactivated);
            PhoneApplicationService.Current.Closing     += new EventHandler<ClosingEventArgs>(OnClosing);
#endif

            Graphics = new GraphicsDeviceManager(this);

            switch( _platform )
            {
                case TargetPlatform.WindowsPhone:
                    BlurRadius = 2;
                    break;
                case TargetPlatform.Xbox360:
                case TargetPlatform.Windows:
                    BlurRadius = 4;
                    break;
            }
        }

        //----------------------------------------------------------------------
        protected override void Initialize()
        {
            if( SaveGame != null )
            {
                SaveGame.Load();
            }

            SpriteBatch = new SpriteBatch( GraphicsDevice );

            GameStateMgr = new GameFlow.GameStateMgr( this );
            Components.Add( GameStateMgr );

#if WINDOWS || XBOX
            GamePadMgr                = new Input.GamePadManager( this );
            Components.Add( GamePadMgr );
#endif

#if WINDOWS_PHONE
            TouchMgr                = new Input.TouchManager( this );
            Components.Add( TouchMgr );
#endif

            base.Initialize();
        }

#if WINDOWS_PHONE
        //----------------------------------------------------------------------
        protected void OnDeactivated( object _sender, DeactivatedEventArgs _args )
        {
            if( SaveGame != null )
            {
                SaveGame.Save();
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
            if( SaveGame != null )
            {
                SaveGame.Save();
            }

            if( MediaPlayer.GameHasControl )
            {
                MediaPlayer.Stop();
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
            if( SaveGame != null )
            {
                SaveGame.Save();
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
                if( _font.MeasureString(strLine + strWord).Length() > _fLineWidth )
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
        public void DrawBlurredText( SpriteFont _font, string _strLabel, Vector2 _vPosition )
        {
            DrawBlurredText( _font, _strLabel, _vPosition, Color.White, Color.Black, Vector2.Zero, 1f  );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color )
        {
            DrawBlurredText( _font, _strLabel, _vPosition, _color, Color.Black, Vector2.Zero, 1f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Vector2 _vOrigin, float _fScale )
        {
            DrawBlurredText( _font, _strLabel, _vPosition, _color, Color.Black, _vOrigin, _fScale );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Color _blurColor )
        {
            DrawBlurredText( _font, _strLabel, _vPosition, _color, _blurColor, Vector2.Zero, 1f );
        }

        int BlurRadius = 4;

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, string _strLabel, Vector2 _vPosition, Color _color, Color _blurColor, Vector2 _vOrigin, float _fScale )
        {
            Color blurColor = _blurColor * 0.1f * (_color.A / 255f);

            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X - BlurRadius, _vPosition.Y - BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X + BlurRadius, _vPosition.Y - BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X + BlurRadius, _vPosition.Y + BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X - BlurRadius, _vPosition.Y + BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );

            SpriteBatch.DrawString( _font, _strLabel, new Vector2( _vPosition.X, _vPosition.Y ), _color, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition )
        {
            DrawBlurredText( _font, _strbLabel, _vPosition, Color.White, Color.Black, Vector2.Zero, 1f  );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color )
        {
            DrawBlurredText( _font, _strbLabel, _vPosition, _color, Color.Black, Vector2.Zero, 1f );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Vector2 _vOrigin, float _fScale )
        {
            DrawBlurredText( _font, _strbLabel, _vPosition, _color, Color.Black, _vOrigin, _fScale );
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Color _blurColor )
        {
            DrawBlurredText( _font, _strbLabel, _vPosition, _color, _blurColor, Vector2.Zero, 1f );
        }


        //----------------------------------------------------------------------
        public void DrawBlurredText( SpriteFont _font, StringBuilder _strbLabel, Vector2 _vPosition, Color _color, Color _blurColor, Vector2 _vOrigin, float _fScale )
        {
            Color blurColor = _blurColor * 0.1f * (_color.A / 255f);

            SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X - BlurRadius, _vPosition.Y - BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X + BlurRadius, _vPosition.Y - BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X + BlurRadius, _vPosition.Y + BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );
            SpriteBatch.DrawString( _font, _strbLabel, new Vector2( _vPosition.X - BlurRadius, _vPosition.Y + BlurRadius ), blurColor, 0f, _vOrigin, _fScale, SpriteEffects.None, 0f );

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

        public SaveData                                     SaveGame;

        //----------------------------------------------------------------------
        // Sound & Music
        public Song                                 Song;

        //----------------------------------------------------------------------
        // Game States
        public GameFlow.GameStateMgr                        GameStateMgr        { get; private set; }

#if WINDOWS || XBOX
        public Input.GamePadManager                         GamePadMgr          { get; private set; }
#endif

#if WINDOWS_PHONE
        public Input.TouchManager                           TouchMgr            { get; private set; }
#endif
    }
}
