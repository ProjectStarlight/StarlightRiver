﻿using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class UndergroundTempleLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.UndergroundTempleTile;

		public override void Load()
		{
			LoadTile(
				"TempleBrick",
				"Ancient Bricks",
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(150, 150, 150),
					stone: true
					)
				);
		}

		public override void AddRecipes()
		{
			var recipe = Recipe.Create(Mod.Find<ModItem>("TempleBrickItem").Type, 50);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
		}
	}
}