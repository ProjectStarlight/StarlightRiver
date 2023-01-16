using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Biomes
{
	public class MapBackgroundHandler : ModPlayer
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
