using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint8_t = System.Byte;
using int8_t = System.SByte;
using int16_t = System.Int16;
using uint16_t = System.UInt16;
using int32_t = System.Int32;
using uint32_t = System.UInt32;
using int64_t = System.Int64;
using uint64_t = System.UInt64;

using FAudioThread = System.IntPtr;
using FAudioMutex = System.IntPtr;

using FAudioDecodeCallback = System.IntPtr;
using FAudioResampleCallback = System.IntPtr;
using FAudioMixCallback = System.IntPtr;
using FAudioFilterState = Microsoft.Xna.Framework.Vector4;
using System.Runtime.InteropServices;

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

namespace FAudioINTERNAL;


#region FAudio.h
//typedef struct FAudio FAudio;
//typedef struct FAudioVoice FAudioVoice;
using FAudioSourceVoice = FAudioVoice;
using FAudioSubmixVoice = FAudioVoice;
using FAudioMasteringVoice = FAudioVoice;
#endregion FAudio.h

#region FAudio_internal.h
using FAudioThread = System.IntPtr; //typedef void* FAudioThread;
using FAudioMutex = System.IntPtr; //typedef void* FAudioMutex;
using static FAudio;
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
    FAudioFilterType Type;
    float Frequency;    /* [0, FAUDIO_MAX_FILTER_FREQUENCY] */
    float OneOverQ;     /* [0, FAUDIO_MAX_FILTER_ONEOVERQ] */
    float WetDryMix;	/* [0, 1] */
}

//typedef struct FAudioEngineCallback FAudioEngineCallback;
//typedef struct FAudioVoiceCallback FAudioVoiceCallback;
#region FAudio.h
unsafe struct FAudioGUID
{
    uint32_t Data1;
    uint16_t Data2;
    uint16_t Data3;
    fixed uint8_t Data4[8];
}
unsafe struct FAudioSendDescriptor
{
    uint32_t Flags; /* 0 or FAUDIO_SEND_USEFILTER */
    FAudioVoice* pOutputVoice;
}
unsafe struct FAudioVoiceSends
{
    uint32_t SendCount;
    FAudioSendDescriptor* pSends;
}
unsafe struct FAudioEffectDescriptor
{
    /*FAPO*/
    void* pEffect;
    int32_t InitialState; /* 1 - Enabled, 0 - Disabled */
    uint32_t OutputChannels;
}

unsafe struct FAudioBuffer
{
    /* Either 0 or FAUDIO_END_OF_STREAM */
    uint32_t Flags;
    /* Pointer to wave data, memory block size.
	 * Note that pAudioData is not copied; FAudio reads directly from your
	 * pointer! This pointer must be valid until FAudio has finished using
	 * it, at which point an OnBufferEnd callback will be generated.
	 */
    uint32_t AudioBytes;
    uint8_t* pAudioData; // readonly
    /* Play region, in sample frames. */
    uint32_t PlayBegin;
    uint32_t PlayLength;
    /* Loop region, in sample frames.
	 * This can be used to loop a subregion of the wave instead of looping
	 * the whole thing, i.e. if you have an intro/outro you can set these
	 * to loop the middle sections instead. If you don't need this, set both
	 * values to 0.
	 */
    uint32_t LoopBegin;
    uint32_t LoopLength;
    /* [0, FAUDIO_LOOP_INFINITE] */
    uint32_t LoopCount;
    /* This is sent to callbacks as pBufferContext */
    void* pContext;
}
unsafe struct FAudioBufferWMA
{
    uint32_t* pDecodedPacketCumulativeBytes;
    uint32_t PacketCount;
}
struct FAudioWaveFormatEx
{
    uint16_t wFormatTag;
    uint16_t nChannels;
    uint32_t nSamplesPerSec;
    uint32_t nAvgBytesPerSec;
    uint16_t nBlockAlign;
    uint16_t wBitsPerSample;
    uint16_t cbSize;
}

struct FAudioWaveFormatExtensible
{
    FAudioWaveFormatEx Format;
    //union
    [StructLayout(LayoutKind.Explicit)]
    struct _Samples
    {
        [FieldOffset(0)] uint16_t wValidBitsPerSample;
        [FieldOffset(0)] uint16_t wSamplesPerBlock;
        [FieldOffset(0)] uint16_t wReserved;
    }
    _Samples Samples;
    uint32_t dwChannelMask;
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
    void* entry;
    LinkedList* next;
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
    FAudioBufferEntry* next;
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
    uint8_t version;
    uint8_t active;
    uint32_t refcount;
    uint32_t initFlags;
    uint32_t updateSize;
    FAudioMasteringVoice* master;
    LinkedList* sources;
    LinkedList* submixes;
    LinkedList* callbacks;
    FAudioMutex sourceLock;
    FAudioMutex submixLock;
    FAudioMutex callbackLock;
    FAudioMutex operationLock;
    FAudioWaveFormatExtensible mixFormat;

