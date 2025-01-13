#pragma warning disable format
#pragma warning disable IDE0044

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FAudioDecodeCallback = nint;
using FAudioFilterState = Microsoft.Xna.Framework.Vector4;
using FAudioMixCallback = nint;
using FAudioMutex = nint;
using FAudioResampleCallback = nint;
using FAudioThread = nint;
using int16_t = System.Int16;
using int32_t = System.Int32;
using int64_t = System.Int64;
using int8_t = System.SByte;
using uint16_t = System.UInt16;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;
using uint8_t = System.Byte;

#region FAudio.h
//typedef struct FAudio FAudio;
//typedef struct FAudioVoice FAudioVoice;
using FAudioSourceVoice = FAudioINTERNAL.FAudioVoice;
using FAudioSubmixVoice = FAudioINTERNAL.FAudioVoice;
using FAudioMasteringVoice = FAudioINTERNAL.FAudioVoice;
#endregion FAudio.h

#region FAudio_internal.h
using static FAudioINTERNAL.FAudio;

/* FAudio - XAudio Reimplementation for FNA
*
* Copyright (c) 2011-2023 Ethan Lee, Luigi Auriemma, and the MonoGame Team
*
* This software is provided 'as-is', without any express or implied warranty.
* In no event will the authors be held liable for any damages arising from
* the use of this software.
*
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
*
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software in a
* product, an acknowledgment in the product documentation would be
* appreciated but is not required.
*
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
*
* 3. This notice may not be removed or altered from any source distribution.
*
* Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
*
*/

// SOME TYPES DO NOT FULLY MATCH THE C++ IMPL
// IF YOU WANNA MAKE SURE THEY DO CHECK
// FACT.c https://github.com/FNA-XNA/FAudio/blob/29a7d3a726383a3907baf4930d2c4d4da773b023/src/FACT.c
// FAudio.c https://github.com/FNA-XNA/FAudio/blob/29a7d3a726383a3907baf4930d2c4d4da773b023/include/FAudio.h#L1293
// FAudio_internal.h https://github.com/FNA-XNA/FAudio/blob/29a7d3a726383a3907baf4930d2c4d4da773b023/src/FAudio_internal.h
// and other files
// Make public the types that are going to be used

namespace FAudioINTERNAL
{
	#endregion

	enum FAPOBufferFlags
	{
		FAPO_BUFFER_SILENT,
		FAPO_BUFFER_VALID
	}

	enum FAudioFilterType
	{
		FAudioLowPassFilter,
		FAudioBandPassFilter,
		FAudioHighPassFilter,
		FAudioNotchFilter
	}
	struct FAudioFilterParametersEXT
	{
		readonly FAudioFilterType Type;
		readonly float Frequency;    /* [0, FAUDIO_MAX_FILTER_FREQUENCY] */
		readonly float OneOverQ;     /* [0, FAUDIO_MAX_FILTER_ONEOVERQ] */
		readonly float WetDryMix;    /* [0, 1] */
	}

	//typedef struct FAudioEngineCallback FAudioEngineCallback;
	//typedef struct FAudioVoiceCallback FAudioVoiceCallback;
	#region FAudio.h
	unsafe struct FAudioGUID
	{
		readonly uint32_t Data1;
		readonly uint16_t Data2;
		readonly uint16_t Data3;
		fixed uint8_t Data4[8];
	}
	unsafe struct FAudioSendDescriptor
	{
		readonly uint32_t Flags; /* 0 or FAUDIO_SEND_USEFILTER */
		readonly FAudioVoice* pOutputVoice;
	}
	unsafe struct FAudioVoiceSends
	{
		readonly uint32_t SendCount;
		readonly FAudioSendDescriptor* pSends;
	}
	unsafe struct FAudioEffectDescriptor
	{
		/*FAPO*/
		readonly void* pEffect;
		readonly int32_t InitialState; /* 1 - Enabled, 0 - Disabled */
		readonly uint32_t OutputChannels;
	}

