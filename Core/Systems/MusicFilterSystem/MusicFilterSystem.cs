using FAudioINTERNAL;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.Audio;
namespace StarlightRiver.Core.Systems.MusicFilterSystem
{
	internal class MusicFilterSystem : ModSystem
	{
		public static float globalPitchModifier = 0f;

		public static void ResetModifiers()
		{
			globalPitchModifier = 0f;
		}

		public unsafe override void PostUpdateEverything()
		{
			if (Main.audioSystem is LegacyAudioSystem audio)
			{
				IEnumerable<IAudioTrack> tracks = audio.AudioTracks.Where(n => n?.IsPlaying ?? false);
				foreach (IAudioTrack item in tracks)
				{
					if (item is ASoundEffectBasedAudioTrack)
					{
						item?.SetVariable("Pitch", globalPitchModifier);
					}
					else if (item is CueAudioTrack)
					{
						var cue = typeof(CueAudioTrack).GetField("_cue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(item) as Cue;
						nint handle = (IntPtr)typeof(Cue).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cue);

						var cuePtr = (FACTCue*)(void*)handle;

						if (cuePtr->simpleWave != null)
						{
							FAudioVoice* voice = cuePtr->simpleWave->voice;
						}
						else if (cuePtr->playingSound != null)
						{
							FACTSound* factSound = cuePtr->playingSound->sound;
							int count = factSound->trackCount;
							for (int i = 0; i < count; i++)
							{
								ref FACTSoundInstance* sound = ref cuePtr->playingSound;
								ref FACTTrackInstance trackz = ref sound->tracks[i];
								ref FACTTrackInstance._wave wave1 = ref trackz.activeWave;
								wave1.basePitch = (short)(globalPitchModifier * 1200);
							}
						}
					}
				}
			}

			ResetModifiers();
		}
	}
}