using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.CustomHooks;
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

    public class FirstContactSystem : ModSystem
    {
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            TargetHost.Maps?.OrderedShaderPass();
            TargetHost.Maps?.OrderedRenderPassBatched(Main.spriteBatch, Main.graphics.GraphicsDevice);

            if (FirstContactEvent.spaceEventFade > 0) //PORTTODO: Move this to a ModSceneEffect
            {
                tileColor = Color.Lerp(Color.White, new Color(70, 60, 120), FirstContactEvent.spaceEventFade);
                backgroundColor = Color.Lerp(Color.White, new Color(17, 15, 30), FirstContactEvent.spaceEventFade);
            }
        }
    }
}
