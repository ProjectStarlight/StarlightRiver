using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Relic
{
	internal class Starborn : ModPrefix
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
			DisplayName.SetDefault("Starborn");
			RelicItem.relicPrefixSet[Type] = true;
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 20f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.statManaMax2 += 50;
			player.GetModPlayer<AbilityHandler>().StaminaMaxBonus += 1;
			player.GetDamage(DamageClass.Magic).Additive += 0.05f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "StarbornTip1", "Increases maximum mana by 50")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "StarbornTip2", "Increases maximum starlight by 1")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "StarbornTip3", "5% increased magic damage")
			{
				IsModifier = true
			};
		}
	}
}