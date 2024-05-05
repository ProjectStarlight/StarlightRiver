﻿using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class GearPuzzleOrigin : GearTile
	{
		public override int DummyType => DummySystem.DummyType<GearPuzzleOriginDummy>();

		public override bool RightClick(int i, int j)
		{
			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				GearPuzzleHandler.PuzzleOriginLocation = new Point16(i, j);
				Main.NewText($"Origin gear at ({i}, {j}) designated as gear puzzle origin for this world!", new Color(255, 255, 0));
				return true;
			}

			return false;
		}
	}

	class GearPuzzleOriginDummy : GearTileDummy
	{
		public GearPuzzleOriginDummy() : base(ModContent.TileType<GearPuzzleOrigin>()) { }

		public override void Update()
		{
			base.Update();

			if (GearEntity is null)
				return;

			GearSize = 3;
			Engaged = true;
			RotationVelocity = 2;

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Lighting.AddLight(Center, new Vector3(1, 0.7f, 0.4f) * 0.5f);
		}

		public override void PostDraw(Color lightColor)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D bgTex = Assets.Tiles.Vitric.OriginGearBase.Value;
			Main.EntitySpriteDraw(bgTex, Center - Main.screenPosition, null, lightColor, 0, new Vector2(bgTex.Width / 2, 4), 1, 0, 0);

			Texture2D tex = Assets.Tiles.Vitric.OriginGear.Value;

			Main.EntitySpriteDraw(tex, Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);

			Texture2D magmiteTex = ModContent.Request<Texture2D>(AssetDirectory.VitricNpc + "MagmitePassive").Value;
			float sinTimer = Main.GameUpdateCount / 20f;
			var frame = new Rectangle(42, sinTimer % 6.28f < 1.57f ? 0 : (int)(Main.GameUpdateCount / 3f) % 5 * 40, 42, 40);

			Main.EntitySpriteDraw(magmiteTex, Center - Main.screenPosition, frame, Color.White, (float)Math.Sin(sinTimer), new Vector2(21, 0), 1, SpriteEffects.FlipHorizontally, 0);
		}
	}

	[SLRDebug]
	class GearPuzzleOriginPlacer : QuickTileItem
	{
		public GearPuzzleOriginPlacer() : base("Gear puzzle origin", "{{Debug}} Item", "GearPuzzleOrigin", 8, AssetDirectory.VitricTile + "OriginGearBase", true) { }
	}
}