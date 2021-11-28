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
	class ObjectiveGear : GearTile
	{
		public override int DummyType => ModContent.ProjectileType<ObjectiveGearDummy>();

		public override bool NewRightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).modProjectile as GearTileDummy);

			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				dummy.Size++;
				return true;
			}

			return true;
		}
	}

	class ObjectiveGearDummy : GearTileDummy
	{
		public ObjectiveGearDummy() : base(ModContent.TileType<ObjectiveGear>()) { }

		public override void OnEngage()
		{

		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D pegTex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearPeg");
			spriteBatch.Draw(pegTex, projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			Texture2D tex;

			switch (size)
			{
				case 0: tex = ModContent.GetTexture(AssetDirectory.Invisible); break;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "CeramicGearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "CeramicGearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "CeramicGearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "CeramicGearSmall"); break;
			}

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class ObjectiveGearItem : QuickTileItem
	{
		public ObjectiveGearItem() : base("Gear puzzle Point", "Debug item", ModContent.TileType<ObjectiveGear>(), 8, AssetDirectory.Debug, true) { }
	}
}
