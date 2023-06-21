using ReLogic.Content;
using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Jungle
{
	public class HauntedAmethyst : SmartAccessory
	{
		// i love private terraria methods which require reflection to access
		public static MethodInfo? playerItemCheckShoot_Info;
		public static Action<Player, int, Item, int>? playerItemCheckShoot;

		public override string Texture => AssetDirectory.JungleItem + "Corpseflower";

		public HauntedAmethyst() : base("Haunted Amethyst", "Increases your number of max minions by 1 when below 50% life\nAutomatically summons a minion if there is a valid weapon in your inventory") { }
		
		public override void Load()
		{
			playerItemCheckShoot_Info = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
			//Here we cache this method for performance
			playerItemCheckShoot = (Action<Player, int, Item, int>)Delegate.CreateDelegate(
				typeof(Action<Player, int, Item, int>), playerItemCheckShoot_Info);

		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			if (Player.statLife < Player.statLifeMax2 * 0.5f)
			{
				Player.maxMinions += 1;
				if (Player.slotsMinions < Player.maxMinions)
				{
					Item summoningItem = null;

					for (int i = 0; i < Player.inventory.Length; i++)
					{
						Item item = Player.inventory[i];

						var dummyProj = new Projectile();

						dummyProj.SetDefaults(item.shoot);

						if (item.CountsAsClass(DamageClass.Summon) && dummyProj.minion && dummyProj.minionSlots <= 1f)
						{
							summoningItem = item;
							break;
						}					
					}

					if (summoningItem != null)
					{
						playerItemCheckShoot(Player, Player.whoAmI, summoningItem, summoningItem.damage);
						for (int i = 0; i < 15; i++)
						{
							Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(255, 0, 255), 0.5f);
						}
					}
				}
			}
		}
	}
}
