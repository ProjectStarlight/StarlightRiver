using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Prefixes.CombatMountPrefixes
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
			var newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+25% Attack Speed")
			{
				IsModifier = true
			};

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "+25% Secondary Cooldown Recovery")
			{
				IsModifier = true
			};

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "-40% Movement Speed")
			{
				IsModifier = true,
				IsModifierBad = true
			};

			tooltips.Add(newline);
		}
	}
}
