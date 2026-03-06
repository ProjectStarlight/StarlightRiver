using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Unfaithful : ModPrefix
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
			DisplayName.SetDefault("Unfaithful");
		}
			
		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.GetCritChance(DamageClass.Generic) += 6;
			player.GetDamage(DamageClass.Generic) -= 0.05f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "UnfaithfulTip1", "+6% critical strike chance")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "UnfaithfulTip2", "-5% damage")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}