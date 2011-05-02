using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorLevel;
using VectorLevel.Entities;

using NuclearWinter;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorUI
{
    public class UISheet
    {
        //----------------------------------------------------------------------
        public UISheet( NuclearGame _game, SpriteFont _font, LevelDesc _uiDesc )
        {
            Game        = _game;
            Font        = _font;
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
                    case VectorLevel.Entities.EntityType.Text:
                        mlWidgets.Add( new Widgets.Label( this, entity as Text ) );
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
            if( _marker.MarkerType.StartsWith( "ListView" ) )
            {
                mlWidgets.Add( new Widgets.ListView( this, _marker ) );
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
        public void DrawBox( Texture2D _tex, Rectangle _extents, int _cornerSize, Color _color )
        {
            // Corners
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Top,                   _cornerSize, _cornerSize ), new Rectangle( 0,                           0,                          _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Top,                   _cornerSize, _cornerSize ), new Rectangle( _tex.Width - _cornerSize,    0,                          _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Bottom - _cornerSize,  _cornerSize, _cornerSize ), new Rectangle( 0,                           _tex.Height - _cornerSize,  _cornerSize, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Bottom - _cornerSize,  _cornerSize, _cornerSize ), new Rectangle( _tex.Width - _cornerSize,    _tex.Height - _cornerSize,  _cornerSize, _cornerSize ), _color );

            // Content
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Top + _cornerSize,     _extents.Width - _cornerSize * 2, _extents.Height - _cornerSize * 2 ), new Rectangle( _cornerSize, _cornerSize, _tex.Width - _cornerSize * 2, _tex.Height - _cornerSize * 2 ), _color );

            // Border top / bottom
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Top,                   _extents.Width - _cornerSize * 2, _cornerSize ), new Rectangle( _cornerSize, 0, _tex.Width - _cornerSize * 2, _cornerSize ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left + _cornerSize,    _extents.Bottom - _cornerSize,  _extents.Width - _cornerSize * 2, _cornerSize ), new Rectangle( _cornerSize, _tex.Height - _cornerSize, _tex.Width - _cornerSize * 2, _cornerSize ), _color );

            // Border left / right
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Left,                  _extents.Top + _cornerSize,     _cornerSize, _extents.Height - _cornerSize * 2 ), new Rectangle( 0, _cornerSize, _cornerSize, _tex.Height - _cornerSize * 2 ), _color );
            Game.SpriteBatch.Draw( _tex, new Rectangle( _extents.Right - _cornerSize,   _extents.Top + _cornerSize,     _cornerSize, _extents.Height - _cornerSize * 2 ), new Rectangle( _tex.Width - _cornerSize, _cornerSize, _cornerSize, _tex.Height - _cornerSize * 2 ), _color );
        }

        //----------------------------------------------------------------------
        public Widgets.Widget GetWidget( string _strName )
        {
            return maWidgets[ _strName ];
        }

        //----------------------------------------------------------------------
        public NuclearGame          Game;
        public SpriteFont           Font;

        //----------------------------------------------------------------------
        List<Widgets.Widget>                    mlWidgets;
        Dictionary<string, Widgets.Widget>      maWidgets;
    }
}
