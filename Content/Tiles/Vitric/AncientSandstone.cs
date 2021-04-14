using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class AncientSandstonePlatform : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileLighted[Type] = true; //im fucking lazy
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.Platforms[Type] = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 27;
            TileObjectData.newTile.StyleWrapLimit = 27;
            TileObjectData.newTile.UsesCustomCanPlace = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
            AddMapEntry(new Color(200, 200, 200));
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Platforms };
        }
    }

    internal class AncientSandstonePlatformItem : QuickTileItem { public AncientSandstonePlatformItem() : base("Ancient Sandstone Platform", "", TileType<AncientSandstonePlatform>(), 0, AssetDirectory.VitricTile) { } }

    internal class AncientSandstoneWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => (this).QuickSetWall(DustID.Copper, SoundID.Dig, ItemType<AncientSandstoneWallItem>(), false, new Color(71, 46, 41));
    }

    internal class AncientSandstoneWallItem : QuickWallItem
    {
        public AncientSandstoneWallItem() : base("Ancient Sandstone Wall", "", WallType<AncientSandstoneWall>(), 0, AssetDirectory.VitricTile) { }
    }

    internal class AncientSandstonePillarWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => (this).QuickSetWall(DustID.Copper, SoundID.Dig, ItemType<AncientSandstonePillarWallItem>(), false, new Color(75, 48, 44));
    }

    internal class AncientSandstonePillarWallItem : QuickWallItem
    {
        public AncientSandstonePillarWallItem() : base("Ancient Sandstone Wall", "", WallType<AncientSandstonePillarWall>(), 0, AssetDirectory.VitricTile) { }
    }
}