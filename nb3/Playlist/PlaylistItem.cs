using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Playlist
{
    public class PlaylistItem
    {
        public string Label { get; set; }
        public string FileName { get; set; }

        public PlaylistItem()
        {

        }

        public override bool Equals(object obj)
        {
            return FileName.Equals(obj);
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
    }
}
