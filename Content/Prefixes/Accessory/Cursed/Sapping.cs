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
	internal class Sapping : CustomTooltipPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sapping");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void Update(Item Item, Player Player)
		{
			Player.lifeRegen += 10;
			Player.manaRegenBonus += 10;

			Player.GetDamage(DamageClass.Generic) -= 0.02f;
			Player.GetCritChance(DamageClass.Generic) -= 2;
			Player.statDefense -= 2;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "SappingTip1", "+10 life regeneration")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "SappingTip2", "+10 mana regeneration")
			{
				IsModifier = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "SappingTip3", "-2% damage")
			{
				IsModifier = true,
				IsModifierBad = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "SappingTip4", "-2% critical strike chance")
			{
				IsModifier = true,
				IsModifierBad = true
			});

			tooltips.Add(new TooltipLine(StarlightRiver.Instance, "SappingTip5", "-2 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			});

		}
	}
}