	unsafe struct FAudioBuffer
	{
		/* Either 0 or FAUDIO_END_OF_STREAM */
		readonly uint32_t Flags;
		/* Pointer to wave data, memory block size.
		 * Note that pAudioData is not copied; FAudio reads directly from your
		 * pointer! This pointer must be valid until FAudio has finished using
		 * it, at which point an OnBufferEnd callback will be generated.
		 */
		readonly uint32_t AudioBytes;
		readonly uint8_t* pAudioData; // readonly
		/* Play region, in sample frames. */
		readonly uint32_t PlayBegin;
		readonly uint32_t PlayLength;
		/* Loop region, in sample frames.
		 * This can be used to loop a subregion of the wave instead of looping
		 * the whole thing, i.e. if you have an intro/outro you can set these
		 * to loop the middle sections instead. If you don't need this, set both
		 * values to 0.
		 */
		readonly uint32_t LoopBegin;
		readonly uint32_t LoopLength;
		/* [0, FAUDIO_LOOP_INFINITE] */
		readonly uint32_t LoopCount;
		/* This is sent to callbacks as pBufferContext */
		readonly void* pContext;
	}
	unsafe struct FAudioBufferWMA
	{
		readonly uint32_t* pDecodedPacketCumulativeBytes;
		readonly uint32_t PacketCount;
	}
	struct FAudioWaveFormatEx
	{
		readonly uint16_t wFormatTag;
		readonly uint16_t nChannels;
		readonly uint32_t nSamplesPerSec;
		readonly uint32_t nAvgBytesPerSec;
		readonly uint16_t nBlockAlign;
		readonly uint16_t wBitsPerSample;
		readonly uint16_t cbSize;
	}

	struct FAudioWaveFormatExtensible
	{
		FAudioWaveFormatEx Format;
		//union
		[StructLayout(LayoutKind.Explicit)]
		struct _Samples
		{
			[FieldOffset(0)] readonly uint16_t wValidBitsPerSample;
			[FieldOffset(0)] readonly uint16_t wSamplesPerBlock;
			[FieldOffset(0)] readonly uint16_t wReserved;
		}
		_Samples Samples;
		readonly uint32_t dwChannelMask;
		FAudioGUID SubFormat;
	}
	#endregion

	#region FAudio_internal.h

	//typedef int32_t (FAUDIOCALL * FAudioThreadFunc)(void* data);
	enum FAudioThreadPriority
	{
		FAUDIO_THREAD_PRIORITY_LOW,
		FAUDIO_THREAD_PRIORITY_NORMAL,
		FAUDIO_THREAD_PRIORITY_HIGH,
	}

	/* Linked Lists */
	unsafe struct LinkedList
	{
		readonly void* entry;
		readonly LinkedList* next;
	};

	/* Internal FAudio Types */

	enum FAudioVoiceType
	{
		FAUDIO_VOICE_SOURCE,
		FAUDIO_VOICE_SUBMIX,
		FAUDIO_VOICE_MASTER
	}

	unsafe struct FAudioBufferEntry
	{
		FAudioBuffer buffer;
		FAudioBufferWMA bufferWMA;
		readonly FAudioBufferEntry* next;
	};

	//unsafe delegate void FAudioDecodeCallback_(
	//	FAudioVoice *voice,
	//	FAudioBuffer *buffer,	/* Buffer to decode */
	//	float *decodeCache,	/* Decode into here */
	//	uint32_t samples	/* Samples to decode */
	//);

	//unsafe delegate void FAudioResampleCallback_(
	//	float *restrict dCache,
	//	float *restrict resampleCache,
	//	uint64_t *resampleOffset,
	//	uint64_t resampleStep,
	//	uint64_t toResample,
	//	uint8_t channels
	//);

	//unsafe delegate void FAudioMixCallback_(
	//	uint32_t toMix,
	//	uint32_t srcChans,
	//	uint32_t dstChans,
	//	float *restrict srcData,
	//	float *restrict dstData,
	//	float *restrict coefficients
	//);

	/* Operation Sets, original implementation by Tyler Glaiel */

	/* Public FAudio Types */

	unsafe struct FAudio
	{
		readonly uint8_t version;
		readonly uint8_t active;
		readonly uint32_t refcount;
		readonly uint32_t initFlags;
		readonly uint32_t updateSize;
		readonly FAudioMasteringVoice* master;
		readonly LinkedList* sources;
		readonly LinkedList* submixes;
		readonly LinkedList* callbacks;
		readonly FAudioMutex sourceLock;
		readonly FAudioMutex submixLock;
		readonly FAudioMutex callbackLock;
		readonly FAudioMutex operationLock;
		FAudioWaveFormatExtensible mixFormat;

		/*FAudio_OPERATIONSET_Operation*/
		readonly void* queuedOperations;
		/*FAudio_OPERATIONSET_Operation*/
		readonly void* committedOperations;

