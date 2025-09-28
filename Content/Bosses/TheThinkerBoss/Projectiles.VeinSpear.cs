using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class VeinSpear : ModProjectile
	{
		public bool hit;
		public int hitTime;

		public int ThinkerWhoAmI
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public ref float Lifetime => ref Projectile.ai[1];

		public NPC Thinker => Main.npc[ThinkerWhoAmI];

		public override string Texture => AssetDirectory.TheThinkerBoss + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vein Spear");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 99999;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Lifetime == 0)
				Lifetime = 300; // Enforce this as the default

			if (Lifetime < 60)
				Lifetime = 60; // Enforce this as the minimum

			if (Projectile.timeLeft >= 90000)
				Projectile.timeLeft = (int)Lifetime;

			if (Projectile.timeLeft == (int)Lifetime - 30)
				Projectile.velocity *= 50f;

			if (Thinker is null)
				return;

			if (Vector2.Distance(Thinker.Center, Projectile.Center) > 32)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (Helpers.CollisionHelper.CheckLinearCollision(Thinker.Center, Projectile.Center, player.Hitbox, out Vector2 collision))
					{
						int mult = Main.masterMode ? 6 : Main.expertMode ? 4 : 1;
						player.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.StarlightRiver.Deaths.ThinkerVein", player.name)), Projectile.damage * mult, 0);
						BuffInflictor.Inflict<Neurosis>(player, Main.masterMode ? 18000 : Main.expertMode ? 3000 : 1500);
					}
				}
			}


			float opacity = 1f;

			if (Projectile.timeLeft <= 30)
				opacity = Projectile.timeLeft / 30f;

			for (int k = 0; k < Vector2.Distance(Thinker.Center, Projectile.Center); k += 24)
			{
				Lighting.AddLight(Thinker.Center + Thinker.Center.DirectionTo(Projectile.Center) * k, Vector3.One * 0.5f * opacity);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.Bleeding, 300);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!hit)
			{
				hit = true;
				hitTime = (int)Lifetime - Projectile.timeLeft;
				Projectile.velocity *= 0;
				Helpers.SoundHelper.PlayPitched("Impacts/StabFleshy", 1f, -0.5f, Projectile.Center);
			}

			return false;
		}

		private void DrawAura(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Projectile.type)
				{
					var mp = proj.ModProjectile as VeinSpear;
					SpriteBatch spriteBatch = Main.spriteBatch;

					Texture2D glow = Assets.GlowTrailNoEnd.Value;

					float opacity = 1f;

					if (proj.timeLeft <= 30)
						opacity = proj.timeLeft / 30f;

					Color glowColor = Color.White * opacity;
					glowColor.A = 0;

					var gSource = new Rectangle(0, 0, glow.Width, glow.Height);
					var gTarget = new Rectangle((int)(proj.Center.X - Main.screenPosition.X), (int)(proj.Center.Y - Main.screenPosition.Y), (int)Vector2.Distance(mp.Thinker.Center, proj.Center), 250);
					var gOrigin = new Vector2(0, glow.Height / 2f);

					spriteBatch.Draw(glow, gTarget, gSource, glowColor, (proj.Center - mp.Thinker.Center).ToRotation() - 3.14f, gOrigin, 0, 0);
				}
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			//behindNPCs.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D chainTex = Assets.Bosses.TheThinkerBoss.TendrilDebris.Value;
			Texture2D glow = Assets.GlowTrailNoEnd.Value;

			float opacity = 1f;

			if (Projectile.timeLeft <= 30)
				opacity = Projectile.timeLeft / 30f;

			Color glowColor = new Color(180, 60, 90) * opacity * 0.25f;
			glowColor.A = 0;

			var gSource = new Rectangle(0, 0, glow.Width, glow.Height);
			var gTarget = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), (int)Vector2.Distance(Thinker.Center, Projectile.Center), 100);
			var gOrigin = new Vector2(0, glow.Height / 2f);

			spriteBatch.Draw(glow, gTarget, gSource, glowColor, (Projectile.Center - Thinker.Center).ToRotation() - 3.14f, gOrigin, 0, 0);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("GestaltLine").Value;

				if (effect != null)
				{
					effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);
					effect.Parameters["u_speed"].SetValue(1f);
					effect.Parameters["color1"].SetValue(new Vector3(0.3f, 0.2f, 0.1f) * opacity);
					effect.Parameters["color2"].SetValue(new Vector3(0.3f, 0.2f, 0.1f) * opacity);
					effect.Parameters["color3"].SetValue(new Vector3(0.3f, 0.2f, 0.1f) * opacity);

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, Main.Rasterizer, effect);

					Texture2D tex = Assets.Misc.Line.Value;

					Vector2 lastpos = Thinker.Center;
					Vector2 pos = Projectile.Center;
					float dist = Vector2.Distance(pos, lastpos);
					float rot = pos.DirectionTo(lastpos).ToRotation();

					var target = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)dist, 60);
					var color = Color.Lerp(new Color(255, 160, 100, 150), new Color(255, 100, 220, 150), 0.5f + MathF.Sin(Main.GameUpdateCount / 4f) * 0.5f);

					spriteBatch.Draw(tex, target, null, color, rot, new Vector2(0, tex.Height / 2f), 0, 0);

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default);
				}
			});

			if (hit)
			{
				Random random = new Random(Projectile.whoAmI);
				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(Thinker.Center, Projectile.Center) / 24))
				{
					float timer = Lifetime - Projectile.timeLeft - hitTime;
					float maxFade = random.Next(15, 30);
					float fade = Math.Min(1f, (timer - 25 + k * 25) / maxFade);

					Vector2 pos = Vector2.Lerp(Projectile.Center, Thinker.Center, k) - Main.screenPosition;
					pos += Vector2.One.RotatedBy(random.NextSingle() * 6.28f) * random.NextSingle() * (8 * (1 + k) + Eases.BezierEase(1f - fade) * 30);
					pos += Vector2.One.RotatedBy(Main.GameUpdateCount * 0.02f + random.NextSingle() * 6.28f) * 4;
					Rectangle frame = new(26 * random.Next(11), 0, 26, 28);
					float rotation = random.NextSingle() * 6.28f;

					var color = Lighting.GetColor(((pos + Main.screenPosition) / 16).ToPoint());

					spriteBatch.Draw(chainTex, pos, frame, color * opacity * fade, rotation, new Vector2(13, 14), 1, 0, 0);
				}
			}

			if (Projectile.timeLeft > Lifetime - 30)
			{
				Texture2D tell = Assets.GlowTrailNoEnd.Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), 500, 24);
				var origin = new Vector2(0, 12);
				Color color = new Color(255, 255, 255) * (float)Math.Sin((Projectile.timeLeft - (Lifetime - 30)) / 30f * 3.14f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, Projectile.rotation, origin, 0, 0);
			}

			return false;
		}
	}
}