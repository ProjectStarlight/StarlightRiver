using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class VitricTempleWall : ModWall
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricTempleWall";

        public override void SetStaticDefaults() => QuickBlock.QuickSetWall(this, DustType<Dusts.Sand>(), SoundID.Dig, ItemType<VitricTempleWallItem>(), true, new Color(54, 48, 42));
    }

    class VitricTempleWallItem : QuickWallItem
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallItem";

        public VitricTempleWallItem() : base("Vitric Forge Brick Wall", "Sturdy", WallType<VitricTempleWall>(), ItemRarityID.White) { }
    }
}
