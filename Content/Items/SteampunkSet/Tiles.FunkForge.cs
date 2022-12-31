using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BuriedArtifacts;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class FunkForgeItem : QuickTileItem
	{
		public FunkForgeItem() : base("Funk Forge", "Combined effects of anvil and hellforge \nReforges of crafted items are garaunteed to be positive", "FunkForgeTile", ItemRarityID.Green, AssetDirectory.SteampunkItem + "FunkForgeItem", true, Item.sellPrice(0, 75, 0, 0)) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Hellforge, 1);
			recipe.AddIngredient(ItemID.IronAnvil, 1);
			recipe.AddIngredient(ModContent.ItemType<CopperCogArtifactItem>(), 1);
			recipe.AddIngredient(ModContent.ItemType<SuspiciouslyStrangeBrewArtifactItem>(), 1);
			recipe.AddIngredient(ModContent.ItemType<ImportantScrewArtifactItem>(), 1);
			recipe.AddIngredient(ModContent.ItemType<InconspicuousPlatingArtifactItem>(), 1);

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.Hellforge, 1);
			recipe2.AddIngredient(ItemID.LeadAnvil, 1);
			recipe2.AddIngredient(ModContent.ItemType<CopperCogArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<SuspiciouslyStrangeBrewArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<ImportantScrewArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<InconspicuousPlatingArtifactItem>(), 1);
		}
	}
	public class FunkForgeTile : ModTile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16};
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type);
			AdjTiles = new int[] { TileID.Anvils, TileID.Hellforge};
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Funk Forge");
			AddMapEntry(new Color(200, 200, 200), name);
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int x, int y, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 32, 16, ModContent.ItemType<FunkForgeItem>());
		}
	}

	public class FunkForgeModifiers : GlobalItem
	{
		public static int[] badReforges = new int[] { PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy, PrefixID.Weak, PrefixID.Ruthless, PrefixID.Slow, PrefixID.Sluggish, PrefixID.Lazy, PrefixID.Annoying, PrefixID.Tiny, PrefixID.Terrible, PrefixID.Small, PrefixID.Dull, PrefixID.Unhappy, PrefixID.Bulky, PrefixID.Shameful, PrefixID.Heavy, PrefixID.Awful, PrefixID.Lethargic, PrefixID.Awkward, PrefixID.Powerful, PrefixID.Frenzying,PrefixID.Inept, PrefixID.Ignorant, PrefixID.Deranged, PrefixID.Deranged, PrefixID.Intense, PrefixID.Taboo, PrefixID.Manic, 0};

		public static int tries = 0;

		public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
		{
			//if (Main.reforgeItem == item || pre != -1)
				return base.PrefixChance(item, pre, rand);

			Player player = Main.LocalPlayer;
			int x = (int)(player.Center.X / 16);
			int y = (int)(player.Center.Y / 16);
			for (int i = -6; i < 6; i++)
				for (int j = -6; j < 6; j++)
				{
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<FunkForgeTile>())
					{
						return true;
					}
				}

			return base.PrefixChance(item, pre, rand);
		}

		public override int ChoosePrefix(Item item, UnifiedRandom rand)
		{
			Player player = Main.LocalPlayer;
			int x = (int)(player.Center.X / 16);
			int y = (int)(player.Center.Y / 16);
			for (int i = -6; i < 6; i++)
				for (int j = -6; j < 6; j++)
				{
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<FunkForgeTile>())
					{
						return -3;
					}
				}
			return base.ChoosePrefix(item, rand);
		}


		public override bool AllowPrefix(Item item, int pre)
		{
			if (Main.reforgeItem == item)
				return true;

			tries++;
			Player player = Main.LocalPlayer;
			int x = (int)(player.Center.X / 16);
			int y = (int)(player.Center.Y / 16);
			for (int i = -6; i < 6; i++)
				for (int j = -6; j < 6; j++)
				{
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<FunkForgeTile>())
					{
						Main.NewText(pre);
						if (tries > 999)
						{
							tries = 0;
							return true;
						}

						if (badReforges.Contains(pre))
						{
							return false;
						}
						else
						{
							tries = 0;
							return true;
						}
					}
				}
			tries = 0;
			return true;
		}
	}
}