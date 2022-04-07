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
    }
}