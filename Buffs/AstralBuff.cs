using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Buffs
{
    class AstralBuff : SmartBuff
    {
        public AstralBuff() : base("Zapped!", "Losing life, but zapping nearby enemies!", true) { }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/MarioCumming";
            return true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 40;

            if (Main.rand.Next(10) == 0)
            {
                Vector2 pos = player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(player.width);
                Helper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Dusts.Electric>(), 0.8f, 3);
            }

            if (Main.rand.Next(20) == 0)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC npc = Main.npc[k];
                    if (npc.active && Vector2.Distance(npc.Center, player.Center) < 100)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<LightningNode>(), 20, 0, player.whoAmI, 2, 100);
                        Helper.DrawElectricity(player.Center, npc.Center, DustType<Dusts.Electric>());
                        return;
                    }
                }
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.Next(10) == 0)
            {
                Vector2 pos = npc.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(npc.width);
                Helper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Dusts.Electric>(), 0.8f, 3);
            }

            if (Main.rand.Next(20) == 0)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC target = Main.npc[k];
                    if (target.active && Vector2.Distance(target.Center, npc.Center) < 100)
                    {
                        Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<LightningNode>(), 20, 0, 0, 2, 100);
                        Helper.DrawElectricity(npc.Center, target.Center, DustType<Dusts.Electric>());
                        return;
                    }
                }
            }
        }
    }
}
