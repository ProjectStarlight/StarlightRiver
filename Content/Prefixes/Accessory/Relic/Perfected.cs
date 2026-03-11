using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Relic;

internal class Perfected : ModPrefix
{
	public override PrefixCategory Category => PrefixCategory.Accessory;

	public override bool CanRoll(Item item)
	{
		return item.accessory;
	}

	public override float RollChance(Item item)
	{
		return 0f;
	}

	public override void SetStaticDefaults()
	{
		DisplayName.SetDefault("Perfected");
		RelicItem.relicPrefixSet[Type] = true;
	}

	public override void ModifyValue(ref float valueMult)
	{
		valueMult *= 20f;
	}

	public override void ApplyAccessoryEffects(Player player)
	{
		player.GetDamage(DamageClass.Generic) += 0.1f;
		player.GetCritChance(DamageClass.Generic) += 0.06f;
		player.GetModPlayer<CritMultiPlayer>().AllCritMult += 0.25f;
	}

	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		yield return new TooltipLine(StarlightRiver.Instance, "PerfectedTip1", "10% increased damage")
		{
			IsModifier = true
		};

		yield return new TooltipLine(StarlightRiver.Instance, "PerfectedTip2", "6% increased critical strike chance")
		{
			IsModifier = true
		};

		yield return new TooltipLine(StarlightRiver.Instance, "PerfectedTip3", "25% increased critical strike damage")
		{
			IsModifier = true
		};
	}
}