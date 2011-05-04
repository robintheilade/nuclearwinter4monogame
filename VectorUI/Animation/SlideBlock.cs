using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VectorUI.Animation
{
    class SlideBlock: AnimationBlock
    {
        //----------------------------------------------------------------------
        public SlideBlock( AnimationLayer _parentLayer, bool _bSlideIn, float _fDuration, float _fDistance, float _fAngle )
        : base( _parentLayer, _fDuration )
        {
            mbSlideIn       = _bSlideIn;
            mfDistance      = _fDistance;
            mfAngle         = _fAngle;
            mRotationMatrix = Matrix.CreateRotationZ( mfAngle );
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fTotalTime )
        {
            // FIXME: Add delay support!
            float fActualTime = MathHelper.Clamp( _fTotalTime, 0f, Duration );
            float fProgress = MathHelper.SmoothStep( 0f, 1f, fActualTime / Duration );
            
            foreach( Widgets.Widget widget in TargetWidgets )
            {
                widget.Offset = Vector2.Transform( new Vector2( ( mbSlideIn ? -mfDistance : 0f ) + fProgress * mfDistance, 0f ), mRotationMatrix );
            }

            return _fTotalTime >= Duration;
        }

        //----------------------------------------------------------------------
        bool            mbSlideIn;
        float           mfDistance;

        float           mfAngle;
        Matrix          mRotationMatrix;
    }
}
