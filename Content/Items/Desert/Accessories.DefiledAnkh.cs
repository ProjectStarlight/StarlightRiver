using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Desert
{
	public class DefiledAnkh : CursedAccessory
	{
		public override string Texture => AssetDirectory.DesertItem + Name;

		public DefiledAnkh() : base(ModContent.Request<Texture2D>(AssetDirectory.DesertItem + "DefiledAnkh").Value) { }

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Cursed : Your Barrier protects against twenty-five percent less damage\n+100% inoculation while barrier is active\nimmunity to most debuffs while barrier is active" +
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

			if (bp.barrier > 0)
			{
				player.buffImmune[BuffID.Bleeding] = true;
				player.buffImmune[BuffID.Confused] = true;
				player.buffImmune[BuffID.Darkness] = true;
				player.buffImmune[BuffID.Silenced] = true;
				player.buffImmune[BuffID.Weak] = true;
				player.buffImmune[BuffID.BrokenArmor] = true;
				player.buffImmune[BuffID.Cursed] = true;
				player.buffImmune[BuffID.Poisoned] = true;
				player.buffImmune[BuffID.Slow] = true;
				player.buffImmune[BuffID.Stoned] = true;
				player.buffImmune[BuffID.Rabies] = true;
				player.buffImmune[BuffID.Chilled] = true;
				player.buffImmune[BuffID.Ichor] = true;
				player.buffImmune[BuffID.Frozen] = true;
				player.buffImmune[BuffID.Webbed] = true;

				player.GetModPlayer<DoTResistancePlayer>().DoTResist += 1;
			}
		}
	}
}