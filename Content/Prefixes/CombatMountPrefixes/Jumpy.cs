using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.CombatMountPrefixes
{
	public class Jumpy : CombatMountPrefix
	{
		public override void ApplyToMount(CombatMount mount)
		{
			mount.primarySpeedMultiplier -= 0.15f;
			mount.moveSpeedMultiplier += 0.1f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			var newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+15% Attack Speed")
			{
				IsModifier = true
			};

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "+10% Movement Speed")
			{
				IsModifier = true
			};

			tooltips.Add(newline);
		}
	}
}