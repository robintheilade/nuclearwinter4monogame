using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace VectorUI.Widgets
{
    public class ListView: Widget
    {
        //----------------------------------------------------------------------
        public ListView( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mBorderTex          = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );
            mItemTex            = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "Item" );
            mSelectedItemTex    = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "SelectedItem" );
            Config              = UISheet.Game.Content.Load<ListViewConfig>( _marker.MarkerFullPath + "Config" );

            Position  = new Point( (int)_marker.Position.X, (int)_marker.Position.Y );
            Size      = new Point( (int)_marker.Size.X, (int)_marker.Size.Y );

            mHitRectangle = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );

            mColor = Color.White;

            ItemTextures = new Dictionary<string,Texture2D>();

            SelectedItemIndex = -1;
            Scroll = 0f;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime, bool _bHandleInput )
        {
            if( _bHandleInput )
            {
#if WINDOWS_PHONE
                foreach( TouchLocation touch in UISheet.Game.TouchMgr.Touches )
                {
                    Vector2 vPos = touch.Position;
#elif WINDOWS
                    Vector2 vPos = new Vector2( UISheet.Game.GamePadMgr.MouseState.X, UISheet.Game.GamePadMgr.MouseState.Y );
#endif
                
                    if( mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y ) )
                    {
                        if( 
#if WINDOWS_PHONE
                            touch.State == TouchLocationState.Pressed
#else
                            UISheet.Game.GamePadMgr.WasMouseButtonJustPressed( 0 )
#endif
                        )
                        {
                            mfDragOffset = Scroll + vPos.Y;
                            mfDragPreviousY = vPos.Y;

                            SelectedItemIndex = (int)( ( vPos.Y - ( Position.Y + Config.FramePadding ) + Scroll ) / Config.ItemHeight );
                        }
                        else
                        if( 
#if WINDOWS_PHONE
                            touch.State == TouchLocationState.Released
#else
                            UISheet.Game.GamePadMgr.WasMouseButtonJustReleased( 0 )
#endif
                        )
                        {
                            if( mbDragging )
                            {
                                mbDragging = false;
                            }
                            else
                            if( SelectItem != null )
                            {
                                UISheet.MenuClickSFX.Play();
                                SelectedItemIndex = (int)( ( vPos.Y - ( Position.Y + Config.FramePadding ) + Scroll ) / Config.ItemHeight );
                                SelectItem( this, SelectedItemIndex );
                            }
                        }
                    }

                    if(
#if WINDOWS_PHONE
                        touch.State == TouchLocationState.Moved
#else
                        UISheet.Game.GamePadMgr.MouseState.LeftButton == ButtonState.Pressed
#endif
                    )
                    {
                        if( ! mbDragging && Math.Abs( mfDragPreviousY - vPos.Y ) > 10f )
                        {
                            SelectedItemIndex = -1;
                            mbDragging = true;
                        }
                        
                        if( mbDragging )
                        {
                            Scroll = MathHelper.Clamp( mfDragOffset - vPos.Y, 0f, mListData.Entries.Count * Config.ItemHeight - Size.Y + Config.FramePadding * 2 );
                            ScrollInertia = ( ScrollInertia + ( mfDragPreviousY - vPos.Y ) ) / 2f;
                            mfDragPreviousY = vPos.Y;
                        }
                    }
#if WINDOWS_PHONE
                }
#endif
            }

            if( ! mbDragging  && ScrollInertia != 0f )
            {
                ScrollInertia *= Math.Max( 0f, 1f - ( _fElapsedTime * 5f ) );
                if( Math.Abs( ScrollInertia ) < 1f )
                {
                    ScrollInertia = 0f;
                }
                else
                {
                    Scroll = MathHelper.Clamp( Scroll + ScrollInertia, 0f, mListData.Entries.Count * Config.ItemHeight - Size.Y + Config.FramePadding * 2 );
                }
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Point actualPosition = new Point( Position.X + (int)Offset.X, Position.Y + (int)Offset.Y );

            UISheet.DrawBox( mBorderTex, new Rectangle( actualPosition.X, actualPosition.Y, Size.X, Size.Y ), Config.FrameCornerSize, mColor * Opacity );
            
            Rectangle savedRectangle = UISheet.Game.GraphicsDevice.ScissorRectangle;

            // FIXME: The scissor rectangle will be too big if actualPosition has a negative coordinate!
            UISheet.Game.GraphicsDevice.ScissorRectangle = new Rectangle( Math.Max( 0, actualPosition.X + Config.FramePadding ), Math.Max( 0, actualPosition.Y + Config.FramePadding ), Size.X - Config.FramePadding * 2, Size.Y - Config.FramePadding * 2 );

            for( int iEntry = 0; iEntry < ListData.Entries.Count; iEntry++ )
            {
                UISheet.DrawBox( iEntry == SelectedItemIndex ? mSelectedItemTex : mItemTex, new Rectangle( actualPosition.X + Config.FramePadding, actualPosition.Y + Config.FramePadding + iEntry * Config.ItemHeight - (int)Scroll, Size.X - Config.FramePadding * 2, Config.ItemHeight ), Config.ItemCornerSize, mColor * Opacity );
            }

            int iOffsetX = 0;
            for( int iColumn = 0; iColumn < mListData.Columns.Length; iColumn++ )
            {
                int iEntry = 0;
                switch( ListData.Columns[iColumn].ColumnType )
                {
                    case Models.ListColumnType.Image:
                        foreach( var entry in ListData.Entries )
                        {
                            if( entry.Values[iColumn] != null )
                            {
                                Texture2D tex = ItemTextures[ entry.Values[iColumn] ];

                                UISheet.Game.SpriteBatch.Draw( tex,
                                    new Vector2(
                                        actualPosition.X + iOffsetX + Config.FramePadding + Config.ItemPadding + ListData.Columns[iColumn].Size / 2 - tex.Width / 2,
                                        actualPosition.Y + Config.FramePadding + Config.ItemPadding + Config.ItemHeight * iEntry - (int)Scroll + Config.ItemHeight / 2 - tex.Height / 2 ),
                                    Color.White * Opacity );
                            }
                            iEntry++;
                        }
                        break;
                    case Models.ListColumnType.Text:
                        foreach( var entry in ListData.Entries )
                        {
                            UISheet.Game.SpriteBatch.DrawString( UISheet.SmallFont,
                                entry.Values[iColumn],
                                new Vector2(
                                    actualPosition.X + iOffsetX + Config.FramePadding + Config.ItemPadding,
                                    actualPosition.Y + Config.FramePadding + Config.ItemPadding + Config.ItemHeight * iEntry - (int)Scroll + Config.ItemHeight / 2 - UISheet.SmallFont.MeasureString( entry.Values[iColumn] ).Y / 2 ),
                                Color.Black * Opacity );
                            iEntry++;
                        }

                        break;
                }

                iOffsetX += ListData.Columns[iColumn].Size;
            }

            UISheet.Game.GraphicsDevice.ScissorRectangle = savedRectangle;
        }

        //----------------------------------------------------------------------
        public Models.ListData  ListData {
            get
            {
                return mListData;
            }

            set { 
                mListData = value;
                
                for( int iColumn = 0; iColumn < ListData.Columns.Length; iColumn++ )
                {
                    if( ListData.Columns[iColumn].ColumnType == Models.ListColumnType.Image )
                    {
                        foreach( var entry in ListData.Entries )
                        {
                            // FIXME: Use a ContentManager from UISheet?
                            string strTexName = entry.Values[iColumn];
                            if( strTexName != null )
                            {
                                ItemTextures[strTexName] = UISheet.Game.Content.Load<Texture2D>( strTexName );
                            }
                        }
                    }
                }
            }
        }

        Models.ListData         mListData;

        //----------------------------------------------------------------------
        Texture2D                       mBorderTex;
        Texture2D                       mItemTex;
        Texture2D                       mSelectedItemTex;
        public ListViewConfig           Config;

        Dictionary<string,Texture2D>    ItemTextures;


        public float                    Scroll;
        public int                      SelectedItemIndex;

        bool                            mbDragging;
        float                           mfDragOffset;
        float                           mfDragPreviousY;
        public float                    ScrollInertia;

        Rectangle                       mHitRectangle;

        public Point                    Position;
        public Point                    Size;
        Color                           mColor;
    }
}
