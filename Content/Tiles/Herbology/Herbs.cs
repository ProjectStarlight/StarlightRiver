using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Herbology
{
    internal abstract class HangingPlant : ModTile
    {
        private readonly string ItemDrop;

        public HangingPlant(string drop)
        {
            ItemDrop = drop;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                mod.TileType(Type.ToString()),
                TileType<Planter>()
            };
            TileObjectData.addTile(Type);
            drop = mod.ItemType(ItemDrop);
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j + 1].active() == false)
                WorldGen.PlaceTile(i, j + 1, mod.TileType(Type.ToString()), true);
        }
    }

    internal abstract class TallPlant : ModTile
    {
        private readonly string ItemDrop;

        public TallPlant(string drop)
        {
            ItemDrop = drop;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
                TileType<Soil>()
            };
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                mod.TileType(Type.ToString())
            };
            TileObjectData.addTile(Type);
            drop = mod.ItemType(ItemDrop);
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j - 1].active() == false)
                WorldGen.PlaceTile(i, j - 1, mod.TileType(Type.ToString()));
        }
    }
}