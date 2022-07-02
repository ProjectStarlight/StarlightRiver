using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Prefixes.CombatMountPrefixes
{
	public class Tanklike : CombatMountPrefix
	{
		public override void ApplyToMount(CombatMount mount)
		{
			mount.primarySpeedMultiplier -= 0.25f;
			mount.secondaryCooldownSpeedMultiplier -= 0.25f;
			mount.moveSpeedMultiplier -= 0.4f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TooltipLine newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+25% Attack Speed");
			newline.IsModifier = true;

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "+25% Secondary Cooldown Recovery");
			newline.IsModifier = true;

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "-40% Movement Speed");
			newline.IsModifier = true;
			newline.IsModifierBad = true;

			tooltips.Add(newline);
		}
	}
}
