using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleBloody
{
    public class WallJungleBloody : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleBloodyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = false;
            dustType = 14;
            drop = mod.ItemType("WallJungleCorruptItem");
            AddMapEntry(new Color(78, 20, 28));
        }
    }
}