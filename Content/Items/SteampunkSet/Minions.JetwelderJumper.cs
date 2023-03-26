using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class JetwelderJumper : ModProjectile
	{
		private readonly int BASE_DURATION = 1200;

		private bool jumping = false;

		private float xVel = 0f;

		private bool fired = false;

		private int fireCounter = 0;

		private Player Player => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.SteampunkItem + "JetwelderJumper";

		public override void Load()
		{
			for (int k = 1; k <= 8; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + k);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Jumper");
			Main.projFrames[Projectile.type] = 14;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 38;
			Projectile.height = 50;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.minion = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = BASE_DURATION;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, tex.Size() / new Vector2(2, 2 * Main.projFrames[Projectile.type]), Projectile.scale, effects, 0f);
			return false;
		}

		public override void AI()
		{
			NPC testtarget = Main.npc.Where(n => n.active && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
			Projectile.frameCounter++;

			if (Projectile.frameCounter % 4 == 0 && !jumping && (fireCounter == 20 || fireCounter == 0 || testtarget == default))
			{
				Projectile.frame++;

				if (Projectile.frame == 11)
					Jump(testtarget);
			}

			if (testtarget != default && !jumping && !fired)
			{
				if (Projectile.frameCounter % 4 == 0 && Projectile.frame < 5)
					Projectile.frame++;

				fireCounter++;

				if (fireCounter == 10)
					FireMissle(testtarget);

				if (fireCounter == 20)
					fired = true;
			}

			if (Projectile.velocity.Y < 15)
				Projectile.velocity.Y += 0.2f;

			if (jumping)
			{
				Projectile.velocity.X = xVel;

				if (Math.Abs(Projectile.velocity.Y) < 0.5f)
					Projectile.frame = 12;
				else if (Projectile.velocity.Y >= 0.5f)
					Projectile.frame = 13;
				else
					Projectile.frame = 11;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y != Projectile.velocity.Y && oldVelocity.Y > 0 && Projectile.frame > 10)
			{
				if (jumping)
				{
					fired = false;
					fireCounter = 0;
					jumping = false;

					for (int i = 0; i < 8; i++)
					{
						Vector2 dustVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(-2, -0.5f);
						dustVel.X *= 10;
						if (Math.Abs(dustVel.X) < 6)
							dustVel.X += Math.Sign(dustVel.X) * 4;
						Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<JetwelderJumperDust>(), dustVel, 0, new Color(236, 214, 146) * 0.3f, Main.rand.NextFloat(0.25f, 0.5f));
					}
				}

				Projectile.frame = 0;
				Projectile.frameCounter = 0;
				Projectile.velocity.X = 0;
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 1; i < 9; i++)
			{
				Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextVector2Circular(5, 5), Mod.Find<ModGore>("JetwelderJumper_Gore" + i.ToString()).Type, 1f);
			}
		}

		private void Jump(NPC target)
		{
			jumping = true;
			var dir = new Vector2(0, -1);

			if (target == default)
			{
				dir = dir.RotatedByRandom(0.4f);
			}
			else
			{
				int offsetDirection = Math.Sign(target.Center.X - Projectile.Center.X);
				dir = dir.RotatedBy(Main.rand.NextFloat(Math.Sign(target.Center.X - offsetDirection * 300 - Projectile.Center.X) * 0.6f));
			}

			Projectile.velocity = dir * Main.rand.Next(6, 11);
			xVel = Projectile.velocity.X;

			Projectile.spriteDirection = Math.Sign(dir.X);
		}

		private void FireMissle(NPC target)
		{
			if (target != default)
			{
				Vector2 vel = ArcVelocityHelper.GetArcVel(Projectile.Center, target.Center, 0.25f, 300, 600);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<JetwelderJumperMissle>(), Projectile.damage * 2, Projectile.knockBack, Player.whoAmI, target.whoAmI);
			}
		}
	}

	public class JetwelderJumperMissle : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		private NPC victim = default;

		public override string Texture => AssetDirectory.SteampunkItem + Name;

		private Player Player => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grenade");
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.25f;
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			Projectile.frameCounter++;

			if (Projectile.frameCounter % 5 == 0)
				Projectile.frame++;

			Projectile.frame %= Main.projFrames[Projectile.type];

			if (Projectile.frame == 0)
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.7f);

			ManageCaches();
			ManageTrail();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail(Main.spriteBatch);
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var origin = new Vector2(tex.Width / 2, frameHeight / 2);

			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.velocity.Y < 0)
				return false;

			return base.CanHitNPC(target);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
				dust.velocity = Main.rand.NextVector2Circular(4, 4);
				dust.scale = Main.rand.NextFloat(1f, 1.5f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
			}

			for (int i = 0; i < 3; i++)
			{
				Vector2 velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2);
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, Player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), Projectile.damage, 0, Player.whoAmI, victim == default ? -1 : victim.whoAmI);

			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + vel * Main.rand.Next(70), 0, 0, ModContent.DustType<JetwelderDustTwo>());
				dust.velocity = vel * Main.rand.Next(7);
				dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			victim = target;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 4, factor =>
			{
				Color trailColor = Color.White;
				return trailColor * 0.3f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);

			trail?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	internal class JetwelderJumperExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private float Progress => 1 - Projectile.timeLeft / 5f;

		private float Radius => 75 * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rocket");
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI == Projectile.ai[0])
				return false;

			return base.CanHitNPC(target);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}