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

		public override bool RightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).ModProjectile as GearTileDummy);

			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				dummy.Size++;
				return true;
			}

			return true;
		}

		public override void OnEngage(GearTileEntity entity)
		{
			GearPuzzleHandler.engagedObjectives++;

			for (int k = 0; k < 10; k++)
				Dust.NewDustPerfect(entity.Position.ToVector2() * 16, ModContent.DustType<Dusts.Glow>(), null, 0, new Color(255, 0, 0));

			if (GearPuzzleHandler.engagedObjectives > 2)
				GearPuzzleHandler.solved = true;
		}
	}

	class ObjectiveGearDummy : GearTileDummy
	{
		public ObjectiveGearDummy() : base(ModContent.TileType<ObjectiveGear>()) { }

		public override void PostDraw(Color lightColor)
		{
			Texture2D pegTex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "GearPeg").Value;
			Main.spriteBatch.Draw(pegTex, Projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			Texture2D tex;

			switch (Size)
			{
				case 0: tex = ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value; break;
				case 1: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearSmall").Value; break;
				case 2: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearMid").Value; break;
				case 3: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearLarge").Value; break;
				default: tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearSmall").Value; break;
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class ObjectiveGearItem : QuickTileItem
	{
		public ObjectiveGearItem() : base("Gear puzzle Point", "Debug Item", "ObjectiveGear", 8, AssetDirectory.VitricTile + "GearPeg", true) { }
	}
}
