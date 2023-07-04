using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class JungleSalad : BonusIngredient
	{
		public JungleSalad() : base("Grasses and plants have a low chance to drop coins and other pot loot when broken") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<MahoganyRoot>(),
			ModContent.ItemType<HoneySyrup>(),
			ModContent.ItemType<DicedMushrooms>(),
			ModContent.ItemType<Vinegar>()
			);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			JungleSaladStatePlayer modplayer = Player.GetModPlayer<JungleSaladStatePlayer>();

			modplayer.Active = JungleSaladGlobalTile.AnyActive = true;
			modplayer.Multiplier = multiplier;
		}
	}

	public class JungleSaladStatePlayer : ModPlayer
	{
		public bool Active = false;

		public float Multiplier;

		public override void ResetEffects()//runs after tile breaking
		{
			Active = JungleSaladGlobalTile.AnyActive = false;
		}
	}

	//public class loaderasd: ModSystem
	//{
	//	public override void Load()
	//	{
	//		On_Player.PickTile += asd;
	//	}

	//	private void asd(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
	//	{
	//		orig(self, x, y, pickPower);
	//	}
	//}

	public class JungleSaladGlobalTile : GlobalTile
	{ 
		public static bool AnyActive = false;//faster to check a static bool than to find the mod player 

		public static HashSet<int> ValidTiles;

		public override void Load()
		{
			ValidTiles = new HashSet<int>//incomplete
			{
				TileID.Plants,
				TileID.Plants2,
				TileID.HallowedPlants,
				TileID.HallowedPlants2,
				TileID.CorruptPlants,
				TileID.CrimsonPlants,
				TileID.JunglePlants,
				TileID.JunglePlants2,
				TileID.AshPlants,
				TileID.OasisPlants,
				TileID.MushroomPlants,

				TileID.LilyPad,
				TileID.Cattail,
				TileID.BloomingHerbs,

				TileID.PlantDetritus,
				TileID.PlantDetritus2x2Echo,
				TileID.PlantDetritus3x2Echo,

				TileID.PottedPlants1,
				TileID.PottedPlants2,
				TileID.PottedLavaPlants,
				TileID.PottedLavaPlantTendrils,
				TileID.PottedCrystalPlants,

				TileID.Vines,
				TileID.HallowedVines,
				TileID.CorruptVines,
				TileID.CrimsonVines,
				TileID.AshVines,
				TileID.JungleVines,
				TileID.MushroomVines,

				TileID.CorruptThorns,
				TileID.CrimsonThorns,
				TileID.JungleThorns,
				TileID.PlanteraThorns,

				TileID.TreeAmber,
				TileID.TreeAmethyst,
				TileID.TreeAsh,
				TileID.TreeDiamond,
				TileID.TreeEmerald,
				TileID.TreeRuby,
				TileID.TreeSapphire,
				TileID.TreeTopaz,
				TileID.MushroomTrees,
				TileID.PalmTree,
				TileID.PineTree,
				TileID.VanityTreeSakura,
				TileID.VanityTreeSakuraSaplings,
				TileID.VanityTreeWillowSaplings,
				TileID.VanityTreeYellowWillow,

				TileID.Cactus,
			};
		}

		public override void Unload()
		{
			ValidTiles = null;
		}

		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			//as far as I know, kill tile is is best place to check.
			//PickTile has access to the causing player, but does not get run on projectiles/swinging weapons.
			if (AnyActive)
			{
				if (ValidTiles.Contains(type))//this should be faster than checking the player, moreso in MP
				{
					JungleSaladStatePlayer modplayer;

					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						modplayer = Main.LocalPlayer.GetModPlayer<JungleSaladStatePlayer>();

						if (!modplayer.Active)//local player is inactive
							return;
					}
					else
					{
						int index = NearestPlayerWithBuff(i, j);//todo: give distance as an out

						if (index == -1)//no player is active
							return;

						modplayer = Main.player[index].GetModPlayer<JungleSaladStatePlayer>();
					}

					const int maxDistance = 1600;//100 tiles

					if (Main.rand.NextFloat(100f) < (3f * modplayer.Multiplier) && modplayer.Player.Distance(new Vector2(i, j) * 16) < maxDistance)
					{
						DropCustomPotLoot(new Vector2(i * 16, j * 16));
					}
				}
			}

			//base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
		}

		private static void DropCustomPotLoot(Vector2 position)
		{

		}

		private static int NearestPlayerWithBuff(int i, int j)
		{
			int closestIndex = -1;
			float lastDistance = 1000000;
			Vector2 playerCoords = new Vector2(i * 16, j * 16);
			foreach (Player player in Main.player)
			{
				if (!player.active || player.DeadOrGhost)
					continue;

				JungleSaladStatePlayer modplayer = player.GetModPlayer<JungleSaladStatePlayer>();

				if (!modplayer.Active)
					continue;

				float distance = player.DistanceSQ(playerCoords);
				if (distance < lastDistance)
				{
					lastDistance = distance;
					closestIndex = player.whoAmI;
				}
			}

			return closestIndex;
		}
	}
}