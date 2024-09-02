﻿using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	class WindowSmall : DummyTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "WindowSmall";

		public override int DummyType => DummySystem.DummyType<WindowSmallDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 6, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(255, 220, 0));
		}

		public override bool SpawnConditions(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}
	}

	[SLRDebug]
	class WindowSmallItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + "WindowSmall";

		public WindowSmallItem() : base("Smol Window", "{{Debug}} Item", "WindowSmall", 1) { }
	}

	class WindowSmallDummy : Dummy, IDrawAdditive
	{
		public float timer;

		public WindowSmallDummy() : base(TileType<WindowSmall>(), 4 * 16, 6 * 16) { }

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Tiles.Overgrow.Window3.Value;
			Texture2D tex2 = Assets.Tiles.Overgrow.WindowSmall.Value;

			var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), 4 * 16, 6 * 16);

			float offX = (Main.screenPosition.X + Main.screenWidth / 2 - Center.X) * -0.14f;
			float offY = (Main.screenPosition.Y + Main.screenHeight / 2 - Center.Y) * -0.14f;
			var source = new Rectangle((int)(position.X % tex.Width) + (int)offX, (int)(position.Y % tex.Height) + (int)offY, 4 * 16, 6 * 16);

			Main.spriteBatch.Draw(tex, target, source, Color.White);
			Main.spriteBatch.Draw(tex2, target, tex2.Frame(), lightColor);
		}

		public override void Update()
		{
			timer += 0.02f;
			Lighting.AddLight(Center + new Vector2(0, 32), new Vector3(1, 1f, 0.6f));

			if (Main.rand.NextBool(20))
			{
				Vector2 off = Vector2.UnitY.RotatedByRandom(0.8f);
				Dust.NewDustPerfect(Center + off * 20, DustType<Dusts.GoldSlowFade>(), off * 0.15f, 0, default, 0.35f);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Tiles.Overgrow.PitGlow.Value;

			float off = (float)Math.Sin(timer) * 0.05f;

			for (int k = -1; k < 2; k++)
			{
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, 100), tex.Frame(), new Color(1, 0.9f, 0.6f) * (0.4f + off), (float)Math.PI + k * (0.9f + off), new Vector2(tex.Width / 2, 0), 0.7f, 0, 0);
			}
		}
	}
}