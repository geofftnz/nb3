using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// Simple ring buffer.
    /// 
    /// Not particularly threadsafe (so far)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        private int length = 1024;
        private T[] items = null;
        private int current = 0;

        public RingBuffer(int length)
        {
            if (length < 1) throw new ArgumentOutOfRangeException("ring buffer length must be at least 1");
            this.length = length;
            this.items = new T[length];
        }

        public void Add(T sample)
        {
            items[current] = sample;
            current++;
            current %= length;
        }

        public void CopyLastTo(T[] buffer, int offset, int count, int samplesIntoPast = 0)
        {
            if (count > length)
                throw new InvalidOperationException(string.Format("Cannot get more than {0} items from this ring buffer, attempted {1}.", length, count));

            if (offset + count > buffer.Length)
                throw new InvalidOperationException(string.Format("Output buffer overflow {0} > {1}", offset + count, buffer.Length));

            int start = (current - count - samplesIntoPast + length + length + length) % length;
            for (int i = offset; i < offset + count; i++)
            {
                buffer[i] = items[start];
                start++;
                if (start >= length)
                    start = 0;
            }
        }

        public IEnumerable<T> Last()
        {
            int i = (current - 1 + length) % length;

            while (true)
            {
                yield return items[i];

                --i;
                if (i < 0)
                    i = length - 1;
            }
        }

    }
}
