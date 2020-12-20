using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class AncientSandstonePlatform : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileBlockLight[Type] = false;
            minPick = 200;
            AddMapEntry(new Color(150, 105, 65));
        }

    }

    internal class AncientSandstonePlatformItem : QuickTileItem { public AncientSandstonePlatformItem() : base("Ancient Sandstone Platform", "", TileType<AncientSandstonePlatform>(), 0) { } }

    internal class AncientSandstoneWall : ModWall
    {
        public override void SetDefaults() => (this).QuickSetWall(DustID.Copper, SoundID.Dig, ItemType<AncientSandstoneWallItem>(), false, new Color(71, 46, 41));
    }

    internal class AncientSandstoneWallItem : QuickWallItem
    {
        public AncientSandstoneWallItem() : base("Ancient Sandstone Wall", "", WallType<AncientSandstoneWall>(), 0) { }
    }

    internal class AncientSandstonePillarWall : ModWall
    {
        public override void SetDefaults() => (this).QuickSetWall(DustID.Copper, SoundID.Dig, ItemType<AncientSandstonePillarWallItem>(), false, new Color(75, 48, 44));
    }

    internal class AncientSandstonePillarWallItem : QuickWallItem
    {
        public AncientSandstonePillarWallItem() : base("Ancient Sandstone Wall", "", WallType<AncientSandstonePillarWall>(), 0) { }
    }
}