using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Reflection;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class StoneOfTheDrowned : SmartAccessory
	{
		internal bool summoned;

		// i love private terraria methods which require reflection to access
		public static MethodInfo? playerItemCheckShoot_Info;
		public static Action<Player, int, Item, int>? playerItemCheckShoot;

		public override string Texture => AssetDirectory.JungleItem + Name;

		public StoneOfTheDrowned() : base("Stone of the Drowned",
			"+30 {{barrier}}\n" +
			"Increases your max number of minions by 2 when you have no {{barrier}}\n" +
			"Automatically re-summons two slots worth of minions when you reach 0 {{barrier}}")
		{ }

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

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<BarrierPlayer>().maxBarrier += 30;

			if (player.GetModPlayer<BarrierPlayer>().barrier <= 0)
			{
				player.maxMinions += 2;

				if (!summoned)
				{
					Helpers.SummonerHelper.RespawnMinions(player, 2);
					summoned = true;
				}
			}
			else
			{
				summoned = false;
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