using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    public class PalePillar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pale Pillar");
        }

        public override void SetDefaults()
        {
            projectile.hide = true;
            projectile.hostile = false;
            projectile.width = 30;
            projectile.height = 5;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.damage = 1;
            projectile.penetrate = -1;
            projectile.alpha = 0;
            projectile.timeLeft = 600;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        int phase = 0; //0 = invis, 1 = raising, 2 = raised
        public override bool PreAI()
        {
            projectile.velocity.X = 0;
            switch (phase)
            {
                case 0:
                    projectile.velocity.Y = 24;
                    projectile.alpha = 255;
                    break;
                case 1:
                    projectile.velocity.Y = -6;
                    projectile.alpha = 0;
                    if (projectile.timeLeft < 70)
                    {
                        phase = 2;
                    }
                    break;
                case 2:
                    projectile.friendly = false;
                    projectile.velocity.Y = 0;
                    break;
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != projectile.velocity.Y && phase == 0)
            {
                projectile.velocity.Y = -6;
                phase = 1;
                projectile.timeLeft = 80;
                projectile.alpha = 0;
                projectile.tileCollide = false;
                projectile.position.Y += 20;
                projectile.friendly = true;
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.noGravity)
            {
                target.velocity.Y = -10;
            }
        }

    }
}