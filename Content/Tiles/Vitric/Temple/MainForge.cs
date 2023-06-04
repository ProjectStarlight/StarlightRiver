using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class MainForge : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<MainForgeDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 19, 15, ModContent.DustType<Dusts.Air>(), SoundID.Shatter, false, Color.Black, mapName: "Vitric Forgeheart");
			Main.tileLighted[Type] = true;
		}
	}

	class MainForgeDummy : Dummy, IHintable
	{
		public float power = 0;

		public int timer;

		public MainForgeDummy() : base(ModContent.TileType<MainForge>(), 19 * 16, 15 * 16) { }

		public override void SafeSetDefaults()
		{
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override void Update()
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			float puzzleProg = LightPuzzle.LightPuzzleHandler.solvedPoints;

			if (power < puzzleProg - 0.01f)
				power += 0.01f;
			else if (power > puzzleProg + 0.01f)
				power -= 0.01f;

			if (LightPuzzle.LightPuzzleHandler.Solved)
				timer++;

			Vector2 pos = Projectile.Center + new Vector2(0, 88);

			Lighting.AddLight(pos, new Vector3(1, 0.8f, 0.5f) * HammerFunction(timer * 0.01f) * 0.02f);

			float sin = 0.75f + (float)Math.Sin(timer * 0.025f) * 0.25f;

			Lighting.AddLight(pos + new Vector2(82, 16), new Vector3(1, 0.7f, 0.5f) * sin);
			Lighting.AddLight(pos + new Vector2(-82, 16), new Vector3(1, 0.7f, 0.5f) * sin);

			var color = new Color(255, Main.rand.Next(100, 255), 50);
			color = Color.Lerp(new Color(100, 220, 255) * 0.25f, color, puzzleProg);

			Dust.NewDustPerfect(pos + new Vector2(Main.rand.NextFloat(-24, 24), -208), ModContent.DustType<Dusts.Cinder>(), new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, 0)), 0, color, Main.rand.NextFloat(2) * (0.5f + power * 0.5f));

			if (timer % 100 == 1)
			{
				for (float k = 0; k <= 0.3f; k += 0.01f)
				{
					Vector2 vel = new Vector2(1, 0).RotatedBy(-k) * Main.rand.NextFloat(2);

					if (Main.rand.NextBool(2))
						vel = new Vector2(-1, 0).RotatedBy(k) * Main.rand.NextFloat(2);

					float rot = Main.rand.NextFloat(6.28f);
					float speed = Main.rand.NextFloat(15);
					float scale = Main.rand.NextFloat(0.5f, 1f);

					var d = Dust.NewDustPerfect(pos + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.GlassAttracted>(), Vector2.One.RotatedBy(rot) * speed, 0, default, scale);
					d.customData = pos;

					d = Dust.NewDustPerfect(pos + Vector2.One.RotatedBy(rot) * 16, ModContent.DustType<Dusts.GlassAttractedGlow>(), Vector2.One.RotatedBy(rot) * speed, 0, default, scale);
					d.customData = pos;

					Dust.NewDustPerfect(pos + new Vector2(vel.X * 0.5f, 5), ModContent.DustType<Dusts.Cinder>(), vel * 1.5f, 0, new Color(255, Main.rand.Next(100, 200), 50), Main.rand.NextFloat());
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Tink with { Pitch = 0.5f }, pos);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70 with { Pitch = 1.5f }, pos);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 1.5f }, pos);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return false;

			SpriteBatch spriteBatch = Main.spriteBatch;

			DrawLaser(spriteBatch);

			Vector2 pos = Projectile.position - Main.screenPosition;

			var bgTarget = new Rectangle(80, 48, 128, 200);
			bgTarget.Offset(pos.ToPoint());

			TempleTileUtils.DrawBackground(spriteBatch, bgTarget);

			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MainForgeOver").Value;
			Texture2D texHammer = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MainForgeHammer").Value;

			Vector2 pos = Projectile.position - Main.screenPosition;

			var offset = new Vector2(0, HammerFunction(timer * 0.01f));

			spriteBatch.End();

			LightingBufferRenderer.DrawWithLighting(pos + offset, texHammer, Color.White);
			LightingBufferRenderer.DrawWithLighting(pos, tex, Color.White);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MainForgeGlow").Value;
			spriteBatch.Draw(texGlow, pos, Color.White);
		}

		public void DrawLaser(SpriteBatch spriteBatch)
		{
			Texture2D texBeam = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;
			Texture2D texBeam2 = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamTrail").Value;
			Texture2D texDark = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "GradientBlack").Value;

			var origin = new Vector2(0, texBeam.Height / 2);
			var origin2 = new Vector2(0, texBeam2.Height / 2);

			float puzzleProg = power;

			Vector2 centerPos = Projectile.position + new Vector2(Projectile.width / 2f, 0);
			Vector2 endpoint = centerPos + new Vector2(0, 200);
			float rot = (centerPos - endpoint).ToRotation();
			var color = Color.Lerp(Color.Orange, new Color(255, 110, 0), 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.5f);

			color = Color.Lerp(new Color(100, 140, 170) * 0.25f, color, puzzleProg);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

			effect.Parameters["uColor"].SetValue(color.ToVector3());

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

			float height = texBeam.Height / 2f * (0.5f + puzzleProg * 0.5f);
			int width = (int)(centerPos - endpoint).Length();

			Vector2 pos = centerPos - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
			var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

			var source = new Rectangle(texBeam.Width + (int)Main.GameUpdateCount * (int)(3 * power + 1), 0, texBeam.Width, texBeam.Height);
			var source2 = new Rectangle(texBeam2.Width + (int)Main.GameUpdateCount * (int)(3 * power + 1), 0, texBeam2.Width, texBeam2.Height);

			spriteBatch.Draw(texBeam, target, source, color, rot, origin, 0, 0);
			spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, rot, origin2, 0, 0);

			source = new Rectangle(texBeam.Width + (int)Main.GameUpdateCount * (int)(5 * power + 1), 0, texBeam.Width, texBeam.Height);
			source2 = new Rectangle(texBeam2.Width + (int)Main.GameUpdateCount * (int)(5 * power + 1), 0, texBeam2.Width, texBeam2.Height);

			spriteBatch.Draw(texBeam, target, source, color, rot, origin, SpriteEffects.FlipVertically, 0);
			spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, rot, origin2, SpriteEffects.FlipVertically, 0);

			for (int i = 0; i < width; i += 10)
			{
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rot) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

			color.A = 0;
			spriteBatch.Draw(glowTex, target, source, color * 0.95f, rot, new Vector2(0, glowTex.Height / 2), 0, 0);
		}

		public float HammerFunction(float input)
		{
			input %= 1;

			if (input < 0.6f)
				return 52 - input * 100;
			else if (input < 0.8f)
				return 52 - 60;
			else
				return 52 - 60 + Helpers.Helper.BezierEase((input - 0.8f) / 0.2f) * 60;
		}
		public string GetHint()
		{
			return "An ancient forge, powered by concentrated light...";
		}
	}

	[SLRDebug]
	class MainForgeItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public MainForgeItem() : base("Main forge", "Debug item", "MainForge") { }
	}
}
