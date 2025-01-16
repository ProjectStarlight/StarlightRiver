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
	internal class Reckless : CustomTooltipPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Reckless");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void Update(Item Item, Player Player)
		{
			Player.GetDamage(DamageClass.Generic) += 0.08f;
			Player.statDefense -= 4;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "RecklessTip1", "+8% damage")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "RecklessTip2", "-4 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			});
		}
	}
}