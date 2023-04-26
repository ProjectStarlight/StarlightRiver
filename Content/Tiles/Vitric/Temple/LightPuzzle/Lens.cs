using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class Lens : Reflector
	{
		public override int DummyType => ModContent.ProjectileType<LensDummy>();

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
			if (!rotating)
			{
				Emit = 1;
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

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;

			Vector2 pos = Projectile.Center - Main.screenPosition + Vector2.UnitY * -48;

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

			Texture2D texMirror = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MirrorOver").Value;
			Main.spriteBatch.Draw(texMirror, Projectile.Center - Main.screenPosition, null, Color.White, Rotation - 3.14f - 1.57f / 2, texMirror.Size() / 2, 1, 0, 0);
		}
	}

	class LensItem : QuickTileItem
	{
		public LensItem() : base("Reflector Lens", "Debug Item", "Lens") { }
	}
}