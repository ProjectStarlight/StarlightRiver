using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InoculationSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Relic
{
	internal class Flawless : ModPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.GetGlobalItem<RelicItem>().isRelic;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flawless");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 20f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.statDefense += 8;
			player.endurance += 0.1f;
			player.GetModPlayer<InoculationPlayer>().DoTResist += 0.15f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "FlawlessTip1", "+8 Defense")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "FlawlessTip2", "Reduces damage taken by 10%")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "FlawlessTip3", "+15% {{Inoculation}}")
			{
				IsModifier = true
			};
		}
	}
}
