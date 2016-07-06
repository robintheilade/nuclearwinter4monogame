
using Microsoft.Xna.Framework;

namespace NuclearWinter.Animation
{
    public class LerpValue : AnimatedValue
    {
        //----------------------------------------------------------------------
        public LerpValue(float start, float end, float duration, float delay, AnimationLoop loop)
        {
            Start = start;
            End = end;
            Duration = duration;
            Delay = delay;
            Time = 0f;
            Loop = loop;
            Direction = AnimationDirection.Forward;
        }

        public LerpValue(float start, float end, float duration, AnimationLoop loop)
        : this(start, end, duration, 0f, loop)
        {
        }

        public LerpValue(float start, float end, float duration, float felay)
        : this(start, end, duration, felay, AnimationLoop.NoLoop)
        {
        }

        public LerpValue(float start, float end, float duration)
        : this(start, end, duration, 0f, AnimationLoop.NoLoop)
        {
        }

        //----------------------------------------------------------------------
        public override float CurrentValue
        {
            get { return MathHelper.Lerp(Start, End, (Time - Delay) / Duration); }
        }

        //----------------------------------------------------------------------
        public float Start;
        public float End;
    }
}