		/* Used to prevent destroying an active voice */
		readonly FAudioSourceVoice* processingSource;

		/* Temp storage for processing, interleaved PCM32F */
		// #define EXTRA_DECODE_PADDING 2
		readonly uint32_t decodeSamples;
		readonly uint32_t resampleSamples;
		readonly uint32_t effectChainSamples;
		readonly float* decodeCache;
		readonly float* resampleCache;
		readonly float* effectChainCache;

		/* Allocator callbacks */
		/*FAudioMallocFunc*/
		readonly void* pMalloc;
		/*FAudioFreeFunc*/
		readonly void* pFree;
		/*FAudioReallocFunc*/
		readonly void* pRealloc;

		/* EngineProcedureEXT */
		readonly void* clientEngineUser;
		/*FAudioEngineProcedureEXT*/
		readonly void* pClientEngineProc;

		// #ifndef FAUDIO_DISABLE_DEBUGCONFIGURATION
		/* Debug Information */
		// FAudioDebugConfiguration debug;
		// #endif /* FAUDIO_DISABLE_DEBUGCONFIGURATION */

		/* Platform opaque pointer */
		readonly void* platform;
	};

	unsafe struct FAudioVoice
	{
		readonly FAudio* audio;
		readonly uint32_t flags;
		readonly FAudioVoiceType type;

		FAudioVoiceSends sends;
		readonly float** sendCoefficients;
		readonly float** mixCoefficients;
		readonly FAudioMixCallback* sendMix;
		/*FAudioFilterParametersEXT*/
		readonly void* sendFilter;
		readonly FAudioFilterState** sendFilterState;
		struct _effects
		{
			readonly FAPOBufferFlags state;
			readonly uint32_t count;
			readonly FAudioEffectDescriptor* desc;
			readonly void** parameters;
			readonly uint32_t* parameterSizes;
			readonly uint8_t* parameterUpdates;
			readonly uint8_t* inPlaceProcessing;
		}
		_effects effects;
		FAudioFilterParametersEXT filter;
		readonly FAudioFilterState* filterState;
		readonly FAudioMutex sendLock;
		readonly FAudioMutex effectLock;
		readonly FAudioMutex filterLock;

		readonly float volume;
		readonly float* channelVolume;
		readonly uint32_t outputChannels;
		readonly FAudioMutex volumeLock;

		//FAUDIONAMELESS union
		[StructLayout(LayoutKind.Explicit)]
		struct NAMELESSUNION0
		{
			struct _src
			{
				/* Sample storage */
				readonly uint32_t decodeSamples;
				readonly uint32_t resampleSamples;

				/* Resampler */
				readonly float resampleFreq;
				readonly uint64_t resampleStep;
				readonly uint64_t resampleOffset;
				readonly uint64_t curBufferOffsetDec;
				readonly uint32_t curBufferOffset;

				/* WMA decoding */
				//#ifdef HAVE_WMADEC
				/*FAudioWMADEC*/
				readonly void* wmadec;
				//#endif /* HAVE_WMADEC*/

				/* Read-only */
				readonly float maxFreqRatio;
				readonly FAudioWaveFormatEx* format;
				readonly FAudioDecodeCallback decode;
				readonly FAudioResampleCallback resample;
				/*FAudioVoiceCallback*/
				readonly void* callback;

				/* Dynamic */
				readonly uint8_t active;
				readonly float freqRatio;
				readonly uint8_t newBuffer;
				readonly uint64_t totalSamples;
				readonly FAudioBufferEntry* bufferList;
				readonly FAudioBufferEntry* flushList;
				readonly FAudioMutex bufferLock;
			}
			[FieldOffset(0)] _src src;
			struct _mix
			{
				/* Sample storage */
				readonly uint32_t inputSamples;
				readonly uint32_t outputSamples;
				readonly float* inputCache;
				readonly uint64_t resampleStep;
				readonly FAudioResampleCallback resample;

				/* Read-only */
				readonly uint32_t inputChannels;
				readonly uint32_t inputSampleRate;
				readonly uint32_t processingStage;
			}
			[FieldOffset(0)] _mix mix;
			struct _master
			{
				/* Output stream, allocated by Platform */
				readonly float* output;

				/* Needed when inputChannels != outputChannels */
				readonly float* effectCache;

				/* Read-only */
				readonly uint32_t inputChannels;
				readonly uint32_t inputSampleRate;
			}
			[FieldOffset(0)] _master master;
		};
	};

