using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.BaseTypes
{
    class RelicItem : GlobalItem
    {
        public bool isRelic = false;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override bool NeedsSaving(Item item) => isRelic;

        public Color RelicColor(int offset) => Color.Lerp(Color.Yellow, Color.LimeGreen, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);
        public Color RelicColorBad(int offset) => Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);

        public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
        {
            if (isRelic)
            {
                if (pre == -3)
                    return false;

                if (pre == -1)
                    return true;
            }

            return base.PrefixChance(item, pre, rand);
        }

        public override int ChoosePrefix(Item item, UnifiedRandom rand)
        {
            if (isRelic)
            {
                //TODO: prefix whitelist here
            }

            return base.ChoosePrefix(item, rand);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (isRelic)
            {
                for(int k = 0; k < tooltips.Count; k++)
                {
                    var line = tooltips[k];

                    if (line.Name == "ItemName")
                    {
                        line.text = line.text.Insert(0, "Twice ");
                        line.overrideColor = RelicColor(k);
                    }

                    if (line.isModifier)
                        line.overrideColor = RelicColor(k);

                    if (line.isModifierBad)
                        line.overrideColor = RelicColorBad(k);
                }

                var newLine = new TooltipLine(mod, "relicLine", "Cannot be reforged");
                newLine.overrideColor = new Color(255, 180, 100);
                tooltips.Add(newLine);
            }
        }

        public override TagCompound Save(Item item)
        {
            return new TagCompound()
            {
                ["isRelic"] = isRelic
            };
        }

        public override void Load(Item item, TagCompound tag)
        {
            isRelic = tag.GetBool("isRelic");
        }
    }
}
