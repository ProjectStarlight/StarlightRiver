using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class SoundHelper
	{
		/// <summary>
		/// Plays a sound based on it's path in the mod's sound directory with a pitch and volume applied
		/// </summary>
		/// <param name="path"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static SlotId PlayPitched(string path, float volume, float pitch, Vector2? position = null)
		{
			if (Main.netMode == NetmodeID.Server)
				return SlotId.Invalid;

			var style = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/{path}")
			{
				Volume = volume,
				Pitch = pitch,
				MaxInstances = 0
			};

			return SoundEngine.PlaySound(style, position);
		}

		/// <summary>
		/// Plays a given vanilla SoundStyle with a pitch and volume applied
		/// </summary>
		/// <param name="style"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static SlotId PlayPitched(SoundStyle style, float volume, float pitch, Vector2? position = null)
		{
			if (Main.netMode == NetmodeID.Server)
				return SlotId.Invalid;

			style.Volume *= volume;
			style.Pitch += pitch;
			style.MaxInstances = 0;

			return SoundEngine.PlaySound(style, position);
		}
	}
}