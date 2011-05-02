using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VectorLevel.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace VectorUI.Widgets
{
    public class ListView: Widget
    {
        //----------------------------------------------------------------------
        public ListView( UISheet _sheet, Marker _marker )
        : base( _marker.Name, _sheet )
        {
            mBorderTexture  = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath );
            mItemTexture    = UISheet.Game.Content.Load<Texture2D>( _marker.MarkerFullPath + "Item" );

            Position  = new Point( (int)_marker.Position.X, (int)_marker.Position.Y );
            Size      = new Point( (int)_marker.Size.X, (int)_marker.Size.Y );

            mHitRectangle = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );

            mColor = Color.White;

            ItemTextures = new Dictionary<string,Texture2D>();

            SelectedItemIndex = -1;
            mfScroll = 0f;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            foreach( TouchLocation touch in UISheet.Game.TouchMgr.Touches )
            {
                Vector2 vPos = touch.Position;

                if( mHitRectangle.Contains( (int)vPos.X, (int)vPos.Y ) )
                {
                    if( touch.State == TouchLocationState.Pressed )
                    {
                        mbDragging = true;
                        mfStartDrag = mfScroll + touch.Position.Y;
                    }
                    else
                    if( touch.State == TouchLocationState.Moved && mbDragging )
                    {
                        mfScroll = MathHelper.Clamp( mfStartDrag - touch.Position.Y, 0f, mListData.Entries.Count * 70 - Size.Y + 20 );
                    }
                    else
                    if( touch.State == TouchLocationState.Released )
                    {
                        mbDragging = false;
                    }

                    if( touch.State == TouchLocationState.Released && Click != null )
                    {
                        Click();
                    }
                    break;
                }
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            UISheet.DrawBox( mBorderTexture, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, mColor );


            
            for( int iEntry = 0; iEntry < ListData.Entries.Count; iEntry++ )
            {
                UISheet.DrawBox( mItemTexture, new Rectangle( Position.X + 10, Position.Y + 10 + iEntry * 70 - (int)mfScroll, Size.X - 20, 70 ), 20, mColor );
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
                                UISheet.Game.SpriteBatch.Draw( ItemTextures[ entry.Values[iColumn] ], new Vector2( Position.X + iOffsetX + 10, Position.Y + 10 + 70 * iEntry - (int)mfScroll ), Color.White );
                            }
                            iEntry++;
                        }
                        break;
                    case Models.ListColumnType.Text:
                        foreach( var entry in ListData.Entries )
                        {
                            UISheet.Game.SpriteBatch.DrawString( UISheet.Font, entry.Values[iColumn], new Vector2( Position.X + iOffsetX + 10, Position.Y + 20 + 70 * iEntry - (int)mfScroll ), Color.Black );
                            iEntry++;
                        }

                        break;
                }

                iOffsetX += ListData.Columns[iColumn].Size;
            }
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
        Texture2D                       mBorderTexture;
        Texture2D                       mItemTexture;

        Dictionary<string,Texture2D>    ItemTextures;


        float                           mfScroll;
        public int                      SelectedItemIndex;

        bool                            mbDragging;
        float                           mfStartDrag;

        Rectangle                       mHitRectangle;

        public Point                    Position;
        public Point                    Size;
        Color                           mColor;
    }
}
