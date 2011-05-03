using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorUI.Animation
{
    public class AnimationLayer
    {
        //----------------------------------------------------------------------
        public AnimationLayer( UISheet _uiSheet )
        {
            UISheet             = _uiSheet;

            TargetWidgetNames   = new List<string>();
            AnimationBlocks     = new List<KeyValuePair<float,AnimationBlock>>();
        }

        //----------------------------------------------------------------------
        public void Play()
        {
            TargetWidgets = new List<Widgets.Widget>();
            foreach( string strWidgetName in TargetWidgetNames )
            {
                TargetWidgets.Add( UISheet.GetWidget( strWidgetName ) );
            }
        }

        //----------------------------------------------------------------------
        public void Update( float _fElapsedTime )
        {
            if( ! HasStarted ) return;
            
            foreach( KeyValuePair<float,AnimationBlock> block in AnimationBlocks )
            {
                block.Value.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        public bool IsDone      { get; private set; }
        public bool HasStarted  { get; private set; }

        //----------------------------------------------------------------------
        public UISheet                                      UISheet;

        public List<string>                                 TargetWidgetNames   { get; private set; }
        public List<Widgets.Widget>                         TargetWidgets;

        public List<KeyValuePair<float,AnimationBlock>>     AnimationBlocks     { get; private set; }
    }
}
