using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.InoculationSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Carnal : ModPrefix
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
			DisplayName.SetDefault("Carnal");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.GetModPlayer<InoculationPlayer>().DoTResist += 0.08f;
			player.statLifeMax2 += 10;
			player.GetModPlayer<CritMultiPlayer>().AllCritMult -= 0.2f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "CarnalTip1", "+8% {{Inoculation}}")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "CarnalTip2", "+10 maximum life")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "CarnalTip3", "20% reduced critical strike damage")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}