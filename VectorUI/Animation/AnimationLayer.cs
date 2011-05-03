using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorUI.Animation
{
    // FIXME: See to merge part of this with NuclearWinter.Animation.Timeline
    public class AnimationLayer
    {
        //----------------------------------------------------------------------
        public AnimationLayer( UISheet _uiSheet )
        {
            UISheet             = _uiSheet;

            TargetWidgetNames   = new List<string>();
            AnimationBlocks     = new Dictionary<string,AnimationBlock>();

            Time                = 0f;
        }

        //----------------------------------------------------------------------
        public void Play()
        {
            TargetWidgets = new List<Widgets.Widget>();
            foreach( string strWidgetName in TargetWidgetNames )
            {
                TargetWidgets.Add( UISheet.GetWidget( strWidgetName ) );
            }

            HasStarted = true;
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            if( ! HasStarted ) return;
            
            Time += _fElapsedTime;

            IsDone = true;
            foreach( AnimationBlock block in AnimationBlocks.Values )
            {
                if( ! block.Update( Time ) )
                {
                    IsDone = false;
                }
            }
        }

        //----------------------------------------------------------------------
        public bool     IsDone      { get; private set; }
        public bool     HasStarted  { get; private set; }

        public float    Time        { get; private set; }

        //----------------------------------------------------------------------
        public UISheet                                      UISheet;

        public List<string>                                 TargetWidgetNames   { get; private set; }
        public List<Widgets.Widget>                         TargetWidgets;

        public Dictionary<string,AnimationBlock>            AnimationBlocks     { get; private set; }
    }
}