	#endregion // FAudio_internal.h

	/* FAudio - XAudio Reimplementation for FNA
	 *
	 * Copyright (c) 2011-2023 Ethan Lee, Luigi Auriemma, and the MonoGame Team
	 *
	 * This software is provided 'as-is', without any express or implied warranty.
	 * In no event will the authors be held liable for any damages arising from
	 * the use of this software.
	 *
	 * Permission is granted to anyone to use this software for any purpose,
	 * including commercial applications, and to alter it and redistribute it
	 * freely, subject to the following restrictions:
	 *
	 * 1. The origin of this software must not be misrepresented; you must not
	 * claim that you wrote the original software. If you use this software in a
	 * product, an acknowledgment in the product documentation would be
	 * appreciated but is not required.
	 *
	 * 2. Altered source versions must be plainly marked as such, and must not be
	 * misrepresented as being the original software.
	 *
	 * 3. This notice may not be removed or altered from any source distribution.
	 *
	 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
	 *
	 */

	/* Internal AudioEngine Types */

	unsafe struct FACTAudioCategory
	{
		readonly uint8_t instanceLimit;
		readonly uint16_t fadeInMS;
		readonly uint16_t fadeOutMS;
		readonly uint8_t maxInstanceBehavior;
		readonly int16_t parentCategory;
		readonly float volume;
		readonly uint8_t visibility;

		readonly uint8_t instanceCount;
		readonly float currentVolume;
	}

	unsafe struct FACTVariable
	{
		readonly uint8_t accessibility;
		readonly float initialValue;
		readonly float minValue;
		readonly float maxValue;
	}

	unsafe struct FACTRPCPoint
	{
		readonly float x;
		readonly float y;
		readonly uint8_t type;
	}

	enum FACTRPCParameter
	{
		RPC_PARAMETER_VOLUME,
		RPC_PARAMETER_PITCH,
		RPC_PARAMETER_REVERBSEND,
		RPC_PARAMETER_FILTERFREQUENCY,
		RPC_PARAMETER_FILTERQFACTOR,
		RPC_PARAMETER_COUNT /* If >=, DSP Parameter! */
	}

	unsafe struct FACTRPC
	{
		readonly uint16_t variable;
		readonly uint8_t pointCount;
		readonly uint16_t parameter;
		readonly FACTRPCPoint* points;
	}

	unsafe struct FACTDSPParameter
	{
		readonly uint8_t type;
		readonly float value;
		readonly float minVal;
		readonly float maxVal;
		readonly uint16_t unknown;
	}

	unsafe struct FACTDSPPreset
	{
		readonly uint8_t accessibility;
		readonly uint16_t parameterCount;
		readonly FACTDSPParameter* parameters;
	}

	enum FACTNoticationsFlags
	{
		NOTIFY_CUEPREPARED = 0x00000001,
		NOTIFY_CUEPLAY = 0x00000002,
		NOTIFY_CUESTOP = 0x00000004,
		NOTIFY_CUEDESTROY = 0x00000008,
		NOTIFY_MARKER = 0x00000010,
		NOTIFY_SOUNDBANKDESTROY = 0x00000020,
		NOTIFY_WAVEBANKDESTROY = 0x00000040,
		NOTIFY_LOCALVARIABLECHANGED = 0x00000080,
		NOTIFY_GLOBALVARIABLECHANGED = 0x00000100,
		NOTIFY_GUICONNECTED = 0x00000200,
		NOTIFY_GUIDISCONNECTED = 0x00000400,
		NOTIFY_WAVEPREPARED = 0x00000800,
		NOTIFY_WAVEPLAY = 0x00001000,
		NOTIFY_WAVESTOP = 0x00002000,
		NOTIFY_WAVELOOPED = 0x00004000,
		NOTIFY_WAVEDESTROY = 0x00008000,
		NOTIFY_WAVEBANKPREPARED = 0x00010000,
		NOTIFY_WAVEBANKSTREAMING_INVALIDCONTENT = 0x00020000
	}

	/* Internal SoundBank Types */

