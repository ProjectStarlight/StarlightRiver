using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed;

internal class Occult : ModPrefix
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
		DisplayName.SetDefault("Occult");
	}

	public override void ModifyValue(ref float valueMult)
	{
		valueMult *= 2f;
	}

	public override void ApplyAccessoryEffects(Player player)
	{
		player.statManaMax2 += 60;
		player.manaRegenBonus -= 25;
	}

	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		yield return new TooltipLine(StarlightRiver.Instance, "OccultTip1", "+60 maximum mana")
		{
			IsModifier = true
		};

		yield return new TooltipLine(StarlightRiver.Instance, "OccultTip2", "-25 mana regeneration")
		{
			IsModifier = true,
			IsModifierBad = true
		};
	}
}