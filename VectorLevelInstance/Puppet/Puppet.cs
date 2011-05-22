using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace VectorLevel.Puppet
{
    public class Puppet
    {
        //----------------------------------------------------------------------
        public Puppet( LevelRenderer _levelRenderer, PuppetDef _def, string _strDefaultAnimation )
        {
            mLevelRenderer      = _levelRenderer;

            mPuppetDef          = _def;

            CurrentAnimation    = mPuppetDef.Animations[ _strDefaultAnimation ];
            Texture             = mLevelRenderer.Content.Load<Texture2D>( CurrentAnimation.TextureName );
        }

        //----------------------------------------------------------------------
        public void SetCurrentAnimation( string _strAnimationName )
        {
            CurrentAnimation    = mPuppetDef.Animations[ _strAnimationName ];
            Texture             = mLevelRenderer.Content.Load<Texture2D>( CurrentAnimation.TextureName );
            Time                = 0f;
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            Time += _fElapsedTime;
        }

        //----------------------------------------------------------------------
        public Rectangle GetFrameRect( int _iFrame )
        {
            return new Rectangle( _iFrame * CurrentAnimation.FrameWidth,  0, (int)CurrentAnimation.FrameWidth, (int)CurrentAnimation.FrameHeight );
        }

        //----------------------------------------------------------------------
        public void Draw( Vector2 _vPosition, Color _color, float _fAngle, Vector2 _vOrigin )
        {
            Draw( _vPosition, _color, _fAngle, _vOrigin, 1f, SpriteEffects.None );
        }

        //----------------------------------------------------------------------
        public void Draw( Vector2 _vPosition, Color _color, float _fAngle, Vector2 _vOrigin, float _fScale, SpriteEffects _spriteEffects )
        {
            int iFrameCount = Texture.Width / CurrentAnimation.FrameWidth;
            int iFrame = CurrentAnimation.IsLooping ? (int)( Time / CurrentAnimation.FrameDuration ) % iFrameCount : Math.Min( (int)( Time / CurrentAnimation.FrameDuration ), iFrameCount - 1 );

            Rectangle sourceRect = GetFrameRect( iFrame );

            mLevelRenderer.DrawSprite( Texture, _vPosition, sourceRect, _color, _fAngle, _vOrigin, new Vector2( _fScale ), _spriteEffects );
        }

        //----------------------------------------------------------------------
        public PuppetAnimation              CurrentAnimation { get; private set; }
        public Texture2D                    Texture;
        public float                        Time;

        //----------------------------------------------------------------------
        LevelRenderer                       mLevelRenderer;

        PuppetDef                           mPuppetDef;
    }
}
