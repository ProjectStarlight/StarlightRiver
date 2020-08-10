using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Tiles.JungleBloody
{
    public class WallJungleBloody : ModWall
    {
        public override void SetDefaults()
        {
            Main.wallHouse[Type] = false;
            dustType = 14;
            drop = mod.ItemType("WallJungleCorruptItem");
            AddMapEntry(new Color(78, 20, 28));
        }
    }
}