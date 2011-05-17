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
using System.Diagnostics;
using VectorUI.Animation;
using Microsoft.Xna.Framework.Audio;

namespace VectorUI
{
    public class UISheet
    {
        //----------------------------------------------------------------------
        public UISheet( NuclearGame _game, LevelDesc _uiDesc )
        {
            Game            = _game;
            Content         = new ContentManager( Game.Services, "Content" );

            mRasterizerState = new RasterizerState();
            mRasterizerState.ScissorTestEnable = true;

            Font            = _game.Content.Load<SpriteFont>( "Fonts/UIFont" );
            SmallFont       = _game.Content.Load<SpriteFont>( "Fonts/UISmallFont" );

            MenuValidateSFX = _game.Content.Load<SoundEffect>( "Sounds/MenuValidate01" );
            MenuClickSFX    = _game.Content.Load<SoundEffect>( "Sounds/MenuClick01" );

            mlWidgets       = new List<Widgets.Widget>();
            AnimationLayers = new Dictionary<string,AnimationLayer>();

            CreateUIFromDesc( _uiDesc );

            maWidgets   = new Dictionary<string,Widgets.Widget>();
            foreach( Widgets.Widget widget in mlWidgets )
            {
                maWidgets[ widget.Name ] = widget;
            }
        }

        //----------------------------------------------------------------------
        void CreateUIFromDesc( LevelDesc _uiDesc )
        {
            foreach( Entity entity in _uiDesc.Root.Entities )
            {
                switch( entity.Type )
                {
                    case EntityType.Group:
                        Group group = entity as Group;
                        if( group.GroupMode == GroupMode.Layer )
                        {
                            if( group.Name.StartsWith( "Animation" ) )
                            {
                                string strAnimationLayerName = group.Name.Substring( "Animation".Length );
                                AnimationLayer layer = new AnimationLayer( this );
                                AnimationLayers[strAnimationLayerName] = layer;

                                VisitAnimationGroup( _uiDesc, layer, group );
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
                    case EntityType.Group:
                        VisitWidgetGroup( entity as Group );
                        break;
                    case EntityType.Marker:
                        CreateUIFromMarker( entity as Marker );
                        break;
                    case EntityType.Text:
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
            if( _marker.MarkerType.StartsWith( "Canvas" ) )
            {
                mlWidgets.Add( new Widgets.Canvas( this, _marker ) );
            }
            else
            {
                mlWidgets.Add( new Widgets.Image( this, _marker ) );
            }
        }

        //----------------------------------------------------------------------
        void VisitAnimationGroup( LevelDesc _uiDesc, AnimationLayer _layer, Group _group )
        {
            foreach( Entity entity in _group.Entities )
            {
                switch( entity.Type )
                {
                    case EntityType.Group:
                        VisitAnimationGroup( _uiDesc, _layer, entity as Group );
                        break;
                    case EntityType.Marker:
                        CreateAnimBlockFromMarker( _uiDesc, _layer, entity as Marker );
                        break;
                    case EntityType.Path:
                        CreateAnimLinkFromPath( _uiDesc, _layer, entity as Path );
                        break;
                    case EntityType.Text:
                        break;
                }
            }
        }

        //----------------------------------------------------------------------
        void CreateAnimBlockFromMarker( LevelDesc _uiDesc, AnimationLayer _layer, Marker _marker )
        {
            AnimationBlock block = null;
            switch( _marker.MarkerType )
            {
                case "SlideIn":
                case "SlideOut":
                    block = new SlideBlock( _layer, _marker.MarkerType.EndsWith( "In" ), 0.3f, 200f, _marker.Angle );
                    break;
                case "Blink":
                    block = new BlinkBlock( _layer, 2f );
                    break;
                case "Pulse":
                    block = new PulseBlock( _layer, 1f );
                    break;
                case "FadeIn":
                case "FadeOut":
                    block = new FadeBlock( _layer, _marker.MarkerType.EndsWith( "In" ), 0.3f );
                    break;
                default:
                    Debug.Assert( false );
                    break;
            }

            Debug.Assert( block != null );

            _layer.AnimationBlocks[ _marker.Name ] = block;
        }

        //----------------------------------------------------------------------
        void CreateAnimLinkFromPath( LevelDesc _uiDesc, AnimationLayer _layer, Path _path )
        {
            List<AnimationBlock> lBlocks    = GetAllBlocks( _uiDesc, _layer, _path.ConnectionStart );
            List<string> lWidgets           = GetAllWidgets( _uiDesc, _path.ConnectionEnd );

            foreach( AnimationBlock block in lBlocks )
            {
                block.TargetWidgetNames.AddRange( lWidgets );
            }
        }

        //----------------------------------------------------------------------
        List<AnimationBlock> GetAllBlocks( LevelDesc _uiDesc, AnimationLayer _layer, string _entityName )
        {
            Entity sourceEntity = _uiDesc.Entities[ _entityName ];

            List<AnimationBlock> lBlocks = new List<AnimationBlock>();

            switch( sourceEntity.Type )
            {
                case EntityType.Group:
                    foreach( Entity entity in ((Group)sourceEntity).Entities )
                    {
                        lBlocks.AddRange( GetAllBlocks( _uiDesc, _layer, entity.Name ) );
                    }

                    break;
                case EntityType.Marker:
                    lBlocks.Add( _layer.AnimationBlocks[ _entityName ] );
                    break;
                default:
                    Debug.Assert( false );
                    break;
            }

            return lBlocks;
        }

        //----------------------------------------------------------------------
        List<string> GetAllWidgets( LevelDesc _uiDesc, string _entityName )
        {
            Entity sourceEntity = _uiDesc.Entities[ _entityName ];

            List<string> lEntities = new List<string>();

            switch( sourceEntity.Type )
            {
                case EntityType.Group:
                    foreach( Entity entity in ((Group)sourceEntity).Entities )
                    {
                        lEntities.AddRange( GetAllWidgets( _uiDesc, entity.Name ) );
                    }

                    break;
                case EntityType.Marker:
                case EntityType.Text:
                    lEntities.Add( _entityName );
                    break;
                default:
                    Debug.Assert( false );
                    break;
            }

            return lEntities;
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime, bool _bHandleInput )
        {
            foreach( Widgets.Widget widget in mlWidgets )
            {
                widget.Update( _fElapsedTime, _bHandleInput );
            }
            
            foreach( AnimationLayer animLayer in AnimationLayers.Values )
            {
                animLayer.Update( _fElapsedTime );
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

        public SoundEffect          MenuValidateSFX;
        public SoundEffect          MenuClickSFX;

        //----------------------------------------------------------------------
        List<Widgets.Widget>                        mlWidgets;
        Dictionary<string, Widgets.Widget>          maWidgets;

        public Dictionary<string,AnimationLayer>    AnimationLayers { get; private set; }

        public RasterizerState      mRasterizerState;
    }
}
