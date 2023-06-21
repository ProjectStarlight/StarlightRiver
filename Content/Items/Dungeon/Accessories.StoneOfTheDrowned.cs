using ReLogic.Content;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Jungle;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class StoneOfTheDrowned : SmartAccessory
	{
		internal float slotsUsed; // used so you only summon the amount of slots just gained. Otherwise would populate all of the players remaining slots
		// i love private terraria methods which require reflection to access
		public static MethodInfo? playerItemCheckShoot_Info;
		public static Action<Player, int, Item, int>? playerItemCheckShoot;

		public override string Texture => AssetDirectory.JungleItem + Name;

		public StoneOfTheDrowned() : base("Stone of the Drowned", "+30 barrier\nIncreases your number of max minions by 2 when you have no barrier\nAutomatically summons two slots worth of minions if there is a valid weapon in your inventory") { }

		public override void Load()
		{
			playerItemCheckShoot_Info = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
			//Here we cache this method for performance
			playerItemCheckShoot = (Action<Player, int, Item, int>)Delegate.CreateDelegate(
				typeof(Action<Player, int, Item, int>), playerItemCheckShoot_Info);

		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 30;
			if (Player.GetModPlayer<BarrierPlayer>().barrier <= 0)
			{
				Player.maxMinions += 2;

				if (Player.slotsMinions < Player.maxMinions && slotsUsed < 2)
				{
					Item summoningItem = null;

					for (int i = 0; i < Player.inventory.Length; i++)
					{
						Item item = Player.inventory[i];

						var dummyProj = new Projectile();

						dummyProj.SetDefaults(item.shoot);

						if (item.CountsAsClass(DamageClass.Summon) && dummyProj.minion && dummyProj.minionSlots <= 2f)
						{
							summoningItem = item;
							break;
						}
					}

					if (summoningItem != null)
					{
						var dummyProj = new Projectile();

						dummyProj.SetDefaults(summoningItem.shoot);

						for (int i = 0; i < 2 / dummyProj.minionSlots; i++)
						{
							playerItemCheckShoot(Player, Player.whoAmI, summoningItem, summoningItem.damage);
							for (int d = 0; d < 5; d++)
							{
								Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(255, 0, 255), 0.5f);
							}

							slotsUsed += dummyProj.minionSlots;
						}

						Terraria.Audio.SoundEngine.PlaySound(SoundID.Item43, Player.Center);
					}
				}
			}
			else if (slotsUsed > 0)
			{
				slotsUsed = 0;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient<HauntedAmethyst>().
				AddIngredient<AquaSapphire>().
				AddTile(TileID.TinkerersWorkbench).
				Register();
		}
	}
}