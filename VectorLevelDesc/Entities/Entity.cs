using System;
using System.Collections.Generic;

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel.Entities
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// An Entity definition base class
    /// </summary>
    public class Entity
    {
        //----------------------------------------------------------------------
        public Entity( string _strEntityName, EntityType _entityType, Group _parent )
        {
            Name    = _strEntityName;
            Type    = _entityType;
            Parent  = _parent;
            
            if( _parent != null )
            {
                _parent.Entities.Add( this );
            }
        }

        //----------------------------------------------------------------------
        public string       Name        { get; private set; }
        public Group       Parent;
        public EntityType   Type        { get; private set; }
    }

    //--------------------------------------------------------------------------
    public enum EntityType
    {
        Marker,
        Path,
        Text,
        Group
    };
}
