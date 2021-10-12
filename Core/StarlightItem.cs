using Microsoft.Xna.Framework;
using StarlightRiver.Prefixes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
    {
        public Rectangle meleeHitbox;
        public string prefixLine = "";

        //Prefix handlers

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) => meleeHitbox = hitbox;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            var prefix = ModPrefix.GetPrefix(item.prefix);

            if (prefix is CustomTooltipPrefix)
                (prefix as CustomTooltipPrefix).Update(item, player);

            base.UpdateAccessory(item, player, hideVisual);
        }

        public override int ChoosePrefix(Item item, UnifiedRandom rand)
        {
            //resetting for custom prefix stuff
            prefixLine = "";

            return -1;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ModPrefix.GetPrefix(item.prefix) is CustomTooltipPrefix)
            {
                var critLine = tooltips.Find(n => n.Name == "Knockback");
                int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

                TooltipLine line = new TooltipLine(mod, "CustomPrefix", prefixLine);
                line.isModifier = true;
                line.isModifierBad = false;
                tooltips.Insert(index + 1, line);
            }

            //Crit display. Same as ammo, maybe move this later?
            if(item.damage > 0 && item.crit > -4)
			{
                TooltipLine line = new TooltipLine(mod, "CritDamage", "");

                var critLine = tooltips.Find(n => n.Name == "Damage");

                if (critLine != null)
                {
                    int index = tooltips.IndexOf(critLine);

                    var mp = Main.LocalPlayer.GetModPlayer<CritMultiPlayer>();

                    float mult = 2;
                    if (item.melee) mult += mp.MeleeCritMult;
                    if (item.ranged) mult += mp.RangedCritMult;
                    if (item.magic) mult += mp.MagicCritMult;
                    mult += mp.AllCritMult;

                    line.text = $"{(int)(item.damage * mult)} critical strike damage";
                    line.overrideColor = new Color(255, 200, 100);
                    tooltips.Insert(index + 1, line);
                }
            }

            //Ammo display, maybe move this later? TODO?

            if(item.useAmmo != 0)
            {
                TooltipLine line = new TooltipLine(mod, "AmmoInfo", "Uses:");

                var critLine = tooltips.Find(n => n.Name == "Knockback");
                int index = critLine is null ? tooltips.Count - 1 : tooltips.IndexOf(critLine);

                line.text += $"[i:{ item.useAmmo}]";

                tooltips.Insert(index + 1, line);
            }
        }
    }
}
