using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class PermafrostTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.PermafrostTile;

		public override void Load()
		{
			LoadTile(
				"IceTempleShingles",
				"Aurora Temple Shingles",
				new TileLoadData(
					minPick: 55,
					dustType: DustID.BorealWood,
					hitSound: SoundID.Tink,
					mapColor: new Color(84, 70, 73),
					stone: true
					)
				);

			LoadTile(
				"IceTempleBricks",
				"Coldstone Bricks",
				new TileLoadData(
					minPick: 55,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(50, 63, 71),
					stone: true
					)
				);

			LoadTile(
				"IceTempleCobbles",
				"Coldstone Cobbles",
				new TileLoadData(
					minPick: 55,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(50, 63, 71),
					stone: true
					)
				);

			LoadTile(
				"IceTempleStucco",
				"Frozen Stucco",
				new TileLoadData(
					minPick: 55,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(190, 190, 176),
					stone: true
					)
				);

			LoadTile(
				"IceTempleStones",
				"Frozen Stones",
				new TileLoadData(
					minPick: 55,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(163, 171, 173),
					stone: true
					)
				);
		}

		public override void AddRecipes()
		{
			var recipe = Recipe.Create(Mod.Find<ModItem>("IceTempleShingles").Type, 200);
			recipe.AddIngredient(ItemID.BorealWood, 200);
			recipe.AddIngredient(ModContent.ItemType<AuroraIceBar>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Mod.Find<ModItem>("IceTempleCobbles").Type, 200);
			recipe.AddIngredient(ItemID.StoneBlock, 200);
			recipe.AddIngredient(ModContent.ItemType<AuroraIceBar>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Mod.Find<ModItem>("IceTempleBricks").Type, 200);
			recipe.AddIngredient(ItemID.GrayBrick, 200);
			recipe.AddIngredient(ModContent.ItemType<AuroraIceBar>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Mod.Find<ModItem>("IceTempleStucco").Type, 200);
			recipe.AddIngredient(ItemID.SnowBlock, 200);
			recipe.AddIngredient(ModContent.ItemType<AuroraIceBar>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Mod.Find<ModItem>("IceTempleStones").Type, 200);
			recipe.AddIngredient(ItemID.StoneBlock, 200);
			recipe.AddIngredient(ModContent.ItemType<AuroraIceBar>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}