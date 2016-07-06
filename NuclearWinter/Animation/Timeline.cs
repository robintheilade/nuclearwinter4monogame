using System;
using System.Collections.Generic;

namespace NuclearWinter.Animation
{
    public class TimelineEvent
    {
        public TimelineEvent(float time, Action action)
        {
            Time = time;
            Action = action;
        }

        public float Time;
        public Action Action;
    }

    public class Timeline
    {
        public Timeline()
        {
            mlEvents = new List<TimelineEvent>();
        }

        public void AddEvent(TimelineEvent @event)
        {
            mlEvents.Add(@event);

            mlEvents.Sort(delegate (TimelineEvent _a, TimelineEvent _b) { return _a.Time.CompareTo(_b.Time); });
        }

        public void Update(float elapsedTime)
        {
            Time += elapsedTime;

            while (miEventOffset < mlEvents.Count && mlEvents[miEventOffset].Time <= Time)
            {
                // NOTE: We must increment miEventOffset before calling the
                // Action, in case the Action calls Reset() or does any other
                // change to the timeline
                miEventOffset++;

                mlEvents[miEventOffset - 1].Action();
            }
        }

        public void Reset()
        {
            Time = 0f;
            miEventOffset = 0;
        }

        public float Time;

        /// Time-sorted list of events
        List<TimelineEvent> mlEvents;
        int miEventOffset;
    }
}
