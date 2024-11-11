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
	internal class Crystalline : CustomTooltipPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystalline");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void Update(Item Item, Player Player)
		{
			Player.endurance += 0.03f;
			Player.statDefense += 4;
			Player.moveSpeed -= 0.1f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "CrystallineTip1", "+4 defense")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "CrystallineTip2", "+3% damage resistance")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "CrystallineTip3", "-10% movement speed")
			{
				IsModifier = true,
				IsModifierBad = true
			});
		}
	}
}