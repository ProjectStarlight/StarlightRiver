using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Eldritch : ModPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.accessory;
		}

		public override float RollChance(Item item)
		{
			return item.ModItem is CursedAccessory ? 1f : 0f;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Eldritch");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.GetModPlayer<BarrierPlayer>().maxBarrier += 40;
			player.GetModPlayer<BarrierPlayer>().barrierDamageReduction += 0.05f;
			player.statDefense -= 6;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "EldritchTip1", "+40 maximum {{Barrier}}")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "EldritchTip2", "+5% {{Barrier}} effectiveness")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "EldritchTip3", "-6 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}