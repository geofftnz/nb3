using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace nb3.Player
{
    /// <summary>
    /// AudioFileReader2 modifications by Geoff Thornburrow.
    /// Copied from NAudio source - may contribute back if it ends up OK.
    /// 
    /// AudioFileReader simplifies opening an audio file in NAudio
    /// Simply pass in the filename, and it will attempt to open the
    /// file and set up a conversion path that turns into PCM IEEE float.
    /// ACM codecs will be used for conversion.
    /// It provides a volume property and implements both WaveStream and
    /// ISampleProvider, making it possibly the only stage in your audio
    /// pipeline necessary for simple playback scenarios
    /// </summary>
    public class AudioFileReader2 : WaveStream, ISampleProvider
    {
        private string fileName;
        private WaveStream readerStream; // the waveStream which we will use for all positioning
        private readonly SampleChannel sampleChannel; // sample provider that gives us most stuff we need
        private readonly int destBytesPerSample;
        private readonly int sourceBytesPerSample;
        private readonly long length;
        private readonly object lockObject;

        /// <summary>
        /// Initializes a new instance of AudioFileReader
        /// </summary>
        /// <param name="fileName">The file to open</param>
        public AudioFileReader2(string fileName)
        {
            lockObject = new object();
            this.fileName = fileName;
            CreateReaderStream(fileName);
            sourceBytesPerSample = (readerStream.WaveFormat.BitsPerSample / 8) * readerStream.WaveFormat.Channels;
            sampleChannel = new SampleChannel(readerStream, false);
            destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
            length = SourceToDest(readerStream.Length);
        }

        /// <summary>
        /// Creates the reader stream, supporting all filetypes in the core NAudio library,
        /// and ensuring we are in PCM format
        /// </summary>
        /// <param name="fileName">File Name</param>
        private void CreateReaderStream(string fileName)
        {
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                //readerStream = new Mp3FileReader(fileName);
                readerStream = new MediaFoundationReader(fileName);
            }
            else if (fileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                //readerStream = new Mp3FileReader(fileName);
                readerStream = new NAudio.Vorbis.VorbisWaveReader(fileName);
            }
            else if (fileName.EndsWith(".flac", StringComparison.OrdinalIgnoreCase))
            {
                //readerStream = new Mp3FileReader(fileName);
                readerStream = new NAudio.Flac.FlacReader(fileName);
            }
            else if (fileName.EndsWith(".aiff"))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }
        }

        /// <summary>
        /// WaveFormat of this stream
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return sampleChannel.WaveFormat; }
        }

        /// <summary>
        /// Length of this stream (in bytes)
        /// </summary>
        public override long Length
        {
            get { return length; }
        }

        /// <summary>
        /// Position of this stream (in bytes)
        /// </summary>
        public override long Position
        {
            get { return SourceToDest(readerStream.Position); }
            set { lock (lockObject) { readerStream.Position = DestToSource(value); } }
        }

        /// <summary>
        /// Reads from this wave stream
        /// </summary>
        /// <param name="buffer">Audio buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">Number of bytes required</param>
        /// <returns>Number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 4;
            int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (lockObject)
            {
                try
                {
                    return sampleChannel.Read(buffer, offset, count);
                }
                catch(Exception) // TODO: FIX FILTHY HACK
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Volume of this AudioFileReader. 1.0f is full volume
        /// </summary>
        public float Volume
        {
            get { return sampleChannel.Volume; }
            set { sampleChannel.Volume = value; }
        }

        /// <summary>
        /// Helper to convert source to dest bytes
        /// </summary>
        private long SourceToDest(long sourceBytes)
        {
            return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
        }

        /// <summary>
        /// Helper to convert dest to source bytes
        /// </summary>
        private long DestToSource(long destBytes)
        {
            return sourceBytesPerSample * (destBytes / destBytesPerSample);
        }

        /// <summary>
        /// Disposes this AudioFileReader
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (readerStream != null)
                {
                    readerStream.Dispose();
                    readerStream = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
