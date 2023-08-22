using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;
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
	{//so that each player has a active bool and mult associated with them that can be checked when getting the nearest player
		public bool Active = false;

		public float Multiplier;

		public override void ResetEffects()//runs after tile breaking
		{
			Active = JungleSaladGlobalTile.AnyActive = false;
		}
	}

	public class JungleSaladGlobalTile : GlobalTile
	{
		public static bool AnyActive = false;//faster to check a static bool than to find the mod player 

		public static HashSet<int> ValidTiles;

		public override void Load()
		{
			ValidTiles = new HashSet<int>
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
			if (Main.netMode == NetmodeID.MultiplayerClient)//test this in MP, only spawns items so should be fine? dusts may break but if thats the case its not worth having dust on item spawn.
				return;
			//as far as I know, kill tile is is best place to check.
			//PickTile has access to the causing player, but does not get run on projectiles/swinging weapons.
			if (AnyActive)
			{
				if (ValidTiles.Contains(type))//this should be faster than checking the player, moreso in MP
				{
					JungleSaladStatePlayer modplayer;

					const int maxDistance = 1600;//100 tiles

					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						modplayer = Main.LocalPlayer.GetModPlayer<JungleSaladStatePlayer>();

						//local player is inactive or outside range
						if (!modplayer.Active || modplayer.Player.Distance(new Vector2(i, j) * 16) > maxDistance)
							return;
					}
					else
					{
						int index = NearestPlayerWithBuff(i, j, out float distance);//todo: give distance as an out

						//no player is active or closest player is out or range
						if (index == -1 || distance > maxDistance)
							return;

						modplayer = Main.player[index].GetModPlayer<JungleSaladStatePlayer>();
					}

					if (Main.rand.NextFloat(100f) < (3.5f * modplayer.Multiplier))
					{
						DropCustomPotLoot(modplayer, i, j);

						for (int g = 0; g < 5; g++)
							Dust.NewDustPerfect(new Vector2(i * 16, j * 16), DustID.GoldCoin, new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.35f, 0.1f)));
					}
				}
			}
		}

		private static void DropCustomPotLoot(ModPlayer modPlayer, int i, int j)// :)
		{//logic copied from vanilla wiki
			Player player = modPlayer.Player;

			//this does not add potions and coin portals, for both complexity and balance reasons

			if (Main.netMode != NetmodeID.SinglePlayer && Main.rand.NextBool(30))
			{
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.WormholePotion);
				return;
			}

			int dropType = Main.rand.Next(0, 7);//may need to be weighted
			switch (dropType)
			{
				case 0://health
					{
						if (player.statLife < player.statLifeMax2)
						{//does not take the extra 2 chances from expert mode into account
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Heart);

							if (Main.rand.NextBool())//might be unbalanced for grass loot
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Heart);
						}
						else
						{
							goto case 1;//go to torches
						}
					}

					break;

				case 1://torches
					{
						if (!PlayerHasTorches(player))
						{//item amounts are only based on normal mode drop chances
							if (Main.tile[i, j].LiquidAmount > 0)//replace torches with glowsticks if in water
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, player.ZoneSnow ? ItemID.StickyGlowstick : ItemID.Glowstick, Main.rand.Next(4, 13));
							else if (player.ZoneHallow)
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.HallowedTorch, Main.rand.Next(4, 13));
							else if (player.ZoneCorrupt)
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.CorruptTorch, Main.rand.Next(4, 13));
							else if (player.ZoneCrimson)
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.CrimsonTorch, Main.rand.Next(4, 13));
							else if (player.ZoneJungle)//this one may be inconsistent with vanilla pot drop rules, ice and desert also share this problem but are rare cases unlike this one
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.JungleTorch, Main.rand.Next(4, 13));
							else if (player.ZoneSnow)
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.IceTorch, Main.rand.Next(2, 6));
							else if (player.ZoneDesert)
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.DesertTorch, Main.rand.Next(4, 13));
							else
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Torch, Main.rand.Next(4, 13));
						}
						else
						{
							goto case 6;//go to money
						}
					}

					break;

				case 2://ammo
					{//wiki says grenades can spawn but source code says that is impossible due to an oversight
						if (player.ZoneUnderworldHeight)
						{
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.HellfireArrow, Main.rand.Next(10, 21));
						}
						else if (Main.hardMode)
						{
							if (Main.rand.NextBool() && (player.ZonePurity || player.ZoneDirtLayerHeight || player.ZoneUnderworldHeight))
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, (WorldGen.SavedOreTiers.Silver != 168) ? ItemID.SilverBullet : ItemID.TungstenBullet, Main.rand.Next(7, 17));//est based on drop tests
							else
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.UnholyArrow, Main.rand.Next(10, 21));
						}
						else
						{
							if (Main.rand.NextBool() && (player.ZonePurity || player.ZoneDirtLayerHeight))
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Shuriken, Main.rand.Next(12, 25));//est based on drop tests
							else
								Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.WoodenArrow, Main.rand.Next(10, 21));
						}
					}

					break;

				case 3://healing potions
					{//ignores expert extra drop chance
						if (!Main.hardMode)
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.LesserHealingPotion);
						else
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.HealingPotion);
					}

					break;

				case 4://bombs
					{//lower drop amounts than vanilla
						if (player.ZoneUndergroundDesert)
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.ScarabBomb, Main.rand.Next(1, 4));
						else if (player.position.Y > (Main.worldSurface * 16))
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Bomb, Main.rand.Next(1, 4));
						else
							goto case 5;//go to ropes
					}

					break;

				case 5://ropes
					{
						if (!Main.hardMode && !player.ZoneUnderworldHeight)
							Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Rope, Main.rand.Next(20, 41));
						else
							goto case 6;//go to money
					}

					break;

				case 6://money
					{
						SpawnCoins(i, j);//basically uses a copy of the vanilla coin method since its overly complicated
					}

					break;
			}
		}

		private static void SpawnCoins(int i, int j)// :|
		{//modified vanilla method
			const float moneymult = 5f;//originally a money multiplier based on pot type, however this should not take into account the biome for this
			float moneyvalue = 200 + Main.rand.Next(-100, 101);//2 silver +/- 1

			//if ((double)j < Main.worldSurface)//mult based on height prob not needed, kept uncase its decided this is needed
			//	moneyvalue *= 0.5f;
			//else if ((double)j < Main.rockLayer)
			//	moneyvalue *= 0.75f;
			//else if (j > Main.maxTilesY - 250)
			//	moneyvalue *= 1.25f;

			moneyvalue *= moneymult;

			if (Main.expertMode)
				moneyvalue *= 1.75f;//vanilla is 2.5

			moneyvalue *= Main.rand.NextFloat(0.8f, 1.2f);

			int prev = 10;
			for (int g = 1; g < 6; g++)//1-5
			{
				int max = g == 5 ? 100 : prev;//10, 20, 40, 80, 100

				if (Main.rand.NextBool(g * 4))
					moneyvalue *= 1f + (float)Main.rand.Next(max / 2, max + 1) * 0.01f;

				prev *= 2;
			}

			if (Main.expertMode)
			{
				for (int g = 1; g < 4; g++)//1-3
				{
					if (Main.rand.NextBool(g + 1))
						moneyvalue *= 1f + 0.25f * g;
				}
			}

			//prehardmode
			if (NPC.downedBoss1)//EoC
				moneyvalue *= 1.1f;

			if (NPC.downedBoss2)//EoW/BoC
				moneyvalue *= 1.1f;

			if (NPC.downedBoss3)//Skeletron
				moneyvalue *= 1.1f;

			if (NPC.downedQueenBee)
				moneyvalue *= 1.1f;

			//hardmode
			if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)//combined
				moneyvalue *= 1.3f;

			if (NPC.downedPlantBoss)//amount increased to cover golem too
				moneyvalue *= 1.2f;

			//events
			if (NPC.downedGoblins)
				moneyvalue *= 1.1f;

			if (NPC.downedPirates)
				moneyvalue *= 1.1f;

			if (NPC.downedFrost)//unsure if this is frostmoon or frost legion...
				moneyvalue *= 1.1f;

			while ((int)moneyvalue > 0)//spawns the coins
			{
				int coinValue = 1;
				int dropItemID = ItemID.CopperCoin;

				if (moneyvalue > 1000000f)
				{
					coinValue = 1000000;
					dropItemID = ItemID.PlatinumCoin;
				}
				else if (moneyvalue > 10000f)
				{
					coinValue = 10000;
					dropItemID = ItemID.GoldCoin;
				}
				else if (moneyvalue > 100f)
				{
					coinValue = 100;
					dropItemID = ItemID.SilverCoin;
				}

				int dropAmount = (int)moneyvalue / coinValue;

				if (dropAmount > 50 && Main.rand.NextBool(2))
					dropAmount /= Main.rand.Next(3) + 1;

				if (Main.rand.NextBool(2))
					dropAmount /= Main.rand.Next(coinValue > 1 ? 3 : 4) + 1;

				if (dropAmount < 1)//&& coinValue == 1)//only copper coins have this extra check, but I dont see a reason to only have for one
					dropAmount = 1;

				moneyvalue -= (float)(coinValue * dropAmount);
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, dropItemID, dropAmount);
			}
		}

		private static bool PlayerHasTorches(Player player)
		{
			int torchCount = 0;
			const int minTorches = 20;
			for (int num5 = 0; num5 < 50; num5++)
			{
				Item item = player.inventory[num5];
				if (!item.IsAir && item.createTile == TileID.Torches)
				{
					torchCount += item.stack;
					if (torchCount >= minTorches)
					{
						break;
					}
				}
			}

			return torchCount < minTorches;
		}

		private static int NearestPlayerWithBuff(int i, int j, out float distance)
		{
			int closestIndex = -1;
			float lastDistance = 1000000;
			Vector2 playerCoords = new Vector2(i * 16, j * 16);
			foreach (Player player in Main.player)
			{
				if (!player.active || player.DeadOrGhost || player.flowerBoots)
					continue;

				JungleSaladStatePlayer modplayer = player.GetModPlayer<JungleSaladStatePlayer>();

				if (!modplayer.Active)
					continue;

				float thisdistance = player.DistanceSQ(playerCoords);
				if (thisdistance < lastDistance)
				{
					lastDistance = thisdistance;
					closestIndex = player.whoAmI;
				}
			}

			distance = (float)Math.Sqrt(lastDistance);
			return closestIndex;
		}
	}
}