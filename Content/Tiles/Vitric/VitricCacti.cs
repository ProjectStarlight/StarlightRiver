using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	public class VitricRoundCactus : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            TileObjectData.newTile.DrawYOffset = 2;

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { Mod.TileType("VitricSand"), Mod.TileType("VitricSoftSand"), TileID.FossilOre,
                TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
                TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand };

            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            (this).QuickSetFurniture(2, 2, DustType<Dusts.GlassNoGravity>(), SoundID.Dig, false, new Color(80, 131, 142));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, Mod.ItemType("VitricCactusItem"), Main.rand.Next(2, 5));
    }

    public class VitricSmallCactus : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            TileObjectData.newTile.DrawYOffset = 2;

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { Mod.TileType("VitricSand"), Mod.TileType("VitricSoftSand"), TileID.FossilOre,
                TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
                TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand };

            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            (this).QuickSetFurniture(1, 1, DustType<Dusts.GlassNoGravity>(), SoundID.Dig, false, new Color(80, 131, 142));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, Mod.ItemType("VitricCactusItem"), 1);
    }

    public class VitricCactus : ModCactus
    {
        public override Texture2D GetTexture() => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "VitricCactus").Value;
    }
}