	enum FACTEventType
	{
		FACTEVENT_STOP = 0,
		FACTEVENT_PLAYWAVE = 1,
		FACTEVENT_PLAYWAVETRACKVARIATION = 3,
		FACTEVENT_PLAYWAVEEFFECTVARIATION = 4,
		FACTEVENT_PLAYWAVETRACKEFFECTVARIATION = 6,
		FACTEVENT_PITCH = 7,
		FACTEVENT_VOLUME = 8,
		FACTEVENT_MARKER = 9,
		FACTEVENT_PITCHREPEATING = 16,
		FACTEVENT_VOLUMEREPEATING = 17,
		FACTEVENT_MARKERREPEATING = 18
	}

	unsafe struct FACTEvent
	{
		readonly uint16_t type;
		readonly uint16_t timestamp;
		readonly uint16_t randomOffset;
		//FAUDIONAMELESS union
		[StructLayout(LayoutKind.Explicit)]
		unsafe struct NAMELESSUNION0
		{
			/* Play Wave Event */
			unsafe struct _wave
			{
				readonly uint8_t flags;
				readonly uint8_t loopCount;
				readonly uint16_t position;
				readonly uint16_t angle;

				/* Track Variation */
				readonly uint8_t isComplex;
				//FAUDIONAMELESS union
				[StructLayout(LayoutKind.Explicit)]
				unsafe struct NAMELESSUNION1
				{
					unsafe struct _simple
					{
						readonly uint16_t track;
						readonly uint8_t wavebank;
					}
					[FieldOffset(0)] _simple simple;
					unsafe struct _complex
					{
						readonly uint16_t variation;
						readonly uint16_t trackCount;
						readonly uint16_t* tracks;
						readonly uint8_t* wavebanks;
						readonly uint8_t* weights;
					}
					[FieldOffset(0)] _complex complex;
				};

				/* Effect Variation */
				readonly int16_t minPitch;
				readonly int16_t maxPitch;
				readonly float minVolume;
				readonly float maxVolume;
				readonly float minFrequency;
				readonly float maxFrequency;
				readonly float minQFactor;
				readonly float maxQFactor;
				readonly uint16_t variationFlags;
			}
			[FieldOffset(0)] _wave wave;
			/* Set Pitch/Volume Event */
			unsafe struct _value
			{
				readonly uint8_t settings;
				readonly uint16_t repeats;
				readonly uint16_t frequency;
				//FAUDIONAMELESS union
				[StructLayout(LayoutKind.Explicit)]
				unsafe struct NAMELESSUNION3
				{
					unsafe struct _ramp
					{
						readonly float initialValue;
						readonly float initialSlope;
						readonly float slopeDelta;
						readonly uint16_t duration;
					}
					[FieldOffset(0)] _ramp ramp;
					unsafe struct _equation
					{
						readonly uint8_t flags;
						readonly float value1;
						readonly float value2;
					}
					[FieldOffset(0)] _equation equation;
				};
			}
			[FieldOffset(0)] _value value;
			/* Stop Event */
			unsafe struct _stop
			{
				readonly uint8_t flags;
			}
			[FieldOffset(0)] _stop stop;
			/* Marker Event */
			unsafe struct _marker
			{
				readonly uint32_t marker;
				readonly uint16_t repeats;
				readonly uint16_t frequency;
			}
			[FieldOffset(0)] _marker marker;
		};
	}

	unsafe struct FACTTrack
	{
		public uint32_t code;

		public float volume;
		public uint8_t filter;
		public uint8_t qfactor;
		public uint16_t frequency;

		public uint8_t rpcCodeCount;
		public uint32_t* rpcCodes;

		public uint8_t eventCount;
		public FACTEvent* events;
	}

	unsafe struct FACTSound
	{
		public uint8_t flags;
		public uint16_t category;
		public float volume;
		public int16_t pitch;
		public uint8_t priority;

		public uint8_t trackCount;
		public uint8_t rpcCodeCount;
		public uint8_t dspCodeCount;

		public FACTTrack* tracks;
		public uint32_t* rpcCodes;
		public uint32_t* dspCodes;
	}

	unsafe struct FACTCueData
	{
		readonly uint8_t flags;
		readonly uint32_t sbCode;
		readonly uint32_t transitionOffset;
		readonly uint8_t instanceLimit;
		readonly uint16_t fadeInMS;
		readonly uint16_t fadeOutMS;
		readonly uint8_t maxInstanceBehavior;
		readonly uint8_t instanceCount;
	}

