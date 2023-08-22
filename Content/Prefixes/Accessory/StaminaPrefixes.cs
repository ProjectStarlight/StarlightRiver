﻿using StarlightRiver.Content.Abilities;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory
{
	internal abstract class StaminaPrefix : CustomTooltipPrefix
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

		public override void Update(Item Item, Player Player)
		{
			Player.GetHandler().StaminaRegenRate += power;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			var newline = new TooltipLine(StarlightRiver.Instance, "StaminaPrefixTip", tip)
			{
				IsModifier = true
			};

			tooltips.Add(newline);
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