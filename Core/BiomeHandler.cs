using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public class BiomeHandler : ModPlayer
    {
        public override Texture2D GetMapBackgroundImage()
        {
            if (Player.InModBiome(GetInstance<OvergrowBiome>()))
                return Request<Texture2D>(AssetDirectory.MapBackgrounds + "OvergrowMap").Value;
            else if (Player.InModBiome(GetInstance<VitricDesertBiome>()))
                return Request<Texture2D>(AssetDirectory.MapBackgrounds + "GlassMap").Value;

            return null;
        }
    }
}

namespace StarlightRiver
{
	public partial class StarlightRiver : Mod
    {
        private int AddExpansion()
        {
            return (int)Math.Floor(((Main.screenPosition.X + (Main.screenWidth * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.X + Main.screenWidth) / 16f) + 2));
        }

        private int AddExpansionY()
        {
            return (int)Math.Floor(((Main.screenPosition.Y + (Main.screenHeight * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2));
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) //PORTTODO: Figure out what the heck these expansion things are for? Were they for zoom?
        {
            TargetHost.Maps?.OrderedShaderPass();
            TargetHost.Maps?.OrderedRenderPassBatched(Main.spriteBatch, Main.graphics.GraphicsDevice);

            if(StarlightWorld.spaceEventFade > 0) //PORTTODO: Move this to a ModSceneEffect
			{
                tileColor = Color.Lerp(Color.White, new Color(70, 60, 120), StarlightWorld.spaceEventFade);
                backgroundColor = Color.Lerp(Color.White, new Color(17, 15, 30), StarlightWorld.spaceEventFade);
            }
        }
    }
}