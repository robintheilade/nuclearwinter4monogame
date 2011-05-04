using System;

using System.Collections.Generic;

namespace VectorLevel
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// Vector level
    /// </summary>
    public class LevelDesc
    {
        //----------------------------------------------------------------------
        public LevelDesc()
        {
            Root = new Entities.Group( "", null, VectorLevel.Entities.GroupMode.Root );

            // OrderedEntities are used for writing entities in the proper order
            Entities = new Dictionary<string, Entities.Entity>();
            OrderedEntities = new List<VectorLevel.Entities.Entity>();

            Entities[ Root.Name ] = Root;
            OrderedEntities.Add( Root );
        }
        
        //----------------------------------------------------------------------
        // Data
        public string                                       Title = "";
        public string                                       Description = "";

        public UInt32                                       MapWidth;
        public UInt32                                       MapHeight;
        
        public Entities.Group                               Root;
        public Dictionary<string,Entities.Entity>           Entities;
        public List<Entities.Entity>                        OrderedEntities;
    }
}
