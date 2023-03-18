using StarlightRiver.Content.Bosses.SquidBoss;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Menus
{
	internal class SquidMenu : ModMenu
	{
		public NPC fakeBoss;

		float VisualTimerA;
		float VisualTimerB;
		readonly ParticleSystem bubblesSystem = new(AssetDirectory.SquidBoss + "Bubble", UpdateBubblesBody);

		public override string DisplayName => "Auroracle";
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/PermafrostPassive");

		private static void UpdateBubblesBody(Particle particle)
		{
			particle.Timer--;

			particle.StoredPosition.Y += particle.Velocity.Y;
			particle.StoredPosition.X += (float)Math.Sin(StarlightWorld.visualTimer + particle.Velocity.X) * 0.45f;
			particle.Position = particle.StoredPosition - Main.screenPosition;
			particle.Alpha = particle.Timer < 70 ? particle.Timer / 70f : particle.Timer > 630 ? 1 - (particle.Timer - 630) / 70f : 1;
		}

		public override void Update(bool isOnTitleScreen)
		{
			VisualTimerA += 0.04f;
			VisualTimerB += 0.01f;

			Main.time = 1000;
			Main.dayTime = false;

			Vector2 center = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

			if (fakeBoss is null)
			{
				fakeBoss = new NPC();
				fakeBoss.SetDefaults(NPCType<SquidBoss>());
				fakeBoss.Center = new Vector2(center.X, Main.screenHeight + 500);
				(fakeBoss.ModNPC as SquidBoss).QuickSetup();
			}

			(fakeBoss.ModNPC as SquidBoss).tentacles.ForEach(n => n.ai[1]++);
			(fakeBoss.ModNPC as SquidBoss).GlobalTimer++;
			(fakeBoss.ModNPC as SquidBoss).Opacity = 0f;
			(fakeBoss.ModNPC as SquidBoss).OpaqueJelly = true;
			(fakeBoss.ModNPC as SquidBoss).Animate(6, 0, 8);

			fakeBoss.Center += (Main.MouseScreen + new Vector2(0, -50) - fakeBoss.Center) * 0.01f;
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			logoScale = 0.8f;

			Main.screenPosition = Vector2.Zero;

			Vector2 center = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

			Texture2D layer0 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background0").Value;
			Texture2D layer1 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background1").Value;
			Texture2D layer2 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background2").Value;

			Vector2 pos = center;
			Vector2 dpos = pos - Main.screenPosition;
			var target = new Rectangle((int)dpos.X - 630, (int)dpos.Y - 595 + 60, 1260, 1020);
			var color = new Color(140, 150, 190);

			spriteBatch.Draw(layer0, target, GetSource(0.2f, layer0), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(layer1, target, GetSource(0.15f, layer1), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(layer2, target, GetSource(0.1f, layer2), color, 0, Vector2.Zero, 0, 0);

			target.Y -= 1100;
			target.X += 64;
			target.Width -= 128;

			spriteBatch.Draw(layer0, target, GetSource(0.2f, layer0), color, 0, Vector2.Zero, 0, 0);
			target.Y -= 100;
			spriteBatch.Draw(layer1, target, GetSource(0.15f, layer1), color, 0, Vector2.Zero, 0, 0);
			target.Y -= 240;
			spriteBatch.Draw(layer2, target, GetSource(0.1f, layer2), color, 0, Vector2.Zero, 0, 0);

			(fakeBoss.ModNPC as SquidBoss).tentacles.ForEach(n => (n.ModNPC as Tentacle).DrawUnderWater(spriteBatch, 0));
			(fakeBoss.ModNPC as SquidBoss).DrawUnderWater(spriteBatch, 0);

			bubblesSystem.DrawParticles(spriteBatch);

			if (Main.rand.NextBool(4))
				bubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(0.6f, 1.2f)), 0, Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.2f, 0.4f), 700, pos + new Vector2(Main.rand.Next(-600, 600), 500), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

			if (Main.rand.NextBool(4))
				bubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(0.6f, 1.2f)), 0, Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.2f, 0.4f), 700, pos + new Vector2(Main.rand.Next(-600, 600), Main.rand.Next(-1200, -600)), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

			if (Main.rand.NextBool(20))
				bubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(1.6f, 2.2f)), 0, Main.rand.NextFloat(1.0f, 1.4f), Color.White * Main.rand.NextFloat(0.4f, 0.5f), 700, pos + new Vector2(Main.rand.Next(-600, 600), 500), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

			if (Main.rand.NextBool(20))
				bubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(1.6f, 2.2f)), 0, Main.rand.NextFloat(1.0f, 1.4f), Color.White * Main.rand.NextFloat(0.4f, 0.5f), 700, pos + new Vector2(Main.rand.Next(-600, 600), Main.rand.Next(-1200, -600)), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

			Texture2D backdrop = Request<Texture2D>(AssetDirectory.SquidBoss + "Window").Value;
			spriteBatch.Draw(backdrop, center - backdrop.Size() / 2 + new Vector2(0, -886) - Main.screenPosition, new Color(45, 45, 60));

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

			Texture2D dome = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowDome").Value;
			spriteBatch.Draw(dome, center - dome.Size() / 2 + Vector2.UnitY * -886 - Main.screenPosition, null, Color.White * 0.325f, 0, Vector2.Zero, 1, 0, 0);

			Texture2D glass = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowIn").Value;
			Texture2D glass2 = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowInGlow").Value;
			spriteBatch.Draw(glass, center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.325f, 0, glass.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(glass2, center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.2f, 0, glass.Size() / 2, 1, 0, 0);

			Texture2D ray = Request<Texture2D>(AssetDirectory.SquidBoss + "Godray").Value;

			for (int k = 0; k < 4; k++)
			{
				var lightColor = new Color(120, 210, 255);
				spriteBatch.Draw(ray, center + new Vector2(450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.9f + (float)Math.Sin((VisualTimerB + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);
				spriteBatch.Draw(ray, center + new Vector2(-450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.45f + (float)Math.Sin((VisualTimerB + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);

				spriteBatch.Draw(ray, center + new Vector2(0, -450) - Main.screenPosition, null, lightColor * 0.5f, 0.68f + (float)Math.Sin(VisualTimerB * 2 + k / 4f * 6.28f) * 0.13f, Vector2.Zero, 1.9f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			return true;
		}

		private Rectangle GetSource(float offset, Texture2D tex)
		{
			int x = tex.Width / 2 - 640;
			int y = tex.Height / 2 - 595;
			var pos = new Vector2(x, y);
			return new Rectangle((int)pos.X, (int)pos.Y, 1280, 1190);
		}

		public override void Unload()
		{

		}
	}
}