	unsafe struct FACTVariation
	{
		//FAUDIONAMELESS union
		[StructLayout(LayoutKind.Explicit)]
		unsafe struct NAMELESSUNION0
		{
			unsafe struct _simple
			{
				readonly uint16_t track;
				readonly uint8_t wavebank;
			}
			[FieldOffset(0)] _simple simple;
			[FieldOffset(0)] readonly uint32_t soundCode;
		}
		NAMELESSUNION0 union;
		readonly float minWeight;
		readonly float maxWeight;
		readonly uint32_t linger;
	}

	unsafe struct FACTVariationTable
	{
		readonly uint8_t flags;
		readonly int16_t variable;
		readonly uint8_t isComplex;

		readonly uint16_t entryCount;
		readonly FACTVariation* entries;
	}

	unsafe struct FACTTransition
	{
		readonly int32_t soundCode;
		readonly uint32_t srcMarkerMin;
		readonly uint32_t srcMarkerMax;
		readonly uint32_t dstMarkerMin;
		readonly uint32_t dstMarkerMax;
		readonly uint16_t fadeIn;
		readonly uint16_t fadeOut;
		readonly uint16_t flags;
	}

	unsafe struct FACTTransitionTable
	{
		readonly uint32_t entryCount;
		readonly FACTTransition* entries;
	}

	/* Internal WaveBank Types */

	unsafe struct FACTSeekTable
	{
		readonly uint32_t entryCount;
		readonly uint32_t* entries;
	}

	/* Internal Cue Types */

	unsafe struct FACTInstanceRPCData
	{
		readonly float rpcVolume;
		readonly float rpcPitch;
		readonly float rpcReverbSend;
		readonly float rpcFilterFreq;
		readonly float rpcFilterQFactor;
	}

	unsafe struct FACTEventInstance
	{
		readonly uint32_t timestamp;
		readonly uint16_t loopCount;
		readonly uint8_t finished;
		//FAUDIONAMELESS union
		[StructLayout(LayoutKind.Explicit)]
		unsafe struct NAMELESSUNION0
		{
			[FieldOffset(0)] readonly float value;
			[FieldOffset(0)] readonly uint32_t valuei;
		};
	}

	unsafe struct FACTTrackInstance
	{
		/* Tracks which events have fired */
		public FACTEventInstance* events;

		/* RPC instance data */
		public FACTInstanceRPCData rpcData;

		/* SetPitch/SetVolume data */
		public float evtPitch;
		public float evtVolume;

		/* Wave playback */
		public unsafe struct _wave
		{
			public FACTWave* wave;
			public float baseVolume;
			public int16_t basePitch;
			public float baseQFactor;
			public float baseFrequency;
		}
		public _wave activeWave, upcomingWave;
		public FACTEvent* waveEvt;
		public FACTEventInstance* waveEvtInst;
	}

	unsafe struct FACTSoundInstance
	{
		/* Base Sound reference */
		public FACTSound* sound;

		/* Per-instance track information */
		public FACTTrackInstance* tracks;

		/* RPC instance data */
		public FACTInstanceRPCData rpcData;

		/* Fade data */
		public uint32_t fadeStart;
		public uint16_t fadeTarget;
		public uint8_t fadeType; /* In (1), Out (2), Release RPC (3) */

		/* Engine references */
		public FACTCue* parentCue;
	}

	/* Internal Wave Types */

	unsafe struct FACTWaveCallback
	{
		/*FAudioVoiceCallback*/
		readonly void* callback;
		readonly FACTWave* wave;
	}

	/* Public XACT Types */

	unsafe struct FACTAudioEngine
	{
		public uint32_t refcount;
		/*FACTNotificationCallback*/
		public void* notificationCallback;
		/*FACTReadFileCallback*/
		public void* pReadFile;
		/*FACTGetOverlappedResultCallback*/
		public void* pGetOverlappedResult;

		public uint16_t categoryCount;
		public uint16_t variableCount;
		public uint16_t rpcCount;
		public uint16_t dspPresetCount;
		public uint16_t dspParameterCount;

		public char** categoryNames;
		public char** variableNames;
		public uint32_t* rpcCodes;
		public uint32_t* dspPresetCodes;

		public FACTAudioCategory* categories;
		public FACTVariable* variables;
		public FACTRPC* rpcs;
		public FACTDSPPreset* dspPresets;

		/* Engine references */
		public LinkedList* sbList;
		public LinkedList* wbList;
		public FAudioMutex sbLock;
		public FAudioMutex wbLock;
		public float* globalVariableValues;

