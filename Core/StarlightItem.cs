using StarlightRiver.Content.Prefixes;
using System.Collections.Generic;
using Terraria.Utilities;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
	{
		public Rectangle meleeHitbox;
		public string prefixLine = "";

		//Prefix handlers

		public override bool InstancePerEntity => true;

		public override void UseItemHitbox(Item Item, Player Player, ref Rectangle hitbox, ref bool noHitbox)
		{
			meleeHitbox = hitbox;
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem(out StarlightItem gi) ? gi : this;
		}

		public override void UpdateAccessory(Item Item, Player Player, bool hideVisual)
		{
			ModPrefix prefix = PrefixLoader.GetPrefix(Item.prefix);

			if (prefix is CustomTooltipPrefix)
				(prefix as CustomTooltipPrefix).Update(Item, Player);

			base.UpdateAccessory(Item, Player, hideVisual);
		}

		public override int ChoosePrefix(Item Item, UnifiedRandom rand)
		{
			//resetting for custom prefix stuff
			prefixLine = "";

			return -1;
		}

		public override void ModifyTooltips(Item Item, List<TooltipLine> tooltips)
		{
			if (PrefixLoader.GetPrefix(Item.prefix) is CustomTooltipPrefix)
			{
				TooltipLine critLine = tooltips.Find(n => n.Name == "Knockback");
				int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

				var line = new TooltipLine(StarlightRiver.Instance, "CustomPrefix", prefixLine)
				{
					IsModifier = true,
					IsModifierBad = false
				};
				tooltips.Insert(index + 1, line);
			}

			//Crit display. Same as ammo, maybe move this later?
			if (Item.damage > 0 && Item.crit > -4)
			{
				var line = new TooltipLine(StarlightRiver.Instance, "CritDamage", "");

				TooltipLine critLine = tooltips.Find(n => n.Name == "Damage");

				if (critLine != null)
				{
					int index = tooltips.IndexOf(critLine);

					CritMultiPlayer mp = Main.LocalPlayer.GetModPlayer<CritMultiPlayer>();

					float mult = 2;

					if (Item.DamageType.Type == DamageClass.Melee.Type)
						mult += mp.MeleeCritMult;

					if (Item.DamageType.Type == DamageClass.Ranged.Type)
						mult += mp.RangedCritMult;

					if (Item.DamageType.Type == DamageClass.Magic.Type)
						mult += mp.MagicCritMult;

					mult += mp.AllCritMult;

					line.Text = $"{(int)(Item.damage * mult)} critical strike damage";
					line.OverrideColor = new Color(255, 200, 100);
					tooltips.Insert(index + 1, line);
				}
			}

			//Ammo display, maybe move this later? TODO?

			if (Item.useAmmo != 0)
			{
				var line = new TooltipLine(StarlightRiver.Instance, "AmmoInfo", "Uses:");

				TooltipLine critLine = tooltips.Find(n => n.Name == "Knockback");
				int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

				line.Text += $"[i:{Item.useAmmo}]";

				tooltips.Insert(index + 1, line);
			}
		}
	}
}