    /*FAudio_OPERATIONSET_Operation*/
    void* queuedOperations;
    /*FAudio_OPERATIONSET_Operation*/
    void* committedOperations;

    /* Used to prevent destroying an active voice */
    FAudioSourceVoice* processingSource;

    /* Temp storage for processing, interleaved PCM32F */
    // #define EXTRA_DECODE_PADDING 2
    uint32_t decodeSamples;
    uint32_t resampleSamples;
    uint32_t effectChainSamples;
    float* decodeCache;
    float* resampleCache;
    float* effectChainCache;

    /* Allocator callbacks */
    /*FAudioMallocFunc*/
    void* pMalloc;
    /*FAudioFreeFunc*/
    void* pFree;
    /*FAudioReallocFunc*/
    void* pRealloc;

    /* EngineProcedureEXT */
    void* clientEngineUser;
    /*FAudioEngineProcedureEXT*/
    void* pClientEngineProc;

    // #ifndef FAUDIO_DISABLE_DEBUGCONFIGURATION
    /* Debug Information */
    // FAudioDebugConfiguration debug;
    // #endif /* FAUDIO_DISABLE_DEBUGCONFIGURATION */

    /* Platform opaque pointer */
    void* platform;
};

unsafe struct FAudioVoice
{
    FAudio* audio;
    uint32_t flags;
    FAudioVoiceType type;

    FAudioVoiceSends sends;
    float** sendCoefficients;
    float** mixCoefficients;
    FAudioMixCallback* sendMix;
    /*FAudioFilterParametersEXT*/
    void* sendFilter;
    FAudioFilterState** sendFilterState;
    struct _effects
    {
        FAPOBufferFlags state;
        uint32_t count;
        FAudioEffectDescriptor* desc;
        void** parameters;
        uint32_t* parameterSizes;
        uint8_t* parameterUpdates;
        uint8_t* inPlaceProcessing;
    }
    _effects effects;
    FAudioFilterParametersEXT filter;
    FAudioFilterState* filterState;
    FAudioMutex sendLock;
    FAudioMutex effectLock;
    FAudioMutex filterLock;

    float volume;
    float* channelVolume;
    uint32_t outputChannels;
    FAudioMutex volumeLock;

    //FAUDIONAMELESS union
    [StructLayout(LayoutKind.Explicit)]
    struct NAMELESSUNION0
    {
        struct _src
        {
            /* Sample storage */
            uint32_t decodeSamples;
            uint32_t resampleSamples;

            /* Resampler */
            float resampleFreq;
            uint64_t resampleStep;
            uint64_t resampleOffset;
            uint64_t curBufferOffsetDec;
            uint32_t curBufferOffset;

            /* WMA decoding */
            //#ifdef HAVE_WMADEC
            /*FAudioWMADEC*/
            void* wmadec;
            //#endif /* HAVE_WMADEC*/

            /* Read-only */
            float maxFreqRatio;
            FAudioWaveFormatEx* format;
            FAudioDecodeCallback decode;
            FAudioResampleCallback resample;
            /*FAudioVoiceCallback*/
            void* callback;

            /* Dynamic */
            uint8_t active;
            float freqRatio;
            uint8_t newBuffer;
            uint64_t totalSamples;
            FAudioBufferEntry* bufferList;
            FAudioBufferEntry* flushList;
            FAudioMutex bufferLock;
        }
        [FieldOffset(0)] _src src;
        struct _mix
        {
            /* Sample storage */
            uint32_t inputSamples;
            uint32_t outputSamples;
            float* inputCache;
            uint64_t resampleStep;
            FAudioResampleCallback resample;

            /* Read-only */
            uint32_t inputChannels;
            uint32_t inputSampleRate;
            uint32_t processingStage;
        }
        [FieldOffset(0)] _mix mix;
        struct _master
        {
            /* Output stream, allocated by Platform */
            float* output;

            /* Needed when inputChannels != outputChannels */
            float* effectCache;

            /* Read-only */
            uint32_t inputChannels;
            uint32_t inputSampleRate;
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
    uint8_t instanceLimit;
    uint16_t fadeInMS;
    uint16_t fadeOutMS;
    uint8_t maxInstanceBehavior;
    int16_t parentCategory;
    float volume;
    uint8_t visibility;

