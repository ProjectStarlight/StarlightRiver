using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricRockItem : QuickTileItem 
    { 
        public VitricRockItem() : base("Vitric Rock", "", TileType<VitricRock>(), 0, AssetDirectory.VitricTile) { }
        //public override void OnConsumeItem(Player player) => Main.NewText(TileType<VitricRock>());
    }

    internal class VitricRock : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.RandomStyleRange = 6;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType("VitricSand"), mod.TileType("VitricSoftSand"), TileID.FossilOre,
                TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
                TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand };
            TileObjectData.addTile(Type);
            soundType = SoundID.Shatter;
            dustType = DustType<Dusts.GlassGravity>();
            AddMapEntry(new Color(80, 131, 142));
        }
    }
}