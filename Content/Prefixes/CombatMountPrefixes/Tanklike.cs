using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.CombatMountPrefixes;

public class Tanklike : CombatMountPrefix
{
	public override void ApplyToMount(CombatMount mount)
	{
		mount.primarySpeedMultiplier -= 0.25f;
		mount.secondaryCooldownSpeedMultiplier -= 0.25f;
		mount.moveSpeedMultiplier -= 0.4f;
	}

	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		yield return new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+25% Attack Speed")
		{
			IsModifier = true
		};

		yield return new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "+25% Secondary Cooldown Recovery")
		{
			IsModifier = true
		};

		yield return new TooltipLine(StarlightRiver.Instance, "PrefixTip3", "-40% Movement Speed")
		{
			IsModifier = true,
			IsModifierBad = true
		};
	}
}