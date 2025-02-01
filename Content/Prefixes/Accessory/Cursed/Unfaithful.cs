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
	internal class Unfaithful : CustomTooltipPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Unfaithful");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void Update(Item Item, Player Player)
		{
			Player.GetCritChance(DamageClass.Generic) += 6;
			Player.GetDamage(DamageClass.Generic) -= 0.05f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "UnfaithfulTip1", "+6% critical strike chance")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "UnfaithfulTip2", "-5% damage")
			{
				IsModifier = true,
				IsModifierBad = true
			});
		}
	}
}