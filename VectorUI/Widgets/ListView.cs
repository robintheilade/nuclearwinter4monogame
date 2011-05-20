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
            try
            {
                mDisabledItemTex    = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "DisabledItem" );
            }
            catch
            {
            }
            Config              = UISheet.Game.Content.Load<ListViewConfig>( _marker.MarkerFullPath + "Config" );

            Position  = new Point( (int)_marker.Position.X, (int)_marker.Position.Y );
            Size      = new Point( (int)_marker.Size.X, (int)_marker.Size.Y );

            mHitRectangle = new Rectangle( Position.X + Config.FramePadding, Position.Y + Config.FramePadding, Size.X - Config.FramePadding * 2, Size.Y - Config.FramePadding * 2 );

            mColor = Color.White;

            ItemTextures = new Dictionary<string,Texture2D>();

            SelectedItemIndex = -1;
            Scroll = 0f;
            mfBoundedScroll = 0f;
            AllowDrag = true;
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
                    bool bHitRectangle = mHitRectangle.Contains( (int)(vPos.X - Offset.X), (int)(vPos.Y - Offset.Y) );
                    if( bHitRectangle )
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

                            if( ListData.Entries[SelectedItemIndex].Disabled )
                            {
                                SelectedItemIndex = -1;
                            }
                            else
                            if( ! AllowDrag && OnSelectItem != null )
                            {
                                UISheet.MenuClickSFX.Play();
                                OnSelectItem( this, SelectedItemIndex, vPos );
                            }
                        }
                        else
                        if(
#if WINDOWS_PHONE
                            touch.State == TouchLocationState.Moved
#else
                            UISheet.Game.GamePadMgr.MouseState.LeftButton == ButtonState.Pressed
#endif
                        )
                        {
                            if( AllowDrag && ! mbDragging && Math.Abs( mfDragPreviousY - vPos.Y ) > 10f )
                            {
                                SelectedItemIndex = -1;
                                mbDragging = true;
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
                        if( mbDragging )
                        {
                            Scroll = mfDragOffset - vPos.Y;
                            ScrollInertia = ( ScrollInertia + ( mfDragPreviousY - vPos.Y ) ) / 2f;
                            mfDragPreviousY = vPos.Y;
                        }
                        else
                        if( AllowDrag && bHitRectangle )
                        {
                            SelectedItemIndex = (int)( ( vPos.Y - ( Position.Y + Config.FramePadding ) + Scroll ) / Config.ItemHeight );
                            if( ListData.Entries[SelectedItemIndex].Disabled )
                            {
                                SelectedItemIndex = -1;
                            }
                        }
                        else
                        {
                            SelectedItemIndex = -1;
                        }
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
                        if( AllowDrag && OnSelectItem != null && bHitRectangle )
                        {
                            SelectedItemIndex = (int)( ( vPos.Y - ( Position.Y + Config.FramePadding ) + Scroll ) / Config.ItemHeight );
                            if( ListData.Entries[SelectedItemIndex].Disabled )
                            {
                                SelectedItemIndex = -1;
                            }
                            else
                            {
                                UISheet.MenuClickSFX.Play();
                                OnSelectItem( this, SelectedItemIndex, vPos );
                            }
                        }
                        else
                        {
                            SelectedItemIndex = -1;
                        }
                    }
#if WINDOWS_PHONE
                }
#endif
            }

            Scroll = MathHelper.Lerp( Scroll, mfBoundedScroll, _fElapsedTime * 5f );

            if( Scroll < 0f )
            {
                Scroll /= 2f;
            }
            else
            if( Scroll > mfScrollMax )
            {
                Scroll = mfScrollMax + ( Scroll - mfScrollMax ) / 2f;
            }

            if( ! mbDragging && ScrollInertia != 0f )
            {
                ScrollInertia *= Math.Max( 0f, 1f - ( _fElapsedTime * 5f ) );
                if( Math.Abs( ScrollInertia ) < 1f )
                {
                    ScrollInertia = 0f;
                }
                else
                {
                    Scroll += ScrollInertia;
                }
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Point actualPosition = new Point( Position.X + (int)Offset.X, Position.Y + (int)Offset.Y );

            UISheet.DrawBox( mBorderTex, new Rectangle( actualPosition.X, actualPosition.Y, Size.X, Size.Y ), Config.FrameCornerSize, mColor * Opacity );
            
            UISheet.Game.SpriteBatch.End();

            Rectangle savedRectangle = UISheet.Game.GraphicsDevice.ScissorRectangle;
            // FIXME: The scissor rectangle will be too big if actualPosition has a negative coordinate!
            UISheet.Game.GraphicsDevice.ScissorRectangle = new Rectangle( Math.Max( 0, actualPosition.X + Config.FramePadding ), Math.Max( 0, actualPosition.Y + Config.FramePadding ), Size.X - Config.FramePadding * 2, Size.Y - Config.FramePadding * 2 );

            UISheet.Game.SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, UISheet.RasterizerState );

            for( int iEntry = 0; iEntry < ListData.Entries.Count; iEntry++ )
            {
                Texture2D entryTex = iEntry == SelectedItemIndex ? mSelectedItemTex : mItemTex;
                if( ListData.Entries[iEntry].Disabled )
                {
                    entryTex = mDisabledItemTex;
                }

                UISheet.DrawBox( entryTex, new Rectangle( actualPosition.X + Config.FramePadding, actualPosition.Y + Config.FramePadding + iEntry * Config.ItemHeight - (int)Scroll, Size.X - Config.FramePadding * 2, Config.ItemHeight ), Config.ItemCornerSize, mColor * Opacity );
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
                                Color color = Color.White;
                                if( ListData.Entries[iEntry].Disabled )
                                {
                                    color *= 0.4f;
                                }

                                UISheet.Game.SpriteBatch.Draw( tex,
                                    new Vector2(
                                        actualPosition.X + iOffsetX + Config.FramePadding + Config.ItemPadding + ListData.Columns[iColumn].Size / 2 - tex.Width / 2,
                                        actualPosition.Y + Config.FramePadding + Config.ItemPadding + Config.ItemHeight * iEntry - (int)Scroll + Config.ItemHeight / 2 - tex.Height / 2 ),
                                    color * Opacity );
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

            UISheet.Game.SpriteBatch.End();
            UISheet.Game.GraphicsDevice.ScissorRectangle = savedRectangle;
            UISheet.Game.SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, UISheet.RasterizerState );

        }

        //----------------------------------------------------------------------
        public Models.ListData  ListData {
            get
            {
                return mListData;
            }

            set
            {
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

                mfScrollMax = mListData.Entries.Count * Config.ItemHeight - Size.Y + Config.FramePadding * 2;
            }
        }

        //----------------------------------------------------------------------
        Models.ListData                 mListData;

        //----------------------------------------------------------------------
        public ListViewConfig           Config;

        Texture2D                       mBorderTex;
        Texture2D                       mItemTex;
        Texture2D                       mSelectedItemTex;
        Texture2D                       mDisabledItemTex;

        Dictionary<string,Texture2D>    ItemTextures;

        public bool                     AllowDrag;

        public float                    Scroll
        {
            get {
                return mfScroll;
            }

            set {
                mfScroll = value;
                mfBoundedScroll = MathHelper.Clamp( mfScroll, 0f, mfScrollMax );
            }
        }
        float                           mfScrollMax;
        float                           mfScroll;
        float                           mfBoundedScroll;

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
