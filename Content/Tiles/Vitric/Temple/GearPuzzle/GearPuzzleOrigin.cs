using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class GearPuzzleOrigin : GearTile
	{
		public override int DummyType => ModContent.ProjectileType<GearPuzzleOriginDummy>();

		public override bool NewRightClick(int i, int j)
		{
			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				GearPuzzleHandler.PuzzleOriginLocation = new Point16(i, j);
				Main.NewText($"Origin gear at ({i}, {j}) designated as gear puzzle origin for this world!", new Color(255, 255, 0));
				return true;
			}

			return false;
		}

		public override void OnEngage(GearTileEntity entity)
		{
			GearPuzzleHandler.engagedObjectives = 0;
		}
	}

	class GearPuzzleOriginDummy : GearTileDummy
	{
		public GearPuzzleOriginDummy() : base(ModContent.TileType<GearPuzzleOrigin>()) { }

		public override void Update()
		{
			base.Update();

			Size = 3;
			Engaged = true;
			RotationVelocity = 2;

			Lighting.AddLight(projectile.Center, new Vector3(1, 0.7f, 0.4f) * 0.5f);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D bgTex = ModContent.GetTexture(AssetDirectory.VitricTile + "OriginGearBase");
			spriteBatch.Draw(bgTex, projectile.Center - Main.screenPosition, null, lightColor, 0, new Vector2(bgTex.Width / 2, 4), 1, 0, 0);

			var tex = ModContent.GetTexture(AssetDirectory.VitricTile + "OriginGear"); 

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);

			var magmiteTex = ModContent.GetTexture(AssetDirectory.VitricNpc + "MagmitePassive");
			var sinTimer = Main.GameUpdateCount / 20f;
			var frame = new Rectangle(42, sinTimer % 6.28f < 1.57f ? 0 : (int)(Main.GameUpdateCount / 3f) % 5 * 40, 42, 40);

			spriteBatch.Draw(magmiteTex, projectile.Center - Main.screenPosition, frame, Color.White, (float)Math.Sin(sinTimer), new Vector2(21, 0), 1, SpriteEffects.FlipHorizontally, 0);
		}
	}

	class GearPuzzleOriginPlacer : QuickTileItem
	{
		public GearPuzzleOriginPlacer() : base("Gear puzzle origin", "Debug item", ModContent.TileType<GearPuzzleOrigin>(), 8, AssetDirectory.Debug, true) { }
	}
}
