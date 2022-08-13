using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class Corpseflower : CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Corpseflower() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Corpseflower").Value) { }

        public override void Load()
        {
            StarlightPlayer.ModifyHitNPCEvent += ApplyDoTItem;
            StarlightPlayer.ModifyHitNPCWithProjEvent += ApplyDoTProjectile;
        }

        private void ApplyDoTProjectile(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Equipped(player))
            {
                target.GetGlobalNPC<CorpseflowerGlobalNPC>().damageAndTimers.Add(new CorpseflowerStruct((int)(damage * 0.33f), 600));
                crit = false;
                damage = 0;
            }
        }

        private void ApplyDoTItem(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (Equipped(player))
            {
                target.GetGlobalNPC<CorpseflowerGlobalNPC>().damageAndTimers.Add(new CorpseflowerStruct((int)(damage * 0.33f), 600));
                crit = false;
                damage = 0;
            }
        }

        public override void SetStaticDefaults()
		{ 
			Tooltip.SetDefault("All damage dealt is converted into damage over time\nDamage is decreased by 66%\nYou are unable to critically strike while equipped");
		}
	}

    public struct CorpseflowerStruct
    {
        public int damage;

        public int timer;

        public CorpseflowerStruct(int damage, int timer)
        {
            this.damage = damage;
            this.timer = timer;
        }
    }

    class CorpseflowerGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public List<CorpseflowerStruct> damageAndTimers = new List<CorpseflowerStruct>();

        public override void ResetEffects(NPC npc)
        {
            for (int i = 0; i < damageAndTimers.Count; i++)
            {
                if (damageAndTimers[i].timer > 0)
                    damageAndTimers[i] = new CorpseflowerStruct(damageAndTimers[i].damage, damageAndTimers[i].timer - 1);
                else
                {
                    damageAndTimers[i] = new CorpseflowerStruct(0, 0);
                    damageAndTimers.Remove(damageAndTimers[i]);
                }
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (damageAndTimers.Count <= 0)
            {
                if (npc.lifeRegen < 0)
                    npc.lifeRegen = 0;

                damage = 0;
                return;
            }

            for (int i = 0; i < damageAndTimers.Count; i++)
            {
                CorpseflowerStruct struct_ = damageAndTimers[i];

                if (struct_.timer > 0 && struct_.damage > 0)
                {
                    if (npc.lifeRegen > 0)
                        npc.lifeRegen = 0;

                    npc.lifeRegen -= struct_.damage; 

                    if (damage < struct_.damage)
                        damage = struct_.damage;
                }
            }
        }
    }
}
