using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VectorUI.Animation
{
    class FadeBlock: AnimationBlock
    {
        //----------------------------------------------------------------------
        public FadeBlock( AnimationLayer _parentLayer, bool _bFadeIn, float _fDuration )
        : base( _parentLayer, _fDuration )
        {
            mbFadeIn   = _bFadeIn;
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fTotalTime )
        {
            // FIXME: Add delay support!
            float fActualTime = MathHelper.Clamp( _fTotalTime, 0f, Duration );
            float fProgress = MathHelper.SmoothStep( 0f, 1f, fActualTime / Duration );

            foreach( Widgets.Widget widget in TargetWidgets )
            {
                widget.Opacity = mbFadeIn ? fProgress : ( 1f - fProgress );
            }

            return _fTotalTime >= Duration;
        }

        //----------------------------------------------------------------------
        bool            mbFadeIn;
    }
}
