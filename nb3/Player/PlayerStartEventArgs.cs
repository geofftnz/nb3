using System;

namespace nb3.Player
{
    public class PlayerStartEventArgs
    {
        public string TrackName { get; set; }
        public int SampleRate { get; set; }
        public TimeSpan TrackLength { get; set; }

    }
}