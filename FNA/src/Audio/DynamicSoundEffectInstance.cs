#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.dynamicsoundeffectinstance.aspx
	public sealed class DynamicSoundEffectInstance : SoundEffectInstance
	{
		#region Public Properties

		public int PendingBufferCount
		{
			get;
			private set;
		}

		#endregion

		#region Private XNA Variables

		private int sampleRate;
		private AudioChannels channels;

		#endregion

		#region Private AL Variables

		private Queue<IALBuffer> queuedBuffers;
		private Queue<IALBuffer> buffersToQueue;
		private Queue<IALBuffer> availableBuffers;

		#endregion

		#region BufferNeeded Event

		public event EventHandler<EventArgs> BufferNeeded;

		#endregion

		#region Public Constructor

		public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels) : base(null)
		{
			this.sampleRate = sampleRate;
			this.channels = channels;

			PendingBufferCount = 0;

			queuedBuffers = new Queue<IALBuffer>();
			buffersToQueue = new Queue<IALBuffer>();
			availableBuffers = new Queue<IALBuffer>();
		}

		#endregion

		#region Destructor

		~DynamicSoundEffectInstance()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Method

		public override void Dispose()
		{
			if (!IsDisposed)
			{
				base.Dispose(); // Will call Stop(true);

				// Delete all known buffer objects
				while (queuedBuffers.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(queuedBuffers.Dequeue());
				}
				queuedBuffers = null;
				while (availableBuffers.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(availableBuffers.Dequeue());
				}
				availableBuffers = null;
				while (buffersToQueue.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(buffersToQueue.Dequeue());
				}
				buffersToQueue = null;

				IsDisposed = true;
			}
		}

		#endregion

		#region Public Time/Sample Information Methods

		public TimeSpan GetSampleDuration(int sizeInBytes)
		{
			return SoundEffect.GetSampleDuration(
				sizeInBytes,
				sampleRate,
				channels
			);
		}

		public int GetSampleSizeInBytes(TimeSpan duration)
		{
			return SoundEffect.GetSampleSizeInBytes(
				duration,
				sampleRate,
				channels
			);
		}

		#endregion

		#region Public SubmitBuffer Methods

		public void SubmitBuffer(byte[] buffer)
		{
			this.SubmitBuffer(buffer, 0, buffer.Length);
		}

		public void SubmitBuffer(byte[] buffer, int offset, int count)
		{
			// Generate a buffer if we don't have any to use.
			if (availableBuffers.Count == 0)
			{
				availableBuffers.Enqueue(
					AudioDevice.ALDevice.GenBuffer()
				);
			}

			// Push the data to OpenAL.
			IALBuffer newBuf = availableBuffers.Dequeue();
			AudioDevice.ALDevice.SetBufferData(
				newBuf,
				channels,
				buffer, // TODO: offset -flibit
				count,
				sampleRate
			);

			// If we're already playing, queue immediately.
			if (State == SoundState.Playing)
			{
				AudioDevice.ALDevice.QueueSourceBuffer(
					INTERNAL_alSource,
					newBuf
				);
				queuedBuffers.Enqueue(newBuf);
			}
			else
			{
				buffersToQueue.Enqueue(newBuf);
			}

			PendingBufferCount += 1;
		}

		#endregion

		#region Public Play Method

		public override void Play()
		{
			Play(true);
		}

		#endregion

		#region Internal Play Method

		internal void Play(bool isManaged)
		{
			if (State != SoundState.Stopped)
			{
				return; // No-op if we're already playing.
			}

			if (INTERNAL_alSource != null)
			{
				// The sound has stopped, but hasn't cleaned up yet...
				AudioDevice.ALDevice.StopAndDisposeSource(INTERNAL_alSource);
				INTERNAL_alSource = null;
			}
			while (queuedBuffers.Count > 0)
			{
				availableBuffers.Enqueue(queuedBuffers.Dequeue());
			}

			INTERNAL_alSource = AudioDevice.ALDevice.GenSource();
			if (INTERNAL_alSource == null)
			{
				System.Console.WriteLine("WARNING: AL SOURCE WAS NOT AVAILABLE. SKIPPING.");
				return;
			}

			// Queue the buffers to this source
			while (buffersToQueue.Count > 0)
			{
				IALBuffer nextBuf = buffersToQueue.Dequeue();
				queuedBuffers.Enqueue(nextBuf);
				AudioDevice.ALDevice.QueueSourceBuffer(INTERNAL_alSource, nextBuf);
			}

			// Apply Pan/Position
			if (INTERNAL_positionalAudio)
			{
				INTERNAL_positionalAudio = false;
				AudioDevice.ALDevice.SetSourcePosition(
					INTERNAL_alSource,
					position
				);
			}
			else
			{
				Pan = Pan;
			}

			// Reassign Properties, in case the AL properties need to be applied.
			Volume = Volume;
			IsLooped = IsLooped;
			Pitch = Pitch;

			// Finally.
			AudioDevice.ALDevice.PlaySource(INTERNAL_alSource);
			if (isManaged)
			{
				AudioDevice.DynamicInstancePool.Add(this);
			}

			// ... but wait! What if we need moar buffers?
			if (PendingBufferCount <= 2 && BufferNeeded != null)
			{
				BufferNeeded(this, null);
			}
		}

		#endregion

		#region Internal Update Method

		internal bool Update()
		{
			if (State == SoundState.Stopped)
			{
				/* If we've stopped, remove ourselves from the list.
				 * Do NOT do anything else, Play/Stop/Dispose do this!
				 * -flibit
				 */
				return false;
			}

			// Get the number of processed buffers.
			int finishedBuffers = AudioDevice.ALDevice.CheckProcessedBuffers(
				INTERNAL_alSource
			);
			if (finishedBuffers == 0)
			{
				// Nothing to do... yet.
				return true;
			}

			// Dequeue the processed buffers, error checking as needed.
			AudioDevice.ALDevice.DequeueSourceBuffers(
				INTERNAL_alSource,
				finishedBuffers,
				queuedBuffers
			);

			// The processed buffers are now available.
			for (int i = 0; i < finishedBuffers; i += 1)
			{
				availableBuffers.Enqueue(queuedBuffers.Dequeue());
			}

			// PendingBufferCount changed during playback, trigger now!
			PendingBufferCount -= finishedBuffers;
			if (BufferNeeded != null)
			{
				BufferNeeded(this, null);
			}

			// Do we need even moar buffers?
			if (PendingBufferCount <= 2 && BufferNeeded != null)
			{
				BufferNeeded(this, null);
			}

			return true;
		}

		#endregion

		#region Public FNA Extension Methods

		/* THIS IS AN EXTENSION OF THE XNA4 API! */
		public void SubmitFloatBufferEXT(float[] buffer)
		{
			/* Float samples are the typical format received from decoders.
			 * We currently use this for the VideoPlayer.
			 * -flibit
			 */

			// Generate a buffer if we don't have any to use.
			if (availableBuffers.Count == 0)
			{
				availableBuffers.Enqueue(AudioDevice.ALDevice.GenBuffer());
			}

			// Push buffer to the AL.
			IALBuffer newBuf = availableBuffers.Dequeue();
			AudioDevice.ALDevice.SetBufferData(
				newBuf,
				channels,
				buffer,
				sampleRate
			);

			// If we're already playing, queue immediately.
			if (State == SoundState.Playing)
			{
				AudioDevice.ALDevice.QueueSourceBuffer(
					INTERNAL_alSource,
					newBuf
				);
				queuedBuffers.Enqueue(newBuf);
			}
			else
			{
				buffersToQueue.Enqueue(newBuf);
			}

			PendingBufferCount += 1;
		}

		#endregion
	}
}
