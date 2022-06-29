using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
    class DynamicGear : GearTile
    {
        public override int DummyType => ModContent.ProjectileType<DynamicGearDummy>();

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<GearTilePlacer>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override bool RightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).ModProjectile as GearTileDummy);

			var entity = TileEntity.ByPosition[new Point16(i, j)] as GearTileEntity;

			if (entity is null)
				return false;

			if (dummy is null || dummy.gearAnimation > 0)
				return false;

			entity.Disengage();

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			GearPuzzleHandler.PuzzleOriginEntity?.Engage(2);

			return true;
		}
	}

    class DynamicGearDummy : GearTileDummy
    {
        public DynamicGearDummy() : base(ModContent.TileType<DynamicGear>()) { }

		public override void Update()
		{
			base.Update();

			Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.2f, 0.3f) * Size);
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D pegTex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "GearPeg").Value;
			Main.spriteBatch.Draw(pegTex, Projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			Texture2D tex;

			switch (Size)
			{
				case 0: tex = ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value; break;
				case 1: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value; break;
				case 2: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearMid").Value; break;
				case 3: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearLarge").Value; break;
				default: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value; break;
			}

			if (gearAnimation > 0) //switching between sizes animation
			{
				Texture2D texOld;

				switch (oldSize)
				{
					case 0: texOld = ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value; break;
					case 1: texOld = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value; break;
					case 2: texOld = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearMid").Value; break;
					case 3: texOld = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearLarge").Value; break;
					default: texOld = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value; break;
				}

				if (gearAnimation > 20)
				{
					float progress = Helpers.Helper.BezierEase((gearAnimation - 20) / 20f);
					Main.spriteBatch.Draw(texOld, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, texOld.Size() / 2, progress, 0, 0);
				}
				else
				{
					float progress = Helpers.Helper.SwoopEase(1 - gearAnimation / 20f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, tex.Size() / 2, progress, 0, 0);
				}

				return;
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "Debug Item", "DynamicGear", 8, AssetDirectory.VitricTile) { }
	}
}
