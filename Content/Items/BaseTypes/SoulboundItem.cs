using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.BaseTypes
{
    internal abstract class SoulboundItem : ModItem
    {
        public virtual void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SafeModifyTooltips(tooltips);

            TooltipLine line = new TooltipLine(mod, "binding", "Soulbound")
            {
                overrideColor = new Color(150, 255, 255)
            };
            tooltips.Add(line);
        }
    }
}