    uint8_t instanceCount;
    float currentVolume;
}

unsafe struct FACTVariable
{
    uint8_t accessibility;
    float initialValue;
    float minValue;
    float maxValue;
}

unsafe struct FACTRPCPoint
{
    float x;
    float y;
    uint8_t type;
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
    uint16_t variable;
    uint8_t pointCount;
    uint16_t parameter;
    FACTRPCPoint* points;
}

unsafe struct FACTDSPParameter
{
    uint8_t type;
    float value;
    float minVal;
    float maxVal;
    uint16_t unknown;
}

unsafe struct FACTDSPPreset
{
    uint8_t accessibility;
    uint16_t parameterCount;
    FACTDSPParameter* parameters;
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
    uint16_t type;
    uint16_t timestamp;
    uint16_t randomOffset;
    //FAUDIONAMELESS union
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct NAMELESSUNION0
    {
        /* Play Wave Event */
        unsafe struct _wave
        {
            uint8_t flags;
            uint8_t loopCount;
            uint16_t position;
            uint16_t angle;

            /* Track Variation */
            uint8_t isComplex;
            //FAUDIONAMELESS union
            [StructLayout(LayoutKind.Explicit)]
            unsafe struct NAMELESSUNION1
            {
                unsafe struct _simple
                {
                    uint16_t track;
                    uint8_t wavebank;
                }
                [FieldOffset(0)]_simple simple;
                unsafe struct _complex
                {
                    uint16_t variation;
                    uint16_t trackCount;
                    uint16_t* tracks;
                    uint8_t* wavebanks;
                    uint8_t* weights;
                }
                [FieldOffset(0)]_complex complex;
            };

            /* Effect Variation */
            int16_t minPitch;
            int16_t maxPitch;
            float minVolume;
            float maxVolume;
            float minFrequency;
            float maxFrequency;
            float minQFactor;
            float maxQFactor;
            uint16_t variationFlags;
        }
        [FieldOffset(0)]_wave wave;
        /* Set Pitch/Volume Event */
        unsafe struct _value
        {
            uint8_t settings;
            uint16_t repeats;
            uint16_t frequency;
            //FAUDIONAMELESS union
            [StructLayout(LayoutKind.Explicit)]
            unsafe struct NAMELESSUNION3
            {
                unsafe struct _ramp
                {
                    float initialValue;
                    float initialSlope;
                    float slopeDelta;
                    uint16_t duration;
                }
                [FieldOffset(0)]_ramp ramp;
                unsafe struct _equation
                {
                    uint8_t flags;
                    float value1;
                    float value2;
                }
                [FieldOffset(0)] _equation equation;
            };
        }
        [FieldOffset(0)] _value value;
        /* Stop Event */
        unsafe struct _stop
        {
            uint8_t flags;
        }
        [FieldOffset(0)] _stop stop;
        /* Marker Event */
        unsafe struct _marker
        {
            uint32_t marker;
            uint16_t repeats;
            uint16_t frequency;
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
    uint8_t flags;
    uint32_t sbCode;
    uint32_t transitionOffset;
    uint8_t instanceLimit;
    uint16_t fadeInMS;
    uint16_t fadeOutMS;
    uint8_t maxInstanceBehavior;
    uint8_t instanceCount;
}

unsafe struct FACTVariation
{
    //FAUDIONAMELESS union
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct NAMELESSUNION0
    {
        unsafe struct _simple
        {
            uint16_t track;
            uint8_t wavebank;
        }
        [FieldOffset(0)] _simple simple;
        [FieldOffset(0)] uint32_t soundCode;
    }
    NAMELESSUNION0 union;
    float minWeight;
    float maxWeight;
    uint32_t linger;
}

unsafe struct FACTVariationTable
{
    uint8_t flags;
    int16_t variable;
    uint8_t isComplex;

    uint16_t entryCount;
    FACTVariation* entries;
}

unsafe struct FACTTransition
{
    int32_t soundCode;
    uint32_t srcMarkerMin;
    uint32_t srcMarkerMax;
    uint32_t dstMarkerMin;
    uint32_t dstMarkerMax;
    uint16_t fadeIn;
    uint16_t fadeOut;
    uint16_t flags;
}

unsafe struct FACTTransitionTable
{
    uint32_t entryCount;
    FACTTransition* entries;
}

/* Internal WaveBank Types */

unsafe struct FACTSeekTable
{
    uint32_t entryCount;
    uint32_t* entries;
}

/* Internal Cue Types */

unsafe struct FACTInstanceRPCData
{
    float rpcVolume;
    float rpcPitch;
    float rpcReverbSend;
    float rpcFilterFreq;
    float rpcFilterQFactor;
}

unsafe struct FACTEventInstance
{
    uint32_t timestamp;
    uint16_t loopCount;
    uint8_t finished;
    //FAUDIONAMELESS union
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct NAMELESSUNION0
    {
        [FieldOffset(0)] float value;
        [FieldOffset(0)] uint32_t valuei;
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
    void* callback;
    FACTWave* wave;
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
    FACTAudioEngine* parentEngine;
    LinkedList* waveList;
    FAudioMutex waveLock;
    uint8_t notifyOnDestroy;
    void* usercontext;

    /* Actual WaveBank information */
    char* name;
    uint32_t entryCount;
    /*FACTWaveBankEntry*/
    void* entries;
    uint32_t* entryRefs;
    FACTSeekTable* seekTables;
    char* waveBankNames;

    /* I/O information */
    uint32_t packetSize;
    uint16_t streaming;
    uint8_t* packetBuffer;
    uint32_t packetBufferLen;
    void* io;
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

