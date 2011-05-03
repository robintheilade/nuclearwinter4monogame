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
            Duration = _fDuration;
        }

        //----------------------------------------------------------------------
        public abstract void Update( float _fElapsedTime );

        //----------------------------------------------------------------------
        public float            Duration                    { get; private set; }

        public AnimationLayer   AnimationLayer              { get; private set; }
    }
}
