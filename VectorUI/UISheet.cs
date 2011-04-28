using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorLevel;
using VectorLevel.Entities;

using NuclearWinter;

namespace VectorUI
{
    public class UISheet
    {
        //----------------------------------------------------------------------
        public UISheet( NuclearGame _game, LevelDesc _uiDesc )
        {
            Game        = _game;
            mlWidgets   = new List<Widgets.Widget>();

            CreateUIFromRoot( _uiDesc.Root );

            maWidgets   = new Dictionary<string,Widgets.Widget>();
            foreach( Widgets.Widget widget in mlWidgets )
            {
                maWidgets[ widget.Name ] = widget;
            }
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
            if( _marker.MarkerType.StartsWith( "Checkbox" ) )
            {
                mlWidgets.Add( new Widgets.Checkbox( this, _marker ) );
            }
            else
            if( _marker.MarkerType.StartsWith( "Button" ) )
            {
                mlWidgets.Add( new Widgets.Button( this, _marker ) );
            }
            else
            {
                mlWidgets.Add( new Widgets.Image( this, _marker ) );
            }
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            foreach( Widgets.Widget widget in mlWidgets )
            {
                widget.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            Game.SpriteBatch.Begin();

            foreach( Widgets.Widget widget in mlWidgets )
            {
                widget.Draw();
            }

            Game.SpriteBatch.End();
        }

        //----------------------------------------------------------------------
        public Widgets.Widget GetWidget( string _strName )
        {
            return maWidgets[ _strName ];
        }

        //----------------------------------------------------------------------
        public NuclearGame          Game;

        //----------------------------------------------------------------------
        List<Widgets.Widget>                    mlWidgets;
        Dictionary<string, Widgets.Widget>      maWidgets;
    }
}
