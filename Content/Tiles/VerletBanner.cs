using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles
{
	class VerletBanner : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<VerletBannerDummy>();

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
			this.QuickSetFurniture(2, 4, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(120, 100, 100));
		}
	}

	[SLRDebug]
	class VerletBannerItem : QuickTileItem
	{
		public VerletBannerItem() : base("Verlet banner", "{{Debug}} Item", "VerletBanner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class VerletBannerDummy : Dummy
	{
		public float timer;

		private VerletChain Chain;

		public VerletBannerDummy() : base(TileType<VerletBanner>(), 32, 32) { }

		public override void SafeSetDefaults()
		{
			Chain = new VerletChain(16, false, Center, 16)
			{
				constraintRepetitions = 2,//defaults to 2, raising this lowers stretching at the cost of performance
				drag = 2f,//This number defaults to 1, Is very sensitive
				forceGravity = new Vector2(0f, 0.25f),//gravity x/y
				scale = 0.6f,
				parent = this
			};
		}

		public override void Update()
		{
			Chain.UpdateChain(Center);

			Chain.IterateRope(WindForce);
			timer += 0.005f;
		}

		private void WindForce(int index)//wind
		{
			int offset = (int)(position.X / 16 + position.Y / 16);

			float sin = (float)System.Math.Sin(StarlightWorld.visualTimer + offset - index / 3f);

			float cos = (float)System.Math.Cos(timer);
			float sin2 = (float)System.Math.Sin(StarlightWorld.visualTimer + offset + cos);

			var pos = new Vector2(Chain.ropeSegments[index].posNow.X + 1 + sin2 * 1.2f, Chain.ropeSegments[index].posNow.Y + sin * 1.4f);

			Color color = new Color(150, 10, 35).MultiplyRGB(Color.White * (1 - sin * 0.2f)).MultiplyRGB(Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16));

			Chain.ropeSegments[index].posNow = pos;
			Chain.ropeSegments[index].color = color;
		}
	}
}