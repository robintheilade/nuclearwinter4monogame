using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorUI.Widgets
{
    public class Image: Widget
    {
        //----------------------------------------------------------------------
        public Image( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            Texture = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );

            Matrix matrix = Matrix.CreateScale( _marker.Scale.X, _marker.Scale.Y, 0f ) * Matrix.CreateRotationZ( _marker.Angle );
            mfAngle = (float)Math.Atan2( matrix.M12 /* cos angle */, matrix.M11 /* sin angle */ );

            Vector2 vRotatedCenter = Vector2.Transform( _marker.Size / 2f, matrix );
            mvPosition = _marker.Position + vRotatedCenter;

            mvOrigin = new Vector2( Texture.Width, Texture.Height ) / 2f;
            mvScale = _marker.Size / new Vector2( Texture.Width, Texture.Height ) * _marker.Scale;

            mColor = _marker.Color;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.Game.SpriteBatch.Draw( Texture, mvPosition + Offset, null, mColor * Opacity, mfAngle, mvOrigin, mvScale * Scale, SpriteEffects.None, 0f );
        }

        //----------------------------------------------------------------------
        public Texture2D    Texture;

        Vector2             mvPosition;
        float               mfAngle;
        Vector2             mvOrigin;
        Vector2             mvScale;
        Color               mColor;
    }
}
