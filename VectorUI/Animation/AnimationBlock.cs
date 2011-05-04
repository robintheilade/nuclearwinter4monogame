using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorUI.Animation
{
    public abstract class AnimationBlock
    {
        //----------------------------------------------------------------------
        public AnimationBlock( AnimationLayer _parentLayer, float _fDuration )
        {
            AnimationLayer      = _parentLayer;
            TargetWidgetNames   = new List<string>();
            Duration            = _fDuration;
        }

        //----------------------------------------------------------------------
        public void ResolveWidgets()
        {
            TargetWidgets = new List<Widgets.Widget>();
            foreach( string strWidgetName in TargetWidgetNames )
            {
                TargetWidgets.Add( AnimationLayer.UISheet.GetWidget( strWidgetName ) );
            }

        }

        //----------------------------------------------------------------------
        public abstract bool Update( float _fElapsedTime );

        //----------------------------------------------------------------------
        public float                                        Duration                    { get; private set; }

        public List<string>                                 TargetWidgetNames           { get; private set; }
        public List<Widgets.Widget>                         TargetWidgets;

        public AnimationLayer                               AnimationLayer              { get; private set; }
    }
}
