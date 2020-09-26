using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Projectiles.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class DisinfectantWipes : SmartAccessory
    {
        public DisinfectantWipes(String tex = "Disinfectant Wipes",String othertex = "Sanitizer Spray") : base(tex, "Melee critical strikes have a 25% chance to clear a random debuff\nThis scales negatively with faster swing times\nStacks with "+ othertex) { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            return true;
        }
        private void OnHitNPCAccessory(Player player, Item proj, NPC target, int damage, float knockback, bool crit)
        {
            if (Equipped(player) && crit && (Main.rand.Next(0,player.itemAnimationMax)>(int)(60f*0.25f)))
            {
                CleanDebuff(player);
            }
        }

        public static void CleanDebuff(Player player)
        {
            List<int> buffs = new List<int>();
            for (int i = 0; i < Player.MaxBuffs; i += 1)
            {
                if (Helper.IsValidDebuff(player, i))
                {
                    buffs.Add(i);
                }
            }
            if (buffs.Count > 0)
            {
                Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 100, 0.65f, -Main.rand.NextFloat(0.35f, 0.75f));
                player.DelBuff(Main.rand.Next(0, buffs.Count));
            }

        }

    }    
    public class SanitizerSpray : DisinfectantWipes
    {
        //It's literally the same thing
        public SanitizerSpray(String tex = "SanitizerSpray") : base("Sanitizer Spray", "Disinfectant Wipes") { }

    }
}