using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class BossWindowDummy : Dummy
	{
		public float timer;

		private ParticleSystem particles = new("StarlightRiver/Assets/GUI/HolyBig", UpdateParticles);

		public BossWindowDummy() : base(TileType<BossWindow>(), 16, 16) { }

		public override void PostDraw(Color lightColor)
		{
			Vector2 dpos = Center - Main.screenPosition + Vector2.UnitY * 16;

			Texture2D frametex = Request<Texture2D>(AssetDirectory.OvergrowTile + "WindowFrame").Value;
			Texture2D glasstex = Request<Texture2D>(AssetDirectory.OvergrowTile + "WindowGlass").Value;

			Main.spriteBatch.Draw(glasstex, dpos, glasstex.Frame(), Color.White * 0.15f, 0, glasstex.Frame().Size() / 2, 1, 0, 0); //glass
			Main.spriteBatch.Draw(frametex, dpos, frametex.Frame(), new Color(255, 255, 200), 0, frametex.Frame().Size() / 2, 1, 0, 0); //frame
		}

		private static ParticleSystem.Update UpdateParticles => UpdateWindowParticles;

		private static void UpdateWindowParticles(Particle particle)
		{
			particle.Color = Color.White * (particle.Timer / 600f) * 0.6f;
			particle.Scale = particle.Timer / 600f * 0.6f;
			particle.Position = particle.StoredPosition + FindOffset(particle.StoredPosition, 0.35f) + particle.Velocity * (600 - particle.Timer) - Main.screenPosition;
			particle.Rotation += 0.1f;

			particle.Timer--;
		}

		public override void Update()
		{
			//Dust
			if (timer > 0 && timer < 359)
				Dust.NewDustPerfect(Center + Vector2.One.RotatedByRandom(6.28f) * 412, DustType<Dusts.Stone>());

			if (Main.rand.NextBool(4) && timer >= 360)
			{
				float rot = Main.rand.NextFloat(-1.5f, 1.5f);
				Dust.NewDustPerfect(Center + new Vector2(0, 1).RotatedBy(rot) * 500, DustType<Dusts.GoldSlowFade>(), (new Vector2(0, 1).RotatedBy(rot) + new Vector2(0, 1.6f)) * (0.1f + Math.Abs(rot / 5f)), 0, default, 0.23f + Math.Abs(rot / 5f));
			}

			//Screenshake
			CameraSystem.shake += (int)(359 - timer) / 175;

			//Lighting
			for (float k = 0; k <= 6.28f; k += 0.2f)
			{
				Lighting.AddLight(Center + Vector2.One.RotatedBy(k) * 23 * 16, new Vector3(1, 1, 0.7f) * 0.8f);
			}

			for (int k = 0; k < 6; k++)
			{
				if (timer > 0)
				{
					float bright = timer / 60f;
					if (bright > 1)
						bright = 1;

					Lighting.AddLight(Center + new Vector2(560 + k * 35, 150 + k * 80), new Vector3(1, 1, 0.7f) * bright);
					Lighting.AddLight(Center + new Vector2(-560 - k * 35, 150 + k * 80), new Vector3(1, 1, 0.7f) * bright);
				}

				if (timer > 60)
				{
					float bright = (timer - 60) / 150f;
					if (bright > 1)
						bright = 1;

					Lighting.AddLight(Center + new Vector2(450 + k * 15, 300 + k * 50), new Vector3(1, 1, 0.7f) * bright);
					Lighting.AddLight(Center + new Vector2(-450 - k * 15, 300 + k * 50), new Vector3(1, 1, 0.7f) * bright);
				}

				if (timer > 210)
				{
					float bright = (timer - 210) / 70f;
					if (bright > 1)
						bright = 1;

					Lighting.AddLight(Center + new Vector2(250 + k * 5, 350 + k * 40), new Vector3(1, 1, 0.7f) * bright);
					Lighting.AddLight(Center + new Vector2(-250 - k * 5, 350 + k * 40), new Vector3(1, 1, 0.7f) * bright);
				}

				if (timer > 280)
				{
					float bright = (timer - 280) / 50f;
					if (bright > 1)
						bright = 1;

					Lighting.AddLight(Center + new Vector2(40, 550 + k * 10), new Vector3(1, 1, 0.7f) * bright);
					Lighting.AddLight(Center + new Vector2(-40, 550 + k * 10), new Vector3(1, 1, 0.7f) * bright);
				}
			}

			if (StarlightWorld.HasFlag(WorldFlags.OvergrowBossFree) && timer <= 360)
				timer++;
		}

		private static Vector2 FindOffset(Vector2 basepos, float factor, bool noVertical = false)
		{
			Vector2 origin = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float x = (origin.X - basepos.X) * factor;
			float y = (origin.Y - basepos.Y) * factor * 0.4f;
			return new Vector2(x, noVertical ? 0 : y);
		}

		private Rectangle GetSource(float offset, Texture2D tex)
		{
			int x = tex.Width / 2 - 564;
			int y = tex.Height / 2 - 564;
			Vector2 pos = new Vector2(x, y) - FindOffset(Center, offset);
			return new Rectangle((int)pos.X, (int)pos.Y + 160, 1128, 1128);
		}

		public void DrawMoonlordLayer(SpriteBatch spriteBatch)
		{
			Vector2 pos = Center;
			Vector2 dpos = pos - Main.screenPosition;
			var target = new Rectangle((int)dpos.X - 564, (int)dpos.Y - 564, 1128, 1128);

			if (!Helpers.Helper.OnScreen(target))
				return;

			//background
			Texture2D backtex1 = Request<Texture2D>(AssetDirectory.OvergrowTile + "Window4").Value;
			spriteBatch.Draw(backtex1, target, GetSource(0, backtex1), Color.White, 0, Vector2.Zero, 0, 0);

			Texture2D backtex2 = Request<Texture2D>(AssetDirectory.OvergrowTile + "Window3").Value;
			spriteBatch.Draw(backtex2, target, GetSource(0.4f, backtex2), Color.White, 0, Vector2.Zero, 0, 0);

			Texture2D backtex3 = Request<Texture2D>(AssetDirectory.OvergrowTile + "Window2").Value;
			spriteBatch.Draw(backtex3, target, GetSource(0.3f, backtex3), Color.White, 0, Vector2.Zero, 0, 0);

			//godbeams
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			// Update + draw dusts
			particles.DrawParticles(spriteBatch);

			if (Main.rand.NextBool(10))
				particles.AddParticle(new Particle(Vector2.Zero, new Vector2(0, Main.rand.NextFloat(0.6f, 1.8f)), 0, 1, Color.White, 600, Center + new Vector2(Main.rand.Next(-350, 350), -580)));

			for (int k = -2; k < 3; k++)
			{
				Texture2D tex2 = Request<Texture2D>(AssetDirectory.OvergrowTile + "PitGlow").Value;
				float rot = (float)Main.time / 50 % 6.28f;
				float sin = (float)Math.Sin(rot + k);
				float sin2 = (float)Math.Sin(rot + k * 1.4f);
				float cos = (float)Math.Cos(rot + k * 1.8f);

				Vector2 beampos = dpos + FindOffset(pos, 0.4f + Math.Abs(k) * 0.05f, true) + new Vector2(k * 85 + (k % 2 == 0 ? sin : sin2) * 30, -220);
				var beamrect = new Rectangle((int)beampos.X - (int)(sin * 30), (int)beampos.Y + (int)(sin2 * 70), 90 + (int)(sin * 30), 700 + (int)(sin2 * 140));

				spriteBatch.Draw(tex2, beamrect, tex2.Frame(), new Color(255, 255, 200) * (1.4f + cos) * 0.9f, 0, tex2.Frame().Size() / 2, SpriteEffects.FlipVertically, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			for (int k = -9; k < 8; k++)// small waterfalls
			{
				Texture2D watertex = Request<Texture2D>(AssetDirectory.OvergrowTile + "Waterfall").Value;
				int frame = (int)Main.time % 16 / 2;
				spriteBatch.Draw(watertex, dpos + new Vector2(100, k * 64) + FindOffset(pos, 0.22f, true), new Rectangle(0, frame * 32, watertex.Width, 32), Color.White * 0.3f, 0, Vector2.Zero, 2, 0, 0);
			}

			//front row
			Texture2D backtex4 = Request<Texture2D>(AssetDirectory.OvergrowTile + "Window1").Value;
			spriteBatch.Draw(backtex4, target, GetSource(0.2f, backtex4), Color.White, 0, Vector2.Zero, 0, 0);

			for (int k = -6; k < 6; k++) //big waterfall
			{
				Texture2D watertex = Request<Texture2D>(AssetDirectory.OvergrowTile + "Waterfall").Value;
				int frame = (int)Main.time % 16 / 2;
				spriteBatch.Draw(watertex, dpos + new Vector2(300, k * 96) + FindOffset(pos, 0.1f, true), new Rectangle(0, frame * 32, watertex.Width, 32), Color.White * 0.3f, 0, Vector2.Zero, 3, 0, 0);
			}

			if (timer <= 360) //wall
			{
				Texture2D walltex = Request<Texture2D>(AssetDirectory.OvergrowTile + "Dor2").Value;
				Texture2D walltex2 = Request<Texture2D>(AssetDirectory.OvergrowTile + "Dor1").Value;
				var sourceRect = new Rectangle(0, 0, walltex.Width, walltex.Height - (int)(timer / 360 * 764));
				var sourceRect2 = new Rectangle(0, (int)(timer / 360 * 564), walltex2.Width, walltex2.Height - (int)(timer / 360 * 564));

				spriteBatch.Draw(walltex, dpos + new Vector2(0, 176 + timer / 360 * 764), sourceRect, new Color(255, 255, 200), 0, walltex.Frame().Size() / 2, 1, 0, 0); //frame
				spriteBatch.Draw(walltex2, dpos + new Vector2(0, -282 - timer / 360), sourceRect2, new Color(255, 255, 200), 0, walltex2.Frame().Size() / 2, 1, 0, 0); //frame
			}
		}

		public override void Unload()
		{
			particles = null;
		}
	}
}