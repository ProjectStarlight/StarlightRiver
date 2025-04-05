using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class Lens : Reflector
	{
		public override int DummyType => DummySystem.DummyType<LensDummy>();

		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Dirt, SoundID.Dig, new Color(1, 1, 1));
		}

		public override bool RightClick(int i, int j)
		{
			if (StarlightRiver.debugMode)
				return base.RightClick(i, j);

			return false;
		}
	}

	class LensDummy : ReflectorDummy
	{

		public LensDummy() : base() { validType = ModContent.TileType<Lens>(); }

		public override void Update()
		{
			if (!rotating && Main.GameUpdateCount % 60 == 0)
			{
				emitting = 1;
				DeactivateDownstream();
				FindEndpoint();
			}

			base.Update();
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			var color = new Color(100, 220, 255)
			{
				A = 0
			};

			var color2 = new Color(100, 130, 255)
			{
				A = 0
			};

			Texture2D tex = Assets.GlowTrail.Value;

			Vector2 pos = Center - Main.screenPosition + Vector2.UnitY * -48;

			for (int k = 0; k < 3; k++)
			{
				int rand = ((k * 5824) ^ 129379123) % 100;
				var color3 = new Color(100, 150 + rand, 255 - rand)
				{
					A = 0
				};

				float sin = (float)Math.Sin(Main.GameUpdateCount * 0.02f + (k ^ 168218));

				Vector2 pos2 = pos + new Vector2(sin * 6 * 1.8f, 48);
				var target2 = new Rectangle((int)pos2.X, (int)pos2.Y - 8, 3 * 16, 13 * (6 + (k ^ 978213) % 5));

				spriteBatch.Draw(tex, target2, null, color3 * 0.15f, 3.14f - (pos2 - pos).ToRotation(), new Vector2(tex.Width, tex.Height / 2f), 0, 0);
			}

			Texture2D texMirror = Assets.Tiles.Vitric.MirrorOver.Value;
			Main.spriteBatch.Draw(texMirror, Center - Main.screenPosition, null, lightColor, rotation - 3.14f - 1.57f / 2, texMirror.Size() / 2, 1, 0, 0);

			//Laser
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 20f); //Just a copy/paste of the boss laser. Need to tune this later
				Color color2 = new Color(100, 200 + sin, 255, 0) * 0.65f;

				Texture2D texBeam = Assets.Misc.BeamCore.Value;
				Texture2D texBeam2 = Assets.Misc.BeamCore.Value;

				var origin = new Vector2(0, texBeam.Height / 2);
				var origin2 = new Vector2(0, texBeam2.Height / 2);

				Effect effect = ShaderLoader.GetShader("GlowingDust").Value;

				if (effect != null)
				{
					effect.Parameters["uColor"].SetValue(color2.ToVector3() * 0.35f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect);

					float height = texBeam.Height / 10f;
					int width = (int)(Center - endPoint).Length();

					Vector2 pos = Center - Main.screenPosition;

					var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 2.75f));
					var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 4f));

					var source = new Rectangle((int)(Main.GameUpdateCount / 140f * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
					var source2 = new Rectangle((int)(Main.GameUpdateCount / 80f * -texBeam2.Width), 0, width, texBeam2.Height);

					Main.spriteBatch.Draw(texBeam, target, source, color2, rotation, origin, 0, 0);
					Main.spriteBatch.Draw(texBeam2, target2, source2, color2 * 0.5f, rotation, origin2, 0, 0);

					for (int i = 0; i < width; i += 10)
					{
						Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rotation) * i + Main.screenPosition, color2.ToVector3() * height * 0.010f);
					}

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null);

					Texture2D impactTex = Assets.Masks.GlowSoftAlpha.Value;
					Texture2D impactTex2 = Assets.GUI.ItemGlow.Value;
					Texture2D glowTex = Assets.GlowTrail.Value;

					target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 4.5f));
					Main.spriteBatch.Draw(glowTex, target, source, color2 * 0.75f, rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

					Main.spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color2 * (height * 0.024f), 0, impactTex.Size() / 2, 2.8f, 0, 0);
					Main.spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 0.5f * (height * 0.1f), StarlightWorld.visualTimer * 2, impactTex2.Size() / 2, 0.2f, 0, 0);
					Main.spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 1.5f * (height * 0.1f), StarlightWorld.visualTimer * 2.2f, impactTex2.Size() / 2, 0.1f, 0, 0);
				}
			});
		}
	}

	[SLRDebug]
	class LensItem : QuickTileItem
	{
		public LensItem() : base("Reflector Lens", "{{Debug}} Item", "Lens") { }
	}
}