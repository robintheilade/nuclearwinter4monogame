using System;
using System.Collections.Generic;

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel.Entities
{
    //--------------------------------------------------------------------------
    public class Text: Entity
    {
        //----------------------------------------------------------------------
        public Text( string _strName, Group _parent, Vector2 _vPosition, float _fAngle )
        : base( _strName, EntityType.Text, _parent )
        {
            Position        = _vPosition;
            Angle           = _fAngle;
            TextSpans       = new List<TextSpan>();
        }

        //----------------------------------------------------------------------
        public List<TextSpan>       TextSpans   { get; private set; }
        public Vector2              Position    { get; private set; }
        public float                Angle       { get; private set; }
    }

    //--------------------------------------------------------------------------
    public class TextSpan
    {
        //----------------------------------------------------------------------
        public TextSpan( string _strValue )
        {
            Value = _strValue;
        }

        //----------------------------------------------------------------------
        public string               Value;
    }
}
