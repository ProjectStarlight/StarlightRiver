using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class ThinkerMindMine : ModProjectile
	{
		public float pulse;
		public float pulseAccum;

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void SetDefaults()
		{
			Projectile.width = 150;
			Projectile.height = 150;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			Projectile.hide = true;
		}

		public override bool CanHitPlayer(Player target)
		{
			return Projectile.timeLeft <= 5;
		}

		public override void AI()
		{
			pulseAccum += 1 + (300 - Projectile.timeLeft) / 100f;
			pulse = pulseAccum * 0.0025f + 0.5f + MathF.Sin(pulseAccum * 0.1f) * (0.5f + pulseAccum * 0.0001f);

			Lighting.AddLight(Projectile.Center, new Vector3(1, 0.2f, 0.2f) * (1f - Projectile.timeLeft / 300f));

			if (Projectile.timeLeft == 20)
			{
				for (int k = 0; k < 3; k++)
				{
					Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.One.RotatedByRandom(6.28f) * 2, Main.rand.Next(392, 396));
				}

				for (int k = 0; k < 80; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, default, 1 + Main.rand.NextFloat(2));
				}

				Helpers.Helper.PlayPitched("Impacts/GoreHeavy", 0.1f, Main.rand.NextFloat(-0.5f, 0f), Projectile.Center);
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft > 20)
			{
				float opacity = 1f;
				if (Projectile.timeLeft > 270)
					opacity = 1 - (Projectile.timeLeft - 270) / 30f;

				Texture2D tex = Assets.Bosses.BrainRedux.ThinkerMindMine.Value;

				float pulse2 = pulseAccum * 0.0025f + 0.5f + MathF.Sin(pulseAccum * 0.12f + 0.5f) * (0.5f + pulseAccum * 0.0001f);
				Vector2 scale = new Vector2(1f + pulse * 0.1f, 1f + pulse2 * 0.1f) * opacity;

				Vector2 scaleAdj = tex.Size() - tex.Size() * scale;
				LightingBufferRenderer.DrawWithLighting(tex, Projectile.position - Main.screenPosition + Vector2.One * 75 - tex.Size() / 2f + scaleAdj / 2, null, Color.White * opacity, 0f, Vector2.Zero, scale);

				if (Projectile.timeLeft < 90)
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 40, 40, 0) * MathF.Sin(Projectile.timeLeft / 90f * 6.28f) * 0.5f * opacity, 0f, tex.Size() / 2f, scale, 0, 0);
			}
			else
			{
				Texture2D flash = Assets.StarTexture.Value;
				Texture2D flash2 = Assets.Keys.GlowAlpha.Value;
				Main.spriteBatch.Draw(flash2, Projectile.Center - Main.screenPosition, null, new Color(255, 70, 70, 0) * (Projectile.timeLeft / 20f), 0f, flash2.Size() / 2f, 3f - Projectile.timeLeft / 20f * 9, 0, 0);
				Main.spriteBatch.Draw(flash, Projectile.Center - Main.screenPosition, null, new Color(255, 120, 120, 0) * (Projectile.timeLeft / 20f), 0f, flash.Size() / 2f, 3f - Projectile.timeLeft / 20f * 3, 0, 0);
			}

			return false;
		}
	}

	internal class FakeMindMine : ModProjectile
	{
		public float pulse;
		public float pulseAccum;

		public override string Texture => AssetDirectory.BrainRedux + "ThinkerMindMine";

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawActiveMines;
		}

		public override void SetDefaults()
		{
			Projectile.width = 150;
			Projectile.height = 150;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			Projectile.hide = true;
		}

		public override bool CanHitPlayer(Player target)
		{
			return false;
		}

		public override void AI()
		{
			pulseAccum += 1 + (300 - Projectile.timeLeft) / 100f;
			pulse = pulseAccum * 0.0025f + 0.5f + MathF.Sin(pulseAccum * 0.1f) * (0.5f + pulseAccum * 0.0001f);

			Lighting.AddLight(Projectile.Center, new Vector3(1, 0.2f, 0.2f) * (1f - Projectile.timeLeft / 300f));
		}

		private void DrawActiveMines(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == ModContent.ProjectileType<FakeMindMine>())
				{
					float opacity = proj.timeLeft > 30 ? 1f : proj.timeLeft / 30f;
					if (proj.timeLeft > 270)
						opacity = 1 - (proj.timeLeft - 270) / 30f;

					float pulse = 0.25f * MathF.Sin(Main.GameUpdateCount * 0.25f) * 0.25f;
					float pulse2 = 0.25f * MathF.Sin(0.5f + Main.GameUpdateCount * 0.25f) * 0.25f;

					Texture2D tex = Assets.Misc.Exclaim.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, Color.White * opacity, 0, tex.Size() / 2f, 1f + pulse, 0, 0);

					Texture2D arrow = Assets.Bosses.BrainRedux.TellArrow.Value;

					Color color = new Color(255, 60, 60, 255) * opacity;

					for (int k = 0; k < 4; k++)
					{
						float rot = k / 4f * 6.28f;
						batch.Draw(arrow, proj.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * -(54 + pulse * 80), null, color * (0.75f + pulse), rot, arrow.Size() / 2f, 1f + pulse, 0, 0);
						batch.Draw(arrow, proj.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * -(74 + pulse2 * 80), null, color * (0.75f + pulse2), rot, arrow.Size() / 2f, 1f + pulse2, 0, 0);
					}
				}
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float opacity = Projectile.timeLeft > 60 ? 1f : Projectile.timeLeft / 60f;
			if (Projectile.timeLeft > 270)
				opacity = 1 - (Projectile.timeLeft - 270) / 30f;

			Texture2D tex = Assets.Bosses.BrainRedux.ThinkerMindMine.Value;

			float pulse2 = pulseAccum * 0.0025f + 0.5f + MathF.Sin(pulseAccum * 0.12f + 0.5f) * (0.5f + pulseAccum * 0.0001f);
			Vector2 scale = new Vector2(1f + pulse * 0.1f, 1f + pulse2 * 0.1f) * opacity;

			Vector2 scaleAdj = tex.Size() - tex.Size() * scale;
			LightingBufferRenderer.DrawWithLighting(tex, Projectile.position - Main.screenPosition + Vector2.One * 75 - tex.Size() / 2f + scaleAdj / 2, null, Color.White * opacity, 0, Vector2.Zero, scale);

			if (Projectile.timeLeft < 90)
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 40, 40, 0) * MathF.Sin(Projectile.timeLeft / 90f * 6.28f) * 0.5f * opacity, 0f, tex.Size() / 2f, scale, 0, 0);

			return false;
		}
	}
}