using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Prefixes.CombatMountPrefixes
{
	public class Feral : CombatMountPrefix
	{
		public override void ApplyToMount(CombatMount mount)
		{
			mount.primarySpeedMultiplier -= 0.35f;
			mount.secondaryCooldownSpeedMultiplier += 0.5f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TooltipLine newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+35% Attack Speed");
			newline.IsModifier = true;

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "-50% Secondary Cooldown Recovery");
			newline.IsModifier = true;
			newline.IsModifierBad = true;

			tooltips.Add(newline);
		}
	}
}
