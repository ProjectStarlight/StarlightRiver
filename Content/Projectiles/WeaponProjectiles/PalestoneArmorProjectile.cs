using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class PalestoneArmorProjectile : ModProjectile
    {
        private int MaxCharge = 300;
        private Vector2 LastLocation;
        public static int MaxTablets = 3;
        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Tablet");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.PlaySound(SoundID.Item27);
        }

        public override bool CanDamage()
        {
            return false;
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            projectile.localAI[0] = (float)reader.ReadDouble();
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write((double)projectile.localAI[0]);
        }

        public PalestoneArmorProjectile()
        {
            LastLocation = Vector2.One;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)projectile.ai[1]];
            Player owner = projectile.Owner();
            bool conditions = (owner != null && owner.active) && owner.Distance(projectile.Center) < 300;//Grow or no?
            bool slamming = projectile.ai[0] > MaxCharge;


            if (slamming)
            {
                Slam();
                return;
            }

            if (Helper.IsTargetValid(target) && projectile.ai[0] >= 0)
            {
                LastLocation = target.position + new Vector2(target.width / 2, -96);
                projectile.ai[0] += conditions ? 1 : -1;
            }
            else
            {
                projectile.Kill();
            }

            projectile.Center = LastLocation;

            void Slam()
            {
                projectile.ai[0] += 1;
                //target.
            }

        }
    }

}