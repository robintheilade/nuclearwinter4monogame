using System;
using System.Collections.Generic;

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel.Entities
{
    //--------------------------------------------------------------------------
    public class Marker: Entity
    {
        //----------------------------------------------------------------------
        public Marker( string _strName, Group _parent, string _strMarkerType, Vector2 _vPosition, float _fAngle, Color _color )
        : base( _strName, EntityType.Marker, _parent )
        {
            MarkerType      = _strMarkerType;
            Position        = _vPosition;
            Angle           = _fAngle;
            Color           = _color;
        }

        //----------------------------------------------------------------------
        public Vector2      Position    { get; private set; }
        public float        Angle       { get; private set; }
        public string       MarkerType  { get; private set; }
        public Color        Color       { get; private set; }
    }
}