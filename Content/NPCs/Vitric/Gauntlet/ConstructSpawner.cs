﻿using StarlightRiver.Content.Dusts;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	public class ConstructSpawner : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		public bool gravity = true;

		public Player nearestPlayer = default;

		public NPC fakeNPC;

		public ref float NPCType => ref Projectile.ai[0];
		public ref float Timer => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			if (fakeNPC is null)
			{
				fakeNPC = new NPC();
				fakeNPC.SetDefaults((int)NPCType);
			}

			nearestPlayer = Main.player.Where(n => n.active && !n.dead).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Timer > 1)
			{
				Timer++;
			}
			else //While being thrown out
			{
				if (gravity)
					Projectile.velocity.Y += 0.1f;

				if (Main.rand.NextBool(2) && Main.netMode != NetmodeID.Server)
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), ModContent.DustType<Cinder>(), Vector2.Zero, 0, new Color(255, 170, 100), 0.65f);

				return;
			}

			if (Timer < 50 && Main.netMode != NetmodeID.Server)
			{
				Vector2 cinderPos = Projectile.Top + Main.rand.NextVector2Circular(40, 40);
				Vector2 vel = -Vector2.UnitY.RotatedBy(cinderPos.AngleTo(Projectile.Center)) * Main.rand.NextFloat(-2, 2);
				var cinder = Dust.NewDustPerfect(cinderPos, ModContent.DustType<Cinder>(), vel, 0, Bosses.GlassMiniboss.Glassweaver.GlowDustOrange, 0.8f);
				cinder.customData = Projectile.Center + Main.rand.NextVector2Circular(40, 40);
			}

			if (Timer > 70)
			{
				SpawnNPC();
				Projectile.Kill();
			}
		}

		public virtual void SpawnNPC()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				NPC.NewNPC(Entity.GetSource_Misc("SLR:GlassGauntlet"), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCType);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity *= 0;
			Timer = 2;
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (fakeNPC is null)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>((fakeNPC.ModNPC as VitricConstructNPC).PreviewTexturePath).Value;

			Effect trailEffect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			trailEffect.Parameters["time"].SetValue(Projectile.timeLeft * -0.04f);
			trailEffect.Parameters["repeats"].SetValue(1);
			trailEffect.Parameters["transformMatrix"].SetValue(world * view * projection);
			trailEffect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(trailEffect);

			Texture2D glowTex = Assets.Keys.GlowAlpha.Value;
			var color = new Color(255, 160, 100, 0);

			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * (1 - Timer / 25f), 0, glowTex.Size() / 2, Timer * 0.1f, 0, 0);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * (1 - Timer / 25f), 0, glowTex.Size() / 2, Timer * 0.05f, 0, 0);

			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * ((25 - Timer) / 25f), 0, glowTex.Size() / 2, 0.75f, 0, 0);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * 2 * ((25 - Timer) / 25f), 0, glowTex.Size() / 2, 0.25f, 0, 0);

			Effect effect = Filters.Scene["MoltenForm"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(Assets.Bosses.VitricBoss.ShieldMap.Value);
			effect.Parameters["uTime"].SetValue(Timer / 70f * 2);
			effect.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, fakeNPC.frame.Width, fakeNPC.frame.Height));
			effect.Parameters["texSize"].SetValue(tex.Size());

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			SpriteEffects spriteEffects = SpriteEffects.None;
			Vector2 drawOffset = (fakeNPC.ModNPC as VitricConstructNPC).PreviewOffset;

			if (nearestPlayer is null)
				return false;

			if (nearestPlayer.Center.X < Projectile.Center.X)
			{
				drawOffset.X *= -1;
				spriteEffects = SpriteEffects.FlipHorizontally;
			}

			Main.spriteBatch.Draw(tex, drawOffset + Projectile.Center - Main.screenPosition, null, Color.White, 0, new Vector2(tex.Width / 2, tex.Height - 5), 1, spriteEffects, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => 24, factor => new Color(255, 200, 165) * factor.X);

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}