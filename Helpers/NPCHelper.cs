using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class NPCHelper
	{
		/// <summary>
		/// determines if an NPC is "fleshy" based on it's hit sound
		/// </summary>
		/// <param name="NPC"></param>
		/// <returns></returns>
		public static bool IsFleshy(NPC NPC)
		{
			return !
				(
					NPC.HitSound == SoundID.NPCHit2 ||
					NPC.HitSound == SoundID.NPCHit3 ||
					NPC.HitSound == SoundID.NPCHit4 ||
					NPC.HitSound == SoundID.NPCHit41 ||
					NPC.HitSound == SoundID.NPCHit42 ||
					NPC.HitSound == new SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact")
				);
		}
	}
}
