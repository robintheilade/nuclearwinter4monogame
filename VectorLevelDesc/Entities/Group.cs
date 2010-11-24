using System;
using System.Collections.Generic;

using System.Text;

namespace VectorLevel.Entities
{
    //--------------------------------------------------------------------------
    public class Group: Entity
    {
        //----------------------------------------------------------------------
        public Group( string _strName, Group _parent )
        : base( _strName, EntityType.Group, _parent )
        {
            Entities = new List<Entity>();
        }

        //----------------------------------------------------------------------
        public List<Entity>     Entities;
    }
}
