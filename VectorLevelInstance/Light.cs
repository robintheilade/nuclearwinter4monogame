using System;
using System.Collections.Generic;

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel
{
    /// <summary>
    /// Light source
    /// </summary>
    public class Light
    {
        //---------------------------------------------------------------------
        public Light( Vector2 _vPosition, float _fAngle, float _fRange, Texture2D _texture, Color _color )
        {
            Position    = _vPosition;
            Angle       = _fAngle;
            Range       = _fRange;
            LightTex    = _texture;
            Color       = _color;

            IsEnabled   = false;
        }
        
        //---------------------------------------------------------------------
        public Vector2      Position;
        public float        Angle;
        public float        Range;
        public Texture2D    LightTex;
        public Color        Color;

        public bool         IsEnabled;
    }
}
