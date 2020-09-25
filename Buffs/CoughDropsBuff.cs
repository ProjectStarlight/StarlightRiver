using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    public class CoughDropsBuff : SmartBuff
    {
        public CoughDropsBuff() : base("Cough Drops", "Speed and Damage improved based on buff time", false) { }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            if (buffIndex >= 0)
            {
                if (player.buffTime[buffIndex] < 1)
                {
                    return true;
                }
                else
                {
                    time = (int)MathHelper.Clamp(time + 100, 0, 60 * 20);
                }

            }
            return false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (buffIndex >= 0)
            {
                float time = (float)player.buffTime[buffIndex] - (60 * 5);
                player.maxRunSpeed += 2f + MathHelper.Clamp(time / 300f, 0, 4);
                player.BoostAllDamage(0.05f + MathHelper.Clamp((time / 600f), 0, 0.15f));
            }
        }
    }
}
