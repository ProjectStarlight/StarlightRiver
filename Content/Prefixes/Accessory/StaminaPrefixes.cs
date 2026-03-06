using StarlightRiver.Content.Abilities;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory
{
	internal abstract class StaminaPrefix : ModPrefix
	{
		private readonly int power;
		private readonly string name;
		private readonly string tip;

		internal StaminaPrefix(int power, string name, string tip)
		{
			this.power = power;
			this.name = name;
			this.tip = tip;
		}

		public override bool CanRoll(Item Item)
		{
			return Item.accessory;
		}

		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(name);
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 1 + 0.05f * power;
		}

		public override void ApplyAccessoryEffects(Player player) 
		{
			player.GetHandler().StaminaRegenRate += power;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "StaminaPrefixTip", tip)
			{
				IsModifier = true
			};
		}
	}

	internal class StaminaPrefix1 : StaminaPrefix
	{
		public StaminaPrefix1() : base(1, "Sparkling", "+1 Starlight regeneration") { }
	}

	internal class StaminaPrefix2 : StaminaPrefix
	{
		public StaminaPrefix2() : base(2, "Shining", "+2 Starlight regeneration") { }
	}

	internal class StaminaPrefix3 : StaminaPrefix
	{
		public StaminaPrefix3() : base(3, "Glowing", "+3 Starlight regeneration") { }
	}

	internal class StaminaPrefix4 : StaminaPrefix
	{
		public StaminaPrefix4() : base(4, "Radiant", "+4 Starlight regeneration") { }
	}
}