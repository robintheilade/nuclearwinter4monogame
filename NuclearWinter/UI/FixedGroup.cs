using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class FixedGroup: Group
    {
        //----------------------------------------------------------------------
        public bool                 AutoSize = false;

        public int Width {
            get { return ContentWidth; }
            set { ContentWidth = value; }
        }

        public int Height {
            get { return ContentHeight; }
            set { ContentHeight = value; }
        }

        //----------------------------------------------------------------------
        public FixedGroup( Screen _screen )
        : base( _screen )
        {
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            if( AutoSize )
            {
                //ContentWidth = 0;
                ContentHeight = 0;

                foreach( FixedWidget fixedWidget in mlChildren )
                {
                    //ContentWidth    = Math.Max( ContentWidth, fixedWidget.LayoutRect.Right );
                    int iHeight = 0;
                    if( fixedWidget.ChildBox.Top.HasValue )
                    {
                        if( fixedWidget.ChildBox.Bottom.HasValue )
                        {
                            iHeight = fixedWidget.ChildBox.Top.Value + fixedWidget.Child.ContentHeight + fixedWidget.ChildBox.Bottom.Value;
                        }
                        else
                        {
                            iHeight = fixedWidget.ChildBox.Top.Value + fixedWidget.ChildBox.Height;
                        }
                    }

                    ContentHeight = Math.Max( ContentHeight, iHeight );
                }
            }

            base.UpdateContentSize();
        }
    }
}
