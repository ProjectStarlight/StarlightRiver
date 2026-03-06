using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Ephemeral : ModPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.ModItem is CursedAccessory;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ephemeral");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.moveSpeed += 0.15f;
			player.statLifeMax2 -= 20;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "EphemeralTip1", "+15% movement speed")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "EphemeralTip2", "-20 maximum life")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}