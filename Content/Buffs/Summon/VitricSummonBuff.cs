using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Buffs.Summon
{
    class VitricSummonBuff : SmartBuff
    {
        public VitricSummonBuff() : base("Glassweaver's Arsonal", "Strike your foes with enchanted glass forged weapons!", false,true) { }

			public override void Update(Player player, ref int buffIndex)
			{
				if (player.ownedProjectileCounts[mod.ProjectileType("VitricSummonOrb")] > 0)
				{
					player.buffTime[buffIndex] = 18000;
				}
				else
				{
					player.DelBuff(buffIndex);
					buffIndex--;
				}
			}
		}
	}
