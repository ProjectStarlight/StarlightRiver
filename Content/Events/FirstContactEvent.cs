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
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/WhipAndNaenae");

		public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override bool IsSceneEffectActive(Player player)
        {
			return FirstContactSystem.FirstContactActive;//this is here for now to stick with the old style
		}
    }

    public class FirstContactSystem : ModSystem
    {
        public static bool FirstContactActive = false;
        public static float FirstContactFade = 0;

        public override void PostUpdateEverything()
		{
            if (FirstContactActive)
            {
                if (FirstContactFade <= 1)
                    FirstContactFade += 0.01f;
            }
            else if (FirstContactFade > 0)
                FirstContactFade -= 0.01f;
        }

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            TargetHost.Maps?.OrderedShaderPass(); //PORTTODO: Why is this here?
            TargetHost.Maps?.OrderedRenderPassBatched(Main.spriteBatch, Main.graphics.GraphicsDevice);

            if (FirstContactFade > 0) //PORTTODO: Move this to a ModSceneEffect
            {
                tileColor = Color.Lerp(Color.White, new Color(70, 60, 120), FirstContactFade);
                backgroundColor = Color.Lerp(Color.White, new Color(17, 15, 30), FirstContactFade);
            }
        }
    }
}
