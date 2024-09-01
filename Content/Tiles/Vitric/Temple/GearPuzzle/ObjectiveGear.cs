using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class ObjectiveGear : GearTile
	{
		public override int DummyType => DummySystem.DummyType<ObjectiveGearDummy>();

		public override bool RightClick(int i, int j)
		{
			var dummy = Dummy(i, j) as GearTileDummy;

			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				dummy.GearSize++;
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
			Texture2D pegTex = Assets.Tiles.Vitric.GearPeg.Value;
			Main.spriteBatch.Draw(pegTex, Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			Texture2D tex = GearSize switch
			{
				0 => Assets.Invisible.Value,
				1 => Assets.Tiles.Vitric.CeramicGearSmall.Value,
				2 => Assets.Tiles.Vitric.CeramicGearMid.Value,
				3 => Assets.Tiles.Vitric.CeramicGearLarge.Value,
				_ => Assets.Tiles.Vitric.CeramicGearSmall.Value,
			};

			Main.spriteBatch.Draw(tex, Center - Main.screenPosition, null, lightColor, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}

	[SLRDebug]
	class ObjectiveGearItem : QuickTileItem
	{
		public ObjectiveGearItem() : base("Gear puzzle Point", "{{Debug}} Item", "ObjectiveGear", 8, AssetDirectory.VitricTile + "GearPeg", true) { }
	}
}