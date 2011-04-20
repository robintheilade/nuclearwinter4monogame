using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.Animation
{
    public class TimelineEvent
    {
        public TimelineEvent( float _fTime, Action _action )
        {
            Time        = _fTime;
            Action      = _action;
        }

        public float        Time;
        public Action       Action;
    }

    public class Timeline
    {
        public Timeline()
        {
            mlEvents = new List<TimelineEvent>();
        }

        public void AddEvent( TimelineEvent _event )
        {
            mlEvents.Add( _event );

            mlEvents.Sort( delegate( TimelineEvent _a, TimelineEvent _b ) { return _b.Time.CompareTo( _a.Time ); } );
        }

        public void Update( float _fElapsedTime )
        {
            mfTime += _fElapsedTime;

            while( mlEvents.Count > 0 && mlEvents[ mlEvents.Count - 1 ].Time <= mfTime )
            {
                mlEvents[ mlEvents.Count - 1 ].Action();
                mlEvents.RemoveAt( mlEvents.Count - 1 );
            }
        }

        /// Time-sorted list of events
        List<TimelineEvent>     mlEvents;
        float                   mfTime;
    }
}
