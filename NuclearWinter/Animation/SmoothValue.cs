
using Microsoft.Xna.Framework;

namespace NuclearWinter.Animation
{
    public class SmoothValue : AnimatedValue
    {
        //----------------------------------------------------------------------
        public SmoothValue(float start, float end, float duration, float delay, AnimationLoop loop)
        {
            Start = start;
            End = end;
            Duration = duration;
            Delay = delay;
            Time = 0f;
            Loop = loop;
        }

        public SmoothValue(float start, float end, float duration, AnimationLoop loop)
        : this(start, end, duration, 0f, loop)
        {
        }

        public SmoothValue(float start, float end, float duration, float delay)
        : this(start, end, duration, delay, AnimationLoop.NoLoop)
        {
        }

        public SmoothValue(float start, float end, float duration)
        : this(start, end, duration, 0f, AnimationLoop.NoLoop)
        {
        }

        //----------------------------------------------------------------------
        public override float CurrentValue
        {
            get { return MathHelper.SmoothStep(Start, End, (Time - Delay) / Duration); }
        }

        //----------------------------------------------------------------------
        public float Start;
        public float End;
    }
}