		/* FAudio references */
		public FAudio* audio;
		public FAudioMasteringVoice* master;
		public FAudioSubmixVoice* reverbVoice;

		/* Engine thread */
		public FAudioThread apiThread;
		public FAudioMutex apiLock;
		public uint8_t initialized;

		/* Allocator callbacks */
		/*FAudioMallocFunc*/
		public void* pMalloc;
		/*FAudioFreeFunc*/
		public void* pFree;
		/*FAudioReallocFunc*/
		public void* pRealloc;

		/* Peristent Notifications */
		public FACTNoticationsFlags notifications;
		public void* cue_context;
		public void* sb_context;
		public void* wb_context;
		public void* wave_context;
		public LinkedList* wb_notifications_list;

		/* Settings handle */
		public void* settings;
	}

	unsafe struct FACTSoundBank
	{
		/* Engine references */
		public FACTAudioEngine* parentEngine;
		public FACTCue* cueList;
		public uint8_t notifyOnDestroy;
		public void* usercontext;

		/* Array sizes */
		public uint16_t cueCount;
		public uint8_t wavebankCount;
		public uint16_t soundCount;
		public uint16_t variationCount;
		public uint16_t transitionCount;

		/* Strings, strings everywhere! */
		public char** wavebankNames;
		public char** cueNames;

		/* Actual SoundBank information */
		public char* name;
		public FACTCueData* cues;
		public FACTSound* sounds;
		public uint32_t* soundCodes;
		public FACTVariationTable* variations;
		public uint32_t* variationCodes;
		public FACTTransitionTable* transitions;
		public uint32_t* transitionCodes;
	}

	unsafe struct FACTWaveBank
	{
		/* Engine references */
		readonly FACTAudioEngine* parentEngine;
		readonly LinkedList* waveList;
		readonly FAudioMutex waveLock;
		readonly uint8_t notifyOnDestroy;
		readonly void* usercontext;

		/* Actual WaveBank information */
		readonly char* name;
		readonly uint32_t entryCount;
		/*FACTWaveBankEntry*/
		readonly void* entries;
		readonly uint32_t* entryRefs;
		readonly FACTSeekTable* seekTables;
		readonly char* waveBankNames;

		/* I/O information */
		readonly uint32_t packetSize;
		readonly uint16_t streaming;
		readonly uint8_t* packetBuffer;
		readonly uint32_t packetBufferLen;
		readonly void* io;
	}

	unsafe struct FACTWave
	{
		/* Engine references */
		public FACTWaveBank* parentBank;
		public FACTCue* parentCue;
		public uint16_t index;
		public uint8_t notifyOnDestroy;
		public void* usercontext;

		/* Playback */
		public uint32_t state;
		public float volume;
		public int16_t pitch;
		public uint8_t loopCount;

		/* Stream data */
		public uint32_t streamSize;
		public uint32_t streamOffset;
		public uint8_t* streamCache;

		/* FAudio references */
		public uint16_t srcChannels;
		public FAudioSourceVoice* voice;
		public FACTWaveCallback callback;
	}

	unsafe struct FACTCue
	{
		/* Engine references */
		public FACTSoundBank* parentBank;
		public FACTCue* next;
		public uint8_t managed;
		public uint16_t index;
		public uint8_t notifyOnDestroy;
		public void* usercontext;

		/* Sound data */
		public FACTCueData* data;
		//FAUDIONAMELESS union
		[StructLayout(LayoutKind.Explicit)]
		public struct NAMELESSUNION0
		{
			[FieldOffset(0)] public FACTVariationTable* variation;

			/* This is only used in scenarios where there is only one
			 * Sound; XACT does not generate variation tables for
			 * Cues with only one Sound.
			 */
			[FieldOffset(0)] public FACTSound* sound;
		}
		public NAMELESSUNION0 union0;

		/* Instance data */
		public float* variableValues;
		public float interactive;

		/* Playback */
		public uint32_t state;
		public FACTWave* simpleWave;
		public FACTSoundInstance* playingSound;
		public FACTVariation* playingVariation;
		public uint32_t maxRpcReleaseTime;

		/* 3D Data */
		public uint8_t active3D;
		public uint32_t srcChannels;
		public uint32_t dstChannels;
		public fixed float matrixCoefficients[2 * 8]; /* Stereo input, 7.1 output */

		/* Timer */
		public uint32_t start;
		public uint32_t elapsed;
	}

#pragma warning restore format
#pragma warning restore IDE0044
}