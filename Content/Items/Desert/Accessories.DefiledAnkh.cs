using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Potions;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Desert
{
	public class DefiledAnkh : CursedAccessory
	{
		public override string Texture => AssetDirectory.DesertItem + Name;

		public DefiledAnkh() : base(ModContent.Request<Texture2D>(AssetDirectory.DesertItem + "DefiledAnkh").Value) { }

		public override void Load()
		{
			StarlightPlayer.PreUpdateBuffsEvent += RemoveDebuffs;
		}

		private void RemoveDebuffs(Player player)
		{
			if (Equipped(player) && (player.GetModPlayer<BarrierPlayer>().barrier > 0 || player.GetModPlayer<BarrierPlayer>().justHitWithBarrier))
			{
				for (int i = 0; i < player.buffType.Length; i++)
				{
					int buffType = player.buffType[i];

					if (Helper.IsValidDebuff(player, i) && buffType != ModContent.BuffType<NoShieldPot>() && Main.debuff[buffType])
					{
						player.DelBuff(i);
						i--;
					}
				}
			}
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Cursed : Your Barrier protects against twenty-five percent less damage\nYou cannot gain any debuff besides Potion, Barrier and Mana Sickness while Barrier is active" +
				"\n+40 max Barrier");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 3, silver: 50);
		}

		public override void SafeUpdateEquip(Player player)
		{
			BarrierPlayer bp = player.GetModPlayer<BarrierPlayer>();
			bp.barrierDamageReduction -= 0.25f;
			bp.maxBarrier += 40;
		}
	}
}