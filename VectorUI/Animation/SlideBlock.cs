using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorUI.Animation
{
    class SlideBlock: AnimationBlock
    {
        //----------------------------------------------------------------------
        public SlideBlock( AnimationLayer _parentLayer, bool _bSlideIn, float _fDuration, float _fDistance )
        : base( _parentLayer, _fDuration )
        {
            mbSlideIn   = _bSlideIn;
            mfDistance  = _fDistance;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            foreach( Widgets.Widget widget in AnimationLayer.TargetWidgets )
            {

            }
        }

        //----------------------------------------------------------------------
        bool            mbSlideIn;
        float           mfDistance;
    }
}
