using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Items.Salvage
{
    public class SalvageItem : ModItem
    {
        public Randstat Damage;
        public Randstat Speed;
        public Randstat Crit;

        public SalvageItem(Randstat damage, Randstat speed, Randstat crit) : base()
        {
            Damage = damage;
            Speed = speed;
            Crit = crit;
        }

        public override bool CloneNewInstances => true;

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["damage"] = Damage.value,
                ["speed"] = Speed.value,
                ["crit"] = Crit.value
            };
        }

        public override void Load(TagCompound tag)
        {
            Damage.value = tag.GetInt("damage");
            Speed.value = tag.GetInt("speed");
            Crit.value = tag.GetInt("crit");
            SetStats();
        }

        public void SetStats()
        {
            item.damage = Damage.value;
            item.useTime = Speed.value;
            item.useAnimation = Speed.value;
            item.crit = Crit.value;
        }

        public void RollStats()
        {
            Damage.value = Main.rand.Next(Damage.minimum, Damage.maximum + 1);
            Speed.value = Main.rand.Next(Speed.minimum, Speed.maximum + 1);
            Crit.value = Main.rand.Next(Crit.minimum, Crit.maximum + 1);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float percent1 = (Damage.value - Damage.minimum) / (float)(Damage.maximum - Damage.minimum);
            float percent2 = (Speed.maximum - Speed.value) / (float)(Speed.maximum - Speed.minimum);
            float percent3 = (Crit.value - Crit.minimum) / (float)(Crit.maximum - Crit.minimum);

            TooltipLine tip0 = new TooltipLine(mod, "Salvage", "Salvaged Item (" + (int)((percent1 + percent2 + percent3) / 3 * 100) + "% intact)")
            {
                overrideColor = new Color(150, 200, 255)
            };
            tooltips.Insert(1, tip0);

            TooltipLine tip1 = tooltips.FirstOrDefault(tooltip => tooltip.Name == "Damage" && tooltip.mod == "Terraria");
            if (tip1 != null)
            {
                tip1.text += " (" + (int)(percent1 * 100) + "%)";
                tip1.overrideColor = new Color((1 - percent1) * (1 / percent1), percent1 * (1 / (1 - percent1)), 0);
            }

            TooltipLine tip2 = tooltips.FirstOrDefault(tooltip => tooltip.Name == "Speed" && tooltip.mod == "Terraria");
            if (tip2 != null)
            {
                tip2.text = "Delay: " + Speed.value + " (" + (int)(percent2 * 100) + "%)";
                tip2.overrideColor = new Color((1 - percent2) * (1 / percent2), percent2 * (1 / (1 - percent2)), 0);
            }

            TooltipLine tip3 = tooltips.FirstOrDefault(tooltip => tooltip.Name == "CritChance" && tooltip.mod == "Terraria");
            if (tip3 != null)
            {
                tip3.text += " (" + (int)(percent3 * 100) + "%)";
                tip3.overrideColor = new Color((1 - percent3) * (1 / percent3), percent3 * (1 / (1 - percent3)), 0);
            }
        }
    }

    public struct Randstat
    {
        public int minimum;
        public int maximum;
        public int value;

        public Randstat(int min, int max, int val = 0)
        {
            minimum = min;
            maximum = max;
            value = val;
        }
    }
}