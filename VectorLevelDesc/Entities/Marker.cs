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
        public Marker( string _strName, Group _parent, string _strMarkerType, Vector2 _vPosition, Vector2 _vSize, float _fAngle, Vector2 _vScale, Color _color )
        : base( _strName, EntityType.Marker, _parent )
        {
            MarkerType      = _strMarkerType;
            Position        = _vPosition;
            Size            = _vSize;
            Angle           = _fAngle;
            Scale           = _vScale;
            Color           = _color;
        }

        //----------------------------------------------------------------------
        public string       MarkerType  { get; private set; }
        public Vector2      Position    { get; private set; }
        public Vector2      Size        { get; private set; }
        public float        Angle       { get; private set; }
        public Vector2      Scale       { get; private set; }
        public Color        Color       { get; private set; }
    }
}