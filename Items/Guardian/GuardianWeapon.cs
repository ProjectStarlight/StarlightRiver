using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Guardian
{
    internal class GuardianWeapon : ModItem
    {
        public int HealthCost { get; set; }
        public int LifeSteal { get; set; }
        public int HealRadius { get; set; }
        public int HealAmount { get; set; }

        public GuardianWeapon(int HPcost, int lifesteal, int healrad, int heal)
        {
            HealthCost = HPcost;
            LifeSteal = lifesteal;
            HealRadius = healrad;
            HealAmount = heal;
        }

        public virtual void SafeSetDefaults()
        {
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            item.melee = false;
            item.ranged = false;
            item.magic = false;
            item.thrown = false;
            item.summon = false;
        }

        public sealed override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat)
        {
            mult *= player.GetModPlayer<StarlightPlayer>().GuardDamage;
        }

        public sealed override void GetWeaponCrit(Player player, ref int crit)
        {
            crit += player.GetModPlayer<StarlightPlayer>().GuardCrit;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine tip = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.mod == "Terraria");
            if (tip != null)
            {
                string[] splitText = tip.text.Split(' ');
                string damageValue = splitText.First();
                string damageWord = splitText.Last();

                tip.text = damageValue + " guardian " + damageWord;
            }
            tooltips.Insert(2, new TooltipLine(mod, "LifeSteal", LifeSteal + " Healing on Hit"));
            tooltips.Insert(3, new TooltipLine(mod, "HealAmount", HealAmount + " Team Healing"));
            tooltips.Insert(4, new TooltipLine(mod, "HealRadius", HealRadius + " Healing Radius"));
            tooltips.Insert(5, new TooltipLine(mod, "HealthCost", "uses " + HealthCost + " Life"));
        }

        public sealed override bool CanUseItem(Player player)
        {
            if (player.statLife > HealthCost)
            {
                player.statLife -= HealthCost;
                CombatText.NewText(player.Hitbox, Microsoft.Xna.Framework.Color.Red, HealthCost);
                return true;
            }
            return false;
        }
    }
}
