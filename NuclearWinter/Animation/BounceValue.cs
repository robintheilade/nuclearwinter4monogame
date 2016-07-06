using System;

namespace NuclearWinter.Animation
{
    public class BounceValue : AnimatedValue
    {
        //----------------------------------------------------------------------
        public BounceValue(float start, float end, float duration, int bounceCount, float delay, AnimationLoop loop)
        {
            Start = start;
            End = end;
            Duration = duration;
            BounceCount = bounceCount;
            Delay = delay;
            Time = 0f;
            Loop = loop;
        }

        public BounceValue(float start, float end, float duration, int bounceCount, AnimationLoop loop)
        : this(start, end, duration, bounceCount, 0f, loop)
        {
        }

        public BounceValue(float start, float end, float duration, int bounceCount, float delay)
        : this(start, end, duration, bounceCount, delay, AnimationLoop.NoLoop)
        {
        }

        public BounceValue(float start, float end, float duration, int bounceCount)
        : this(start, end, duration, bounceCount, 0f, AnimationLoop.NoLoop)
        {
        }

        //----------------------------------------------------------------------
        public override float CurrentValue
        {
            get
            {
                // This might be overly complicated.
                // But it (kind of) works and I don't care enough to fix it.
                if (Time <= Delay)
                {
                    return Start;
                }

                float fProgress = (Time - Delay) / Duration;

                float fBounceInterval = 1f / BounceCount;
                fProgress += (fBounceInterval / 2f);

                float fBounceNumber = (int)(fProgress * BounceCount);
                float fCurrentBounceProgress = (fProgress - fBounceNumber * fBounceInterval) / fBounceInterval;

                if (fCurrentBounceProgress > 0.5f)
                {
                    fCurrentBounceProgress = 1f - fCurrentBounceProgress;
                }
                fCurrentBounceProgress = 1f - (1f - fCurrentBounceProgress) * (1f - fCurrentBounceProgress);

                float fBounceAmplitude = (float)Math.Pow((float)fBounceNumber / BounceCount, BounceRestitution);
                float fValue = 1f - (1f - fBounceAmplitude) * fCurrentBounceProgress;

                return Start + fValue * (End - Start);
            }
        }

        //----------------------------------------------------------------------
        public float Start;
        public float End;
        public int BounceCount;
        public float BounceRestitution = 0.5f;
    }
}
