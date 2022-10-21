using Microsoft.Xna.Framework;
using StarlightRiver.Core;
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

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.AnchorValidTiles = new int[] { Mod.Find<ModTile>("VitricSand").Type, Mod.Find<ModTile>("VitricSoftSand").Type, TileID.FossilOre,
				TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
				TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand };

			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.StyleHorizontal = true;
			this.QuickSetFurniture(2, 2, DustType<Dusts.GlassNoGravity>(), SoundID.Dig, false, new Color(80, 131, 142));
		}
	}

	public class VitricSmallCactus : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.AnchorValidTiles = new int[] { Mod.Find<ModTile>("VitricSand").Type, Mod.Find<ModTile>("VitricSoftSand").Type, TileID.FossilOre,
				TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
				TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand };

			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.StyleHorizontal = true;
			this.QuickSetFurniture(1, 1, DustType<Dusts.GlassNoGravity>(), SoundID.Dig, false, new Color(80, 131, 142));
		}
	}
}