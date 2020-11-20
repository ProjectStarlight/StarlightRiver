using System;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Projectiles.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class HolyAmulet : SmartAccessory
    {
        public HolyAmulet() : base("Holy Amulet", "It's obviously not done, being lazy sry") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtAccessory;
            return true;
        }
        public override void SafeUpdateEquip(Player player)
        {
            player.maxRunSpeed += 6f;
        }

        private bool PreHurtAccessory(Player player,bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(player))
            {
                float xvel = Math.Abs(player.velocity.X);
                float half = player.maxRunSpeed - 6f;
                if (xvel > half)
                {
                    float percentover = (xvel - half) / (half);
                    damage = (int)(damage * (1f + (Math.Min(1f,percentover) * 0.50f)));
                    //Main.NewText(damage + " you were moving too fast");
                }
            }
            return true;
        }

    }
}