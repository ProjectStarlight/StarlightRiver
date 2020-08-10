using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Prefixes;
using Terraria.Utilities;

namespace StarlightRiver.Core
{
    internal partial class StarlightItem : GlobalItem
    {
        public Rectangle meleeHitbox;
        public string prefixLine = "";

        //Prefix handlers
        public int staminaRegenUp = 0;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) => meleeHitbox = hitbox;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            base.UpdateAccessory(item, player, hideVisual);

            //prefix updates for accessoies
            player.GetModPlayer<Abilities.AbilityHandler>().StatStaminaRegenMax -= staminaRegenUp;
        }

        public override int ChoosePrefix(Item item, UnifiedRandom rand)
        {
            //resetting for custom prefix stuff
            prefixLine = "";
            staminaRegenUp = 0;

            return -1;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ModPrefix.GetPrefix(item.prefix) is CustomTooltipPrefix)
            {
                TooltipLine line = new TooltipLine(mod, "CustomPrefix", prefixLine);
                line.isModifier = true;
                line.isModifierBad = false;
                tooltips.Add(line);
            }
        }
    }
}
