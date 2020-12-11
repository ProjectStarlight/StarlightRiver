using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Void
{
    public class VoidWall : ModWall
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSetWall(this, DustType<Dusts.Darkness>(), SoundID.Dig, ItemType<VoidWallItem>(), true, new Color(10, 20, 15));
        }
    }
    public class VoidWallItem : QuickWallItem
    {
        public VoidWallItem() : base("Void Brick Wall", "", WallType<VoidWall>(), 0) { }
    }
}