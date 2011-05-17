using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VectorUI.Animation
{
    class BlinkBlock: AnimationBlock
    {
        //----------------------------------------------------------------------
        public BlinkBlock( AnimationLayer _parentLayer, float _fDuration )
        : base( _parentLayer, _fDuration )
        {
            mfDuration = _fDuration;
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fTotalTime )
        {
            bool bVisible = ( _fTotalTime % mfDuration ) < mfDuration * 3f / 4f;

            foreach( Widgets.Widget widget in TargetWidgets )
            {
                widget.Opacity = bVisible ? 1f : 0f;
            }

            return _fTotalTime >= Duration;
        }

        //----------------------------------------------------------------------
        float       mfDuration;
    }
}
