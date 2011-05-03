using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorLevel;
using VectorLevel.Entities;

using NuclearWinter;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace VectorUI
{
    public class UISheet
    {
        //----------------------------------------------------------------------
        public UISheet( NuclearGame _game, LevelDesc _uiDesc )
        {
            Game        = _game;
            Content     = new ContentManager( Game.Services, "Content" );

            Font        = _game.Content.Load<SpriteFont>( "Fonts/UIFont" );
            SmallFont   = _game.Content.Load<SpriteFont>( "Fonts/UISmallFont" );
            mlWidgets   = new List<Widgets.Widget>();

            CreateUIFromRoot( _uiDesc.Root );

            maWidgets   = new Dictionary<string,Widgets.Widget>();
            foreach( Widgets.Widget widget in mlWidgets )
            {
                maWidgets[ widget.Name ] = widget;
            }

            mRasterizerState = new RasterizerState();
            mRasterizerState.ScissorTestEnable = true;
        }

        //----------------------------------------------------------------------
        void CreateUIFromRoot( Group _root )
        {
            foreach( Entity entity in _root.Entities )
            {
                switch( entity.Type )
                {
                    case VectorLevel.Entities.EntityType.Group:
                        Group group = entity as Group;
                        if( group.GroupMode == GroupMode.Layer )
                        {
                            if( group.Name.StartsWith( "Animation" ) )
                            {
                                VisitAnimationGroup( group );
                            }
                            else
                            {
                                VisitWidgetGroup( group );
                            }
                        }
                        else
                        {
                            throw new Exception( "There can only be layers at UI root" );
                        }
                        break;
                    default:
                        throw new Exception( "There can only be groups at UI root" );
                }
            }
        }

        //----------------------------------------------------------------------
        void VisitWidgetGroup( Group _group )
        {
            foreach( Entity entity in _group.Entities )
            {
                switch( entity.Type )
                {
                    case VectorLevel.Entities.EntityType.Group:
                        VisitWidgetGroup( entity as Group );
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
        void VisitAnimationGroup( Group _group )
        {

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
            Game.SpriteBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, mRasterizerState );

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
        public ContentManager       Content;
        public SpriteFont           Font;
        public SpriteFont           SmallFont;

        //----------------------------------------------------------------------
        List<Widgets.Widget>                    mlWidgets;
        Dictionary<string, Widgets.Widget>      maWidgets;

        public RasterizerState      mRasterizerState;
    }
}
