using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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


        public ref float Timer => ref Projectile.ai[0];
        public ref float Rotation => ref Projectile.ai[1];

        public bool extraShots = false;

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
        }

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
            overWiresUI.Add(index);
        }

		public override void AI()
        {
            Projectile.rotation = Rotation;
            Projectile.timeLeft = 2;
            Timer++; //ticks up the timer

            if (Timer <= 30) //drawing in fire
            {
                var pos1 = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 0.2f) * Main.rand.Next(-550, -450);
                var pos2 = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + 0.2f) * Main.rand.Next(-550, -450);
                var posRand = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.Next(-420, -380);

                Dust.NewDustPerfect(pos1, DustType<PowerupDust>(), (pos1 - Projectile.Center) * -0.03f, 0, new Color(255, 240, 220), Timer / 25f);
                Dust.NewDustPerfect(pos2, DustType<PowerupDust>(), (pos2 - Projectile.Center) * -0.03f, 0, new Color(255, 240, 220), Timer / 25f);
                Dust.NewDustPerfect(posRand, DustType<PowerupDust>(), (posRand - Projectile.Center) * -0.03f, 0, new Color(255, 220, 100), Timer / 25f);
            }

            if(Timer == 70)
			{
                for (int k = 0; k < 4; k++)
                {
                    var rot = Projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f);
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * -80, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(rot), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Projectile.Center);
            }

			if (Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (Timer == 70)
					Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation + 3.14f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);

                if (Timer == 74)
                {
                    Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation + 3.14f + 0.5f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation + 3.14f - 0.5f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);
                }

                if (Projectile.ai[1] == 1 && Timer == 78)
                {
                    Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation + 3.14f + 1f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation + 3.14f - 1f) * 11, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);
                }

                if(Timer == 70 && Main.masterMode)
				{
                    Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<FireRingHostile>(), 20, 0, Main.myPlayer, 100);
                    //TODO: Homing fireball
                }
            }

            if (Timer >= 94) //when this Projectile goes off
            {
                Projectile.Kill(); //self-destruct
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
		{
            target.AddBuff(BuffID.OnFire, 300);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (Timer > 70 && Timer < 78)
            {
                return Helper.CheckConicalCollision(Projectile.Center, (int)(((Timer - 70) / 8f) * 700), Projectile.rotation, 0.2f, targetHitbox);
            }

            return false;
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Timer < 66) //draws the proejctile's tell ~1 second before it goes off
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ConeTell").Value;
                float alpha = (Timer * 2 / 33 - (float)Math.Pow(Timer, 2) / 1086) * 0.5f;
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), new Color(255, 170, 100) * alpha, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            if (Timer >= 66) //draws the proejctile
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LavaBurst").Value;
                Rectangle frame = new Rectangle(0, tex.Height / 7 * (int)((Timer - 66) / 4), tex.Width, tex.Height / 7);
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height / 7), 2, 0, 0);
            }

            return false;
        }
    }
}