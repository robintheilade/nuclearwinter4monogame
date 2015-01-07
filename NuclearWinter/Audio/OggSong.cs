using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System.IO;
 
namespace NuclearWinter.Audio
{
    // Based on https://gist.github.com/nickgravelyn/5580531
    public class OggSong : IDisposable
    {
        private VorbisReader reader;
        private DynamicSoundEffectInstance effect;
 
        private Thread thread;
        private EventWaitHandle threadRunHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private EventWaitHandle needBufferHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private byte[] buffer;
        private float[] nvBuffer;
 
        public SoundState State
        {
            get { return effect.State; }
        }
 
        public float Volume
        {
            get { return effect.Volume; }
            set { effect.Volume = MathHelper.Clamp(value, 0, 1); }
        }

        public float Pan
        {
            get { return effect.Pan; }
            set { effect.Pan = value; }
        }

        public float Pitch
        {
            get { return effect.Pitch; }
            set { effect.Pitch = value; }
        }
        
        public bool IsLooped { get; set; }
        
        OggSong(VorbisReader _reader)
        {
            reader = _reader;
            effect = new DynamicSoundEffectInstance( reader.SampleRate, (AudioChannels)reader.Channels );
            buffer = new byte[ effect.GetSampleSizeInBytes( TimeSpan.FromMilliseconds(500) ) ];
            nvBuffer = new float[buffer.Length / 2];
 
            // When a buffer is needed, set our handle so the helper thread will read in more data
            effect.BufferNeeded += (s, e) => needBufferHandle.Set();
        }

        public OggSong( string oggFile )
        : this( new VorbisReader( oggFile ) )
        {
        }

        public OggSong( Stream oggStream )
        : this( new VorbisReader( oggStream, true ) )
        {
        }

        ~OggSong()
        {
            Dispose( false );
        }
 
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
 
        protected void Dispose( bool isDisposing )
        {
            // They might be null if the constructor threw an exception

            if( threadRunHandle != null ) threadRunHandle.Set();

            if( effect != null )
            {
                lock( effect )
                {
                    effect.Dispose();
                }
            }
        }
 
        public void Play()
        {
            Stop();

            // Submit a few samples to get started
            // If we submitted less, we'd risk running out of buffers before the thread even starts
            for( int i = 0; i < 8; i++ )
            {
                if( ! SubmitSamples() ) break;
            }

            lock( effect )
            {
                effect.Play();
            }

            // Start the streaming thread
            if( thread == null )
            {
                thread = new Thread( StreamThread );
                thread.IsBackground = true;
                thread.Start();
            }
        }
 
        public void Pause()
        {
            lock( effect )
            {
                effect.Pause();
            }
        }
 
        public void Resume()
        {
            lock( effect )
            {
                effect.Resume();
            }
        }
 
        public void Stop()
        {
            lock( effect )
            {
                if( ! effect.IsDisposed )
                {
                    effect.Stop();
                }
            }
 
            reader.DecodedTime = TimeSpan.Zero;
 
            if( thread != null )
            {
                // Stop our thread
                threadRunHandle.Set();
                thread.Join();
                thread = null;
            }
        }
 
        private void StreamThread()
        {
            lock( effect )
            {
                if( effect.IsDisposed ) return;
            }

            while( true )
            {
                // sleep until we need a buffer
                while( ! effect.IsDisposed && ! threadRunHandle.WaitOne(0) && ! needBufferHandle.WaitOne(0) )
                {
                    Thread.Sleep(50);
                }
 
                // if the thread is waiting to exit, leave
                if( threadRunHandle.WaitOne(0) )
                {
                    threadRunHandle.Reset();
                    break;
                }
                
                // reset our handle
                needBufferHandle.Reset();
                
                lock( effect )
                {
                    if( effect.IsDisposed ) break;
                }

                if( ! SubmitSamples() ) break;
            }
        }

        bool SubmitSamples()
        {
            // read the next chunk of data
            int samplesRead = reader.ReadSamples( nvBuffer, 0, nvBuffer.Length );
            
            // out of data and looping? reset the reader and read again
            if( samplesRead == 0 )
            {
                if( IsLooped )
                {
                    reader.DecodedTime = TimeSpan.Zero;
                    samplesRead = reader.ReadSamples(nvBuffer, 0, nvBuffer.Length);
                }
                else
                {
                    // Song is over, stop thread
                    thread = null;
                    return false;
                }
            }
                
            if( samplesRead > 0 )
            {
                // Submit our buffers
#if !FNA
                for (int i = 0; i < samplesRead; i++)
                {
                    short sValue = (short)Math.Max(Math.Min(short.MaxValue * nvBuffer[i], short.MaxValue), short.MinValue);
                    buffer[i * 2] = (byte)(sValue & 0xff);
                    buffer[i * 2 + 1] = (byte)((sValue >> 8) & 0xff);
                }
#endif
                lock( effect )
                {
                    // Ensure the effect isn't disposed
                    if( effect.IsDisposed ) return false;
                    
                    // Ensure the number of samples read is block-aligned
                    samplesRead += samplesRead % (reader.Channels * 2);

#if !FNA
                    effect.SubmitBuffer(buffer, 0, samplesRead);
                    effect.SubmitBuffer(buffer, samplesRead, samplesRead);
#else
                    effect.SubmitFloatBufferEXT( nvBuffer );
#endif
                }
            }
                
            return true;
        }
    }
}
