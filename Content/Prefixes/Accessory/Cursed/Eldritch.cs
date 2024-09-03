using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Eldritch : CustomTooltipPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Eldritch");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void Update(Item Item, Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 40;
			Player.GetModPlayer<BarrierPlayer>().barrierDamageReduction += 0.05f;
			Player.statDefense -= 6;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "EldritchTip1", "+40 {{Barrier}}")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "EldritchTip2", "+5% {{Barrier}} effectiveness")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "EldritchTip3", "-6 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			});
		}
	}
}