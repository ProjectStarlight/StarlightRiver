using Microsoft.Xna.Framework;
using StarlightRiver.Prefixes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
    {
        public Rectangle meleeHitbox;
        public string prefixLine = "";

        //Prefix handlers

        public override bool InstancePerEntity => true;

        public override void UseItemHitbox(Item Item, Player Player, ref Rectangle hitbox, ref bool noHitbox) => meleeHitbox = hitbox;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
            return item.GetGlobalItem<StarlightItem>();
		}

		public override void UpdateAccessory(Item Item, Player Player, bool hideVisual)
        {
            var prefix = PrefixLoader.GetPrefix(Item.prefix);

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
                var critLine = tooltips.Find(n => n.Name == "Knockback");
                int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

                TooltipLine line = new TooltipLine(Mod, "CustomPrefix", prefixLine);
                line.isModifier = true;
                line.isModifierBad = false;
                tooltips.Insert(index + 1, line);
            }

            //Crit display. Same as ammo, maybe move this later?
            if(Item.damage > 0 && Item.crit > -4)
			{
                TooltipLine line = new TooltipLine(Mod, "CritDamage", "");

                var critLine = tooltips.Find(n => n.Name == "Damage");

                if (critLine != null)
                {
                    int index = tooltips.IndexOf(critLine);

                    var mp = Main.LocalPlayer.GetModPlayer<CritMultiPlayer>();

                    float mult = 2;
                    if (Item.DamageType.CountsAs(DamageClass.Melee)) mult += mp.MeleeCritMult;
                    if (Item.DamageType.CountsAs(DamageClass.Ranged)) mult += mp.RangedCritMult;
                    if (Item.DamageType.CountsAs(DamageClass.Magic)) mult += mp.MagicCritMult;
                    mult += mp.AllCritMult;

                    line.text = $"{(int)(Item.damage * mult)} critical strike damage";
                    line.overrideColor = new Color(255, 200, 100);
                    tooltips.Insert(index + 1, line);
                }
            }

            //Ammo display, maybe move this later? TODO?

            if(Item.useAmmo != 0)
            {
                TooltipLine line = new TooltipLine(Mod, "AmmoInfo", "Uses:");

                var critLine = tooltips.Find(n => n.Name == "Knockback");
                int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

                line.text += $"[i:{ Item.useAmmo}]";

                tooltips.Insert(index + 1, line);
            }
        }
    }
}
