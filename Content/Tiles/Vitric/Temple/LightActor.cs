using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal class LightActor : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => ModContent.ProjectileType<LightActorDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, 0, SoundID.Tink, Color.White);
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		public override bool RightClick(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (Main.LocalPlayer.controlSmart)
			{
				if (Main.mouseLeft)
					tile.TileFrameX--;
				else
					tile.TileFrameY--;
			}
			else
			{
				if (Main.mouseLeft)
					tile.TileFrameX++;
				else
					tile.TileFrameY++;
			}

			return true;
		}
	}

	[SLRDebug]
	internal class LightActorItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public LightActorItem() : base("Light actor placer", "Debug item", "LightActor", 6) { }
	}

	internal class LightActorDummy : Dummy
	{
		public LightActorDummy() : base(ModContent.TileType<LightActor>(), 16, 16) { }

		public override void Update()
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Vector2 pos = Projectile.Center;
			int w = Parent.TileFrameY * 8; // The sprite is flipped so we have to flip this too
			int h = Parent.TileFrameX * 16;

			var target = new Rectangle((int)pos.X - w / 2, (int)pos.Y, w, h);

			if (Main.rand.NextBool(12))
				Dust.NewDust(target.TopLeft(), target.Width, target.Height, ModContent.DustType<Dusts.Aurora>(), 0, -Main.rand.NextFloat(2, 5), 0, new Color(100, 200, 255) * 0.75f, Main.rand.NextFloat(0.75f));

			for (int k = 0; k < h / 16; k++)
			{
				Lighting.AddLight(pos + Vector2.UnitY * k * 16, new Vector3(0.5f, 0.75f, 0.9f) * 0.5f);
			}

			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(Color lightColor)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

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

			Vector2 pos = Projectile.Center - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y + 8, Parent.TileFrameX * 16, Parent.TileFrameY * 16);
			spriteBatch.Draw(tex, target, null, color * 0.1f, 1.57f, new Vector2(0, tex.Height / 2f), 0, 0);

			var targetBack = new Rectangle((int)pos.X, (int)pos.Y + 8, Parent.TileFrameX * 16, Parent.TileFrameY * 48);
			spriteBatch.Draw(tex, targetBack, null, color2 * 0.075f, 1.57f, new Vector2(0, tex.Height / 2f), 0, 0);

			for (int k = 0; k < 3; k++)
			{
				int rand = ((k * 5824) ^ 129379123) % 100;
				var color3 = new Color(100, 150 + rand, 255 - rand)
				{
					A = 0
				};

				Vector2 pos2 = pos + Vector2.UnitX * (float)Math.Sin(Main.GameUpdateCount * 0.02f + (k ^ 168218)) * Parent.TileFrameY * 1.8f;
				var target2 = new Rectangle((int)pos2.X, (int)pos2.Y + 8, Parent.TileFrameX * 16, Parent.TileFrameY * (8 + (k ^ 978213) % 10));

				spriteBatch.Draw(tex, target2, null, color3 * 0.075f, 1.57f, new Vector2(0, tex.Height / 2f), 0, 0);
			}

			if (StarlightRiver.debugMode)
				Utils.DrawBorderString(spriteBatch, $"p: {Parent.TileFrameX}, {Parent.TileFrameY}", pos, Color.White);
		}
	}
}