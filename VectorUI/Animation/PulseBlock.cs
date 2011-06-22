using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;

namespace VectorUI.Animation
{
    class PulseBlock: AnimationBlock
    {
        //----------------------------------------------------------------------
        public PulseBlock( AnimationLayer _parentLayer, float _fDuration )
        : base( _parentLayer, _fDuration )
        {
            mfDuration = _fDuration;
            mScaleAnim = new SmoothValue( 1f, 1.2f, mfDuration / 2f, AnimationLoop.LoopBackAndForth );
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fTotalTime )
        {
            mScaleAnim.SetTime( _fTotalTime );
            float fScale = mScaleAnim.CurrentValue;

            foreach( Widgets.Widget widget in TargetWidgets )
            {
                widget.Scale = new Vector2( fScale );
            }

            return _fTotalTime >= Duration;
        }

        //----------------------------------------------------------------------
        float           mfDuration;
        SmoothValue     mScaleAnim;
    }
}
