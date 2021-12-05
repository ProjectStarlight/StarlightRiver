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
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D pegTex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearPeg");
			spriteBatch.Draw(pegTex, projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			var tex = ModContent.GetTexture(AssetDirectory.VitricTile + "OriginGear"); 

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class GearPuzzleOriginPlacer : QuickTileItem
	{
		public GearPuzzleOriginPlacer() : base("Gear puzzle origin", "Debug item", ModContent.TileType<GearPuzzleOrigin>(), 8, AssetDirectory.Debug, true) { }
	}
}
