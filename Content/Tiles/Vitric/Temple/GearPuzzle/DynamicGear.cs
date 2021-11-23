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

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
    class DynamicGear : GearTile
    {
        public override int DummyType => ModContent.ProjectileType<DynamicGearDummy>();
	}

    class DynamicGearDummy : GearTileDummy
    {
        public DynamicGearDummy() : base(ModContent.TileType<DynamicGear>()) { }

        public override void OnEngage()
        {

        }

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex;

			switch (size)
			{
				case 0: tex = ModContent.GetTexture(AssetDirectory.Invisible); break;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
			}

			if (gearAnimation > 0) //switching between sizes animation
			{
				Texture2D texOld;

				switch (oldSize)
				{
					case 0: texOld = ModContent.GetTexture(AssetDirectory.Invisible); break;
					case 1: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
					case 2: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
					case 3: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
					default: texOld = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				}

				if (gearAnimation > 20)
				{
					float progress = Helpers.Helper.BezierEase((gearAnimation - 20) / 20f);
					spriteBatch.Draw(texOld, projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, texOld.Size() / 2, progress, 0, 0);
				}
				else
				{
					float progress = Helpers.Helper.SwoopEase(1 - gearAnimation / 20f);
					spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, tex.Size() / 2, progress, 0, 0);
				}

				return;
			}

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "Debug item", ModContent.TileType<DynamicGear>(), 8, AssetDirectory.Debug, true) { }
	}
}
