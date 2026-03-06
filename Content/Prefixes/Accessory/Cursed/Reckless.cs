using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Reckless : ModPrefix
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

		public override void ApplyAccessoryEffects(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.08f;
			player.statDefense -= 4;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "RecklessTip1", "+8% damage")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "RecklessTip2", "-4 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}