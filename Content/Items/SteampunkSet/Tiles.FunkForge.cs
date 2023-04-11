//TODO on funk forge:
//Glowmask
//Animations
//Item sprite
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
using Terraria.Localization;

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
			recipe.Register();

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.Hellforge, 1);
			recipe2.AddIngredient(ItemID.LeadAnvil, 1);
			recipe2.AddIngredient(ModContent.ItemType<CopperCogArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<SuspiciouslyStrangeBrewArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<ImportantScrewArtifactItem>(), 1);
			recipe2.AddIngredient(ModContent.ItemType<InconspicuousPlatingArtifactItem>(), 1);
			recipe2.Register();
		}
	}
	public class FunkForgeTile : ModTile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
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
			LocalizedText name = CreateMapEntryName();
			name.SetDefault("Funk Forge");
			AddMapEntry(Color.Purple, name);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.4f;
			g = 0.1f;
			b = 0.6f;
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
		#region Code if bug is fixed
		//Uncomment when TML bug is fixed
		/*
		public static int[] badReforges = new int[] { PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy, PrefixID.Weak, PrefixID.Ruthless, PrefixID.Slow, PrefixID.Sluggish, PrefixID.Lazy, PrefixID.Annoying, PrefixID.Tiny, PrefixID.Terrible, PrefixID.Small, PrefixID.Dull, PrefixID.Unhappy, PrefixID.Bulky, PrefixID.Shameful, PrefixID.Heavy, PrefixID.Awful, PrefixID.Lethargic, PrefixID.Awkward, PrefixID.Powerful, PrefixID.Frenzying,PrefixID.Inept, PrefixID.Ignorant, PrefixID.Deranged, PrefixID.Deranged, PrefixID.Intense, PrefixID.Taboo, PrefixID.Manic, 0};

		public static int tries = 0;

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
		}*/

		#endregion

		public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
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
						return BestPrefix(item);
					}
				}
			return base.ChoosePrefix(item, rand);
		}

		private int BestPrefix(Item item)
		{
			if (item.axe > 0 || item.hammer > 0 || item.pick > 0)
				return PrefixID.Light;
			if (item.DamageType == DamageClass.Melee) //Can't be a switch statement because damage classes aren't constant
				return PrefixID.Legendary;
			if (item.DamageType == DamageClass.Magic || item.DamageType == DamageClass.MagicSummonHybrid || item.DamageType == DamageClass.Summon)
				return PrefixID.Mythical;
			if (item.DamageType == DamageClass.Ranged)
				return PrefixID.Unreal;
			if (item.DamageType == DamageClass.SummonMeleeSpeed)
				return PrefixID.Legendary;

			if (item.accessory)
				return Main.rand.NextBool() ? PrefixID.Warding : PrefixID.Menacing;

			return -3;
		}
	}
}