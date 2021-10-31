using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class FireCone : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 1;
            projectile.height = 1;
            projectile.timeLeft = 2;
            projectile.hide = true;
        }

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
            drawCacheProjsOverWiresUI.Add(index);
		}

		public override void AI()
        {
            projectile.timeLeft = 2;
            projectile.ai[0]++; //ticks up the timer

            if (projectile.ai[0] <= 30) //drawing in fire
            {
                var pos1 = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 0.2f) * Main.rand.Next(-550, -450);
                var pos2 = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation + 0.2f) * Main.rand.Next(-550, -450);
                var posRand = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.Next(-420, -380);

                Dust.NewDustPerfect(pos1, DustType<PowerupDust>(), (pos1 - projectile.Center) * -0.03f, 0, new Color(255, 240, 220), projectile.ai[0] / 25f);
                Dust.NewDustPerfect(pos2, DustType<PowerupDust>(), (pos2 - projectile.Center) * -0.03f, 0, new Color(255, 240, 220), projectile.ai[0] / 25f);
                Dust.NewDustPerfect(posRand, DustType<PowerupDust>(), (posRand - projectile.Center) * -0.03f, 0, new Color(255, 220, 100), projectile.ai[0] / 25f);
            }

            if(projectile.ai[0] == 70)
			{
                for (int k = 0; k < 4; k++)
                {
                    var rot = projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f);
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * -80, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(rot), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                }

                Main.PlaySound(SoundID.DD2_BetsyFireballShot, projectile.Center);
            }

			if (Main.expertMode)
			{
				if (projectile.ai[0] == 70)
					Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(projectile.rotation + 3.14f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), projectile.damage, 1, projectile.owner);

                if (projectile.ai[0] == 74)
                {
                    Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(projectile.rotation + 3.14f + 0.5f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), projectile.damage, 1, projectile.owner);
                    Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(projectile.rotation + 3.14f - 0.5f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), projectile.damage, 1, projectile.owner);
                }

                if (projectile.ai[1] == 1 && projectile.ai[0] == 78)
                {
                    Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(projectile.rotation + 3.14f + 1f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), projectile.damage, 1, projectile.owner);
                    Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy(projectile.rotation + 3.14f - 1f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), projectile.damage, 1, projectile.owner);
                }
            }

            if (projectile.ai[0] >= 94) //when this projectile goes off
            {
                projectile.Kill(); //self-destruct
            }
        }

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
            target.AddBuff(BuffID.OnFire, 300);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (projectile.ai[0] > 68 && projectile.ai[0] < 76)
            {
                return Helper.CheckConicalCollision(projectile.Center, 700, projectile.rotation, 0.2f, targetHitbox);
            }

            return false;
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (projectile.ai[0] < 66) //draws the proejctile's tell ~1 second before it goes off
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/ConeTell");
                float alpha = (projectile.ai[0] * 2 / 33 - (float)Math.Pow(projectile.ai[0], 2) / 1086) * 0.5f;
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), new Color(255, 170, 100) * alpha, projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[0] >= 66) //draws the proejctile
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/LavaBurst");
                Rectangle frame = new Rectangle(0, tex.Height / 7 * (int)((projectile.ai[0] - 66) / 4), tex.Width, tex.Height / 7);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height / 7), 2, 0, 0);
            }

            return false;
        }
    }
}