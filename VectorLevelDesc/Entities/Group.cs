using System;
using System.Collections.Generic;

using System.Text;

namespace VectorLevel.Entities
{
    public enum GroupMode
    {
        Root,
        Layer,
        Group
    }

    //--------------------------------------------------------------------------
    public class Group: Entity
    {
        //----------------------------------------------------------------------
        public Group( string _strName, Group _parent, GroupMode _groupMode )
        : base( _strName, EntityType.Group, _parent )
        {
            GroupMode = _groupMode;
            Entities = new List<Entity>();
        }

        //----------------------------------------------------------------------
        public GroupMode        GroupMode;
        public List<Entity>     Entities;
    }
}
