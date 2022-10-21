using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class ObjectiveGear : GearTile
	{
		public override int DummyType => ModContent.ProjectileType<ObjectiveGearDummy>();

		public override bool RightClick(int i, int j)
		{
			var dummy = Dummy(i, j).ModProjectile as GearTileDummy;

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
		}
	}

	class ObjectiveGearDummy : GearTileDummy
	{
		public ObjectiveGearDummy() : base(ModContent.TileType<ObjectiveGear>()) { }

		public override void PostDraw(Color lightColor)
		{
			Texture2D pegTex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "GearPeg").Value;
			Main.spriteBatch.Draw(pegTex, Projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);
			Texture2D tex = Size switch
			{
				0 => ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value,
				1 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearSmall").Value,
				2 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearMid").Value,
				3 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearLarge").Value,
				_ => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CeramicGearSmall").Value,
			};
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	class ObjectiveGearItem : QuickTileItem
	{
		public ObjectiveGearItem() : base("Gear puzzle Point", "Debug Item", "ObjectiveGear", 8, AssetDirectory.VitricTile + "GearPeg", true) { }
	}
}
