using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public enum CheckBoxState
    {
        Unchecked,
        Checked,
        Inconsistent
    }

    //--------------------------------------------------------------------------
    public class CheckBox: Widget
    {
        //----------------------------------------------------------------------
        public string Text
        {
            get { return mLabel.Text; }
            set { mLabel.Text = value; }
        }

        public CheckBoxState    CheckState;

        //----------------------------------------------------------------------
        Label                   mLabel;
        bool                    mbIsHovered;
        Rectangle               mCheckBoxRect; 

        //----------------------------------------------------------------------
        public CheckBox( Screen _screen, string _strText )
        : base( _screen )
        {
            mLabel = new Label( Screen, _strText, Anchor.Start );
        }

        protected internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );

            mCheckBoxRect = new Rectangle( LayoutRect.X, LayoutRect.Y, LayoutRect.Height, LayoutRect.Height );
            mLabel.DoLayout( new Rectangle( LayoutRect.X + LayoutRect.Height, LayoutRect.Y, LayoutRect.Width - LayoutRect.Height, LayoutRect.Height ) );
            
            HitBox = LayoutRect;
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseMove( Point _hitPoint )
        {
            mbIsHovered = mCheckBoxRect.Contains( _hitPoint ) || mLabel.LayoutRect.Contains( _hitPoint );
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
        }

        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( mbIsHovered )
            {
                CheckBoxState newState = ( CheckState == CheckBoxState.Checked ) ? CheckBoxState.Unchecked : CheckBoxState.Checked;
                //CheckStateChangedHandler( newState );

                CheckState = newState;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.TreeViewCheckBoxFrame, mCheckBoxRect, Screen.Style.TreeViewCheckBoxFrameCornerSize, Color.White );

            if( mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.TreeViewCheckBoxFrameHover, mCheckBoxRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
            }

            Texture2D tex;
                
            switch( CheckState )
            {
                case UI.CheckBoxState.Checked:
                    tex = Screen.Style.TreeViewCheckBoxChecked;
                    break;
                case UI.CheckBoxState.Unchecked:
                    tex = Screen.Style.TreeViewCheckBoxUnchecked;
                    break;
                case UI.CheckBoxState.Inconsistent:
                    tex = Screen.Style.TreeViewCheckBoxInconsistent;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Screen.Game.SpriteBatch.Draw( tex, new Vector2( mCheckBoxRect.Center.X, mCheckBoxRect.Center.Y ), null, Color.White, 0f, new Vector2( tex.Width, tex.Height ) / 2f, 1f, SpriteEffects.None, 1f );

            mLabel.Draw();
        }
    }
}
