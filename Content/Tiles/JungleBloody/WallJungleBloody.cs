using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.JungleBloody
{
	public class WallJungleBloody : ModWall
    {
        public override string Texture => AssetDirectory.JungleBloodyTile + Name;

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = false;
            dustType = 14;
            drop = Mod.ItemType("WallJungleCorruptItem");
            AddMapEntry(new Color(78, 20, 28));
        }
    }
}