#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using OpenAL;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	internal class OpenALDevice : IALDevice
	{
		#region OpenAL Buffer Container Class

		private class OpenALBuffer : IALBuffer
		{
			public uint Handle
			{
				get;
				private set;
			}

			public TimeSpan Duration
			{
				get;
				private set;
			}

			public OpenALBuffer(uint handle, TimeSpan duration)
			{
				Handle = handle;
				Duration = duration;
			}
		}

		#endregion

		#region OpenAL Source Container Class

		private class OpenALSource : IALSource
		{
			public uint Handle
			{
				get;
				private set;
			}

			public OpenALSource(uint handle)
			{
				Handle = handle;
			}
		}

		#endregion

		#region OpenAL Reverb Effect Container Class

		private class OpenALReverb : IALReverb
		{
			public uint SlotHandle
			{
				get;
				private set;
			}

			public uint EffectHandle
			{
				get;
				private set;
			}

			public OpenALReverb(uint slot, uint effect)
			{
				SlotHandle = slot;
				EffectHandle = effect;
			}
		}

		#endregion

		#region OpenAL Filter Container Class

		private class OpenALFilter : IALFilter
		{
			public uint Handle
			{
				get;
				private set;
			}

			public OpenALFilter(uint handle)
			{
				Handle = handle;
			}
		}

		#endregion

		#region Private ALC Variables

		// OpenAL Device/Context Handles
		private IntPtr alDevice;
		private IntPtr alContext;

		#endregion

		#region Public Constructor

		public OpenALDevice()
		{
			string envDevice = Environment.GetEnvironmentVariable("FNA_AUDIO_DEVICE_NAME");
			if (String.IsNullOrEmpty(envDevice))
			{
				/* Be sure ALC won't explode if the variable doesn't exist.
				 * But, fail if the device name is wrong. The user needs to know
				 * if their environment variable was incorrect.
				 * -flibit
				 */
				envDevice = String.Empty;
			}
			alDevice = ALC10.alcOpenDevice(envDevice);
			if (CheckALCError() || alDevice == IntPtr.Zero)
			{
				throw new Exception("Could not open audio device!");
			}

			int[] attribute = new int[0];
			alContext = ALC10.alcCreateContext(alDevice, attribute);
			if (CheckALCError() || alContext == IntPtr.Zero)
			{
				Dispose();
				throw new Exception("Could not create OpenAL context");
			}

			ALC10.alcMakeContextCurrent(alContext);
			if (CheckALCError())
			{
				Dispose();
				throw new Exception("Could not make OpenAL context current");
			}

			float[] ori = new float[]
			{
				0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f
			};
			AL10.alListenerfv(AL10.AL_ORIENTATION, ori);
			AL10.alListener3f(AL10.AL_POSITION, 0.0f, 0.0f, 0.0f);
			AL10.alListener3f(AL10.AL_VELOCITY, 0.0f, 0.0f, 0.0f);
			AL10.alListenerf(AL10.AL_GAIN, 1.0f);

			// We do NOT use automatic attenuation! XNA does not do this!
			AL10.alDistanceModel(AL10.AL_NONE);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			ALC10.alcMakeContextCurrent(IntPtr.Zero);
			if (alContext != IntPtr.Zero)
			{
				ALC10.alcDestroyContext(alContext);
				alContext = IntPtr.Zero;
			}
			if (alDevice != IntPtr.Zero)
			{
				ALC10.alcCloseDevice(alDevice);
				alDevice = IntPtr.Zero;
			}
		}

		#endregion

		#region Public Update Method

		public void Update()
		{
#if DEBUG
			CheckALError();
#endif
		}

		#endregion

		#region OpenAL Buffer Methods

		public IALBuffer GenBuffer()
		{
			uint result;
			AL10.alGenBuffers((IntPtr) 1, out result);
			return new OpenALBuffer(result, TimeSpan.Zero);
		}

		public IALBuffer GenBuffer(
			byte[] data,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		) {
			uint result;

			// Generate the buffer now, in case we need to perform alBuffer ops.
			AL10.alGenBuffers((IntPtr) 1, out result);

			int format;
			if (isADPCM)
			{
				format = (channels == 2) ?
					ALEXT.AL_FORMAT_STEREO_MSADPCM_SOFT :
					ALEXT.AL_FORMAT_MONO_MSADPCM_SOFT;
				AL10.alBufferi(
					result,
					ALEXT.AL_UNPACK_BLOCK_ALIGNMENT_SOFT,
					(int) formatParameter
				);
			}
			else
			{
				if (formatParameter == 1)
				{
					format = (channels == 2) ?
						AL10.AL_FORMAT_STEREO16:
						AL10.AL_FORMAT_MONO16;
				}
				else
				{
					format = (channels == 2) ?
						AL10.AL_FORMAT_STEREO8:
						AL10.AL_FORMAT_MONO8;
				}
			}

			// Load it!
			AL10.alBufferData(
				result,
				format,
				data,
				(IntPtr) data.Length,
				(IntPtr) sampleRate
			);

			// Calculate the duration now, after we've unpacked the buffer
			int bufLen, bits;
			AL10.alGetBufferi(
				result,
				AL10.AL_SIZE,
				out bufLen
			);
			AL10.alGetBufferi(
				result,
				AL10.AL_BITS,
				out bits
			);
			TimeSpan resultDur = TimeSpan.FromSeconds(
				bufLen /
				(bits / 8) /
				channels /
				((double) sampleRate)
			);

			// Set the loop points, if applicable
			if (loopStart > 0 || loopEnd > 0)
			{
				AL10.alBufferiv(
					result,
					ALEXT.AL_LOOP_POINTS_SOFT,
					new int[]
					{
						(int) loopStart,
						(int) loopEnd
					}
				);
			}

			// Finally.
			return new OpenALBuffer(result, resultDur);
		}

		public void DeleteBuffer(IALBuffer buffer)
		{
			uint handle = (buffer as OpenALBuffer).Handle;
			AL10.alDeleteBuffers((IntPtr) 1, ref handle);
		}

		public void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			byte[] data,
			int count,
			int sampleRate
		) {
			AL10.alBufferData(
				(buffer as OpenALBuffer).Handle,
				XNAToShort[channels],
				data, // TODO: offset -flibit
				(IntPtr) count,
				(IntPtr) sampleRate
			);
		}

		public void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			float[] data,
			int sampleRate
		) {
			AL10.alBufferData(
				(buffer as OpenALBuffer).Handle,
				XNAToFloat[channels],
				data,
				(IntPtr) (data.Length * 4),
				(IntPtr) sampleRate
			);
		}

		#endregion

		#region OpenAL Source Methods

		public IALSource GenSource()
		{
			uint result;
			AL10.alGenSources((IntPtr) 1, out result);
			if (result == 0)
			{
				return null;
			}
			return new OpenALSource(result);
		}

		public IALSource GenSource(IALBuffer buffer)
		{
			uint result;
			AL10.alGenSources((IntPtr) 1, out result);
			if (result == 0)
			{
				return null;
			}
			AL10.alSourcei(
				result,
				AL10.AL_BUFFER,
				(int) (buffer as OpenALBuffer).Handle
			);
			return new OpenALSource(result);
		}

		public void StopAndDisposeSource(IALSource source)
		{
			uint handle = (source as OpenALSource).Handle;
			AL10.alSourceStop(handle);
			AL10.alDeleteSources((IntPtr) 1, ref handle);
		}

		public void PlaySource(IALSource source)
		{
			AL10.alSourcePlay((source as OpenALSource).Handle);
		}

		public void PauseSource(IALSource source)
		{
			AL10.alSourcePause((source as OpenALSource).Handle);
		}

		public void ResumeSource(IALSource source)
		{
			AL10.alSourcePlay((source as OpenALSource).Handle);
		}

		public SoundState GetSourceState(IALSource source)
		{
			int state;
			AL10.alGetSourcei(
				(source as OpenALSource).Handle,
				AL10.AL_SOURCE_STATE,
				out state
			);
			if (state == AL10.AL_PLAYING)
			{
				return SoundState.Playing;
			}
			else if (state == AL10.AL_PAUSED)
			{
				return SoundState.Paused;
			}
			return SoundState.Stopped;
		}

		public void SetSourceVolume(IALSource source, float volume)
		{
			AL10.alSourcef(
				(source as OpenALSource).Handle,
				AL10.AL_GAIN,
				volume * SoundEffect.MasterVolume
			);
		}

		public void SetSourceLooped(IALSource source, bool looped)
		{
			AL10.alSourcei(
				(source as OpenALSource).Handle,
				AL10.AL_LOOPING,
				looped ? 1 : 0
			);
		}

		public void SetSourcePan(IALSource source, float pan)
		{
			AL10.alSource3f(
				(source as OpenALSource).Handle,
				AL10.AL_POSITION,
				pan,
				0.0f,
				(float) Math.Sqrt(1 - Math.Pow(pan, 2))
			);
		}

		public void SetSourcePosition(IALSource source, Vector3 pos)
		{
			AL10.alSource3f(
				(source as OpenALSource).Handle,
				AL10.AL_POSITION,
				pos.X,
				pos.Y,
				pos.Z
			);
		}

		public void SetSourcePitch(IALSource source, float pitch, bool clamp)
		{
			/* XNA sets pitch bounds to [-1.0f, 1.0f], each end being one octave.
			 * OpenAL's AL_PITCH boundaries are (0.0f, INF).
			 * Consider the function f(x) = 2 ^ x
			 * The domain is (-INF, INF) and the range is (0, INF).
			 * 0.0f is the original pitch for XNA, 1.0f is the original pitch for OpenAL.
			 * Note that f(0) = 1, f(1) = 2, f(-1) = 0.5, and so on.
			 * XNA's pitch values are on the domain, OpenAL's are on the range.
			 * Remember: the XNA limit is arbitrarily between two octaves on the domain.
			 * To convert, we just plug XNA pitch into f(x).
			 * -flibit
			 */
			if (clamp && (pitch < -1.0f || pitch > 1.0f))
			{
				throw new Exception("XNA PITCH MUST BE WITHIN [-1.0f, 1.0f]!");
			}
			AL10.alSourcef(
				(source as OpenALSource).Handle,
				AL10.AL_PITCH,
				(float) Math.Pow(2, pitch)
			);
		}

		public void SetSourceReverb(IALSource source, IALReverb reverb)
		{
			AL10.alSource3i(
				(source as OpenALSource).Handle,
				EFX.AL_AUXILIARY_SEND_FILTER,
				(int) (reverb as OpenALReverb).SlotHandle,
				0,
				0
			);
		}

		public void SetSourceFilter(IALSource source, IALFilter filter)
		{
			AL10.alSourcei(
				(source as OpenALSource).Handle,
				EFX.AL_DIRECT_FILTER,
				(int) (filter as OpenALFilter).Handle
			);
		}

		public void QueueSourceBuffer(IALSource source, IALBuffer buffer)
		{
			uint buf = (buffer as OpenALBuffer).Handle;
			AL10.alSourceQueueBuffers(
				(source as OpenALSource).Handle,
				(IntPtr) 1,
				ref buf
			);
		}

		public void DequeueSourceBuffers(
			IALSource source,
			int buffersToDequeue,
			Queue<IALBuffer> errorCheck
		) {
			uint[] bufs = new uint[buffersToDequeue];
			AL10.alSourceUnqueueBuffers(
				(source as OpenALSource).Handle,
				(IntPtr) buffersToDequeue,
				bufs
			);
#if DEBUG
			// Error check our queuedBuffers list.
			IALBuffer[] sync = errorCheck.ToArray();
			for (int i = 0; i < buffersToDequeue; i += 1)
			{
				if (bufs[i] != (sync[i] as OpenALBuffer).Handle)
				{
					throw new Exception("Buffer desync!");
				}
			}
#endif
		}

		public int CheckProcessedBuffers(IALSource source)
		{
			int result;
			AL10.alGetSourcei(
				(source as OpenALSource).Handle,
				AL10.AL_BUFFERS_PROCESSED,
				out result
			);
			return result;
		}

		#endregion

		#region OpenAL Reverb Effect Methods

		public IALReverb GenReverb(DSPParameter[] parameters)
		{
			uint slot, effect;
			EFX.alGenAuxiliaryEffectSlots((IntPtr) 1, out slot);
			EFX.alGenEffects((IntPtr) 1, out effect);
			// Set up the Reverb Effect
			EFX.alEffecti(
				effect,
				EFX.AL_EFFECT_TYPE,
				EFX.AL_EFFECT_EAXREVERB
			);

			IALReverb result = new OpenALReverb(slot, effect);

			// Apply initial values
			SetReverbReflectionsDelay(result, parameters[0].Value);
			SetReverbDelay(result, parameters[1].Value);
			SetReverbPositionLeft(result, parameters[2].Value);
			SetReverbPositionRight(result, parameters[3].Value);
			SetReverbPositionLeftMatrix(result, parameters[4].Value);
			SetReverbPositionRightMatrix(result, parameters[5].Value);
			SetReverbEarlyDiffusion(result, parameters[6].Value);
			SetReverbLateDiffusion(result, parameters[7].Value);
			SetReverbLowEQGain(result, parameters[8].Value);
			SetReverbLowEQCutoff(result, parameters[9].Value);
			SetReverbHighEQGain(result, parameters[10].Value);
			SetReverbHighEQCutoff(result, parameters[11].Value);
			SetReverbRearDelay(result, parameters[12].Value);
			SetReverbRoomFilterFrequency(result, parameters[13].Value);
			SetReverbRoomFilterMain(result, parameters[14].Value);
			SetReverbRoomFilterHighFrequency(result, parameters[15].Value);
			SetReverbReflectionsGain(result, parameters[16].Value);
			SetReverbGain(result, parameters[17].Value);
			SetReverbDecayTime(result, parameters[18].Value);
			SetReverbDensity(result, parameters[19].Value);
			SetReverbRoomSize(result, parameters[20].Value);
			SetReverbWetDryMix(result, parameters[21].Value);

			// Bind the Effect to the EffectSlot. XACT will use the EffectSlot.
			EFX.alAuxiliaryEffectSloti(
				slot,
				EFX.AL_EFFECTSLOT_EFFECT,
				(int) effect
			);

			return result;
		}

		public void DeleteReverb(IALReverb reverb)
		{
			OpenALReverb rv = (reverb as OpenALReverb);
			uint slot = rv.SlotHandle;
			uint effect = rv.EffectHandle;
			EFX.alDeleteAuxiliaryEffectSlots((IntPtr) 1, ref slot);
			EFX.alDeleteEffects((IntPtr) 1, ref effect);
		}

		public void CommitReverbChanges(IALReverb reverb)
		{
			OpenALReverb rv = (reverb as OpenALReverb);
			EFX.alAuxiliaryEffectSloti(
				rv.SlotHandle,
				EFX.AL_EFFECTSLOT_EFFECT,
				(int) rv.EffectHandle
			);
		}

		public void SetReverbReflectionsDelay(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_REFLECTIONS_DELAY,
				value / 1000.0f
			);
		}

		public void SetReverbDelay(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_LATE_REVERB_DELAY,
				value / 1000.0f
			);
		}

		public void SetReverbPositionLeft(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionRight(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionLeftMatrix(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbPositionRightMatrix(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbEarlyDiffusion(IALReverb reverb, float value)
		{
			// Same as late diffusion, whatever... -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DIFFUSION,
				value / 15.0f
			);
		}

		public void SetReverbLateDiffusion(IALReverb reverb, float value)
		{
			// Same as early diffusion, whatever... -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DIFFUSION,
				value / 15.0f
			);
		}

		public void SetReverbLowEQGain(IALReverb reverb, float value)
		{
			// Cutting off volumes from 0db to 4db! -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAINLF,
				Math.Min(
					XACTCalculator.CalculateAmplitudeRatio(
						value - 8.0f
					),
					1.0f
				)
			);
		}

		public void SetReverbLowEQCutoff(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_LFREFERENCE,
				(value * 50.0f) + 50.0f
			);
		}

		public void SetReverbHighEQGain(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAINHF,
				XACTCalculator.CalculateAmplitudeRatio(
					value - 8.0f
				)
			);
		}

		public void SetReverbHighEQCutoff(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_HFREFERENCE,
				(value * 500.0f) + 1000.0f
			);
		}

		public void SetReverbRearDelay(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterFrequency(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterMain(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbReflectionsGain(IALReverb reverb, float value)
		{
			// Cutting off possible float values above 3.16, for EFX -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_REFLECTIONS_GAIN,
				Math.Min(
					XACTCalculator.CalculateAmplitudeRatio(value),
					3.16f
				)
			);
		}

		public void SetReverbGain(IALReverb reverb, float value)
		{
			// Cutting off volumes from 0db to 20db! -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAIN,
				Math.Min(
					XACTCalculator.CalculateAmplitudeRatio(value),
					1.0f
				)
			);
		}

		public void SetReverbDecayTime(IALReverb reverb, float value)
		{
			/* FIXME: WTF is with this XACT value?
			 * XACT: 0-30 equal to 0.1-inf seconds?!
			 * EFX: 0.1-20 seconds
			 * -flibit
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_GAIN,
				value
			);
			*/
		}

		public void SetReverbDensity(IALReverb reverb, float value)
		{
			EFX.alEffectf(
				(reverb as OpenALReverb).EffectHandle,
				EFX.AL_EAXREVERB_DENSITY,
				value / 100.0f
			);
		}

		public void SetReverbRoomSize(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		public void SetReverbWetDryMix(IALReverb reverb, float value)
		{
			// No known mapping :(
		}

		#endregion

		#region OpenAL Filter Methods

		public IALFilter GenFilter()
		{
			uint handle;
			EFX.alGenFilters((IntPtr) 1, out handle);
			return new OpenALFilter(handle);
		}

		public void DeleteFilter(IALFilter filter)
		{
			uint handle = (filter as OpenALFilter).Handle;
			EFX.alDeleteFilters((IntPtr) 1, ref handle);
		}

		public void ApplyLowPassFilter(IALFilter filter, float hfGain)
		{
			uint handle = (filter as OpenALFilter).Handle;
			EFX.alFilteri(handle, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_LOWPASS);
			EFX.alFilterf(handle, EFX.AL_LOWPASS_GAINHF, hfGain);
		}

		public void ApplyHighPassFilter(IALFilter filter, float lfGain)
		{
			uint handle = (filter as OpenALFilter).Handle;
			EFX.alFilteri(handle, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_HIGHPASS);
			EFX.alFilterf(handle, EFX.AL_HIGHPASS_GAINLF, lfGain);
		}

		public void ApplyBandPassFilter(IALFilter filter, float hfGain, float lfGain)
		{
			uint handle = (filter as OpenALFilter).Handle;
			EFX.alFilteri(handle, EFX.AL_FILTER_TYPE, EFX.AL_FILTER_BANDPASS);
			EFX.alFilterf(handle, EFX.AL_BANDPASS_GAINHF, hfGain);
			EFX.alFilterf(handle, EFX.AL_BANDPASS_GAINLF, lfGain);
		}

		#endregion

		#region Private OpenAL Error Check Methods

		private void CheckALError()
		{
			int err = AL10.alGetError();

			if (err == AL10.AL_NO_ERROR)
			{
				return;
			}

			System.Console.WriteLine("OpenAL Error: {0:X}", err);
		}

		private bool CheckALCError()
		{
			int err = ALC10.alcGetError(alDevice);

			if (err == ALC10.ALC_NO_ERROR)
			{
				return false;
			}

			System.Console.WriteLine("OpenAL Device Error: {0:X}", err);
			return true;
		}

		#endregion

		#region Private Static XNA->AL Dictionaries

		private static readonly Dictionary<AudioChannels, int> XNAToShort = new Dictionary<AudioChannels, int>
		{
			{ AudioChannels.Mono, AL10.AL_FORMAT_MONO16 },
			{ AudioChannels.Stereo, AL10.AL_FORMAT_STEREO16 }
		};

		private static readonly Dictionary<AudioChannels, int> XNAToFloat = new Dictionary<AudioChannels, int>
		{
			{ AudioChannels.Mono, ALEXT.AL_FORMAT_MONO_FLOAT32 },
			{ AudioChannels.Stereo, ALEXT.AL_FORMAT_STEREO_FLOAT32 }
		};

		#endregion

		#region OpenAL Device Enumerator

		public ReadOnlyCollection<RendererDetail> GetDevices()
		{
			IntPtr deviceList = ALC10.alcGetString(IntPtr.Zero, ALEXT.ALC_ALL_DEVICES_SPECIFIER);
			List<RendererDetail> renderers = new List<RendererDetail>();

			int i = 0;
			string curString = Marshal.PtrToStringAnsi(deviceList);
			while (!String.IsNullOrEmpty(curString))
			{
				renderers.Add(new RendererDetail(
					curString,
					i.ToString()
				));
				i += 1;
				deviceList += curString.Length + 1;
				curString = Marshal.PtrToStringAnsi(deviceList);
			}

			return new ReadOnlyCollection<RendererDetail>(renderers);
		}

		#endregion
	}
}
