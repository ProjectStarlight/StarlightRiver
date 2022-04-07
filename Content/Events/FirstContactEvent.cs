using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Biomes
{
	public class FirstContactEvent : ModSceneEffect
	{
		//moved here from starlight world
		//TODO: figure out what do to with these
		public static bool spaceEventActive = false;
		public static float spaceEventFade = 0;

		public override int Music => MusicLoader.GetMusicSlot("Sounds/WhipAndNaenae");

		public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override bool IsSceneEffectActive(Player player)
        {
			return spaceEventActive;//this is here for now to stick with the old style
		}
    }
}
