using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorLevel;
using VectorLevel.Entities;

namespace VectorUI
{
    public class UISheet
    {
        //----------------------------------------------------------------------
        public UISheet( LevelDesc _uiDesc )
        {
            CreateUIFromRoot( _uiDesc.Root );
        }

        //----------------------------------------------------------------------
        void CreateUIFromRoot( Group _root )
        {
            foreach( Entity entity in _root.Entities )
            {
                switch( entity.Type )
                {
                    case VectorLevel.Entities.EntityType.Group:
                        VisitGroup( entity as Group );
                        break;
                    default:
                        throw new Exception( "There can only be groups at UI root" );
                }
            }
        }

        //----------------------------------------------------------------------
        void VisitGroup( Group _group )
        {
            foreach( Entity entity in _group.Entities )
            {
                switch( entity.Type )
                {
                    case VectorLevel.Entities.EntityType.Group:
                        VisitGroup( entity as Group );
                        break;
                    case VectorLevel.Entities.EntityType.Marker:
                        CreateUIFromMarker( entity as Marker );
                        break;
                }
            }
        }

        //----------------------------------------------------------------------
        void CreateUIFromMarker( Marker _marker )
        {
            if( _marker.MarkerType.StartsWith( "Button" ) )
            {

            }
        }

        //----------------------------------------------------------------------

    }
}
