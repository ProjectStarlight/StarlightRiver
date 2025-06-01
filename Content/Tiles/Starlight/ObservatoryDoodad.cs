using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class ObservatoryDoodad : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<ObservatoryDoodadDummy>();

		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 6, 8, DustID.Gold, SoundID.Tink, false, new Color(200, 200, 200));
			MinPick = 999;
		}
	}

	class ObservatoryDoodadDummy : Dummy
	{
		public ObservatoryDoodadDummy() : base(ModContent.TileType<ObservatoryDoodad>(), 16 * 6, 16 * 8) { }

		public override void Update()
		{
			Lighting.AddLight(Center + Vector2.UnitY * -16, new Vector3(0.35f, 0.7f, 1.1f));
			base.Update();
		}

		public override void DrawBehindTiles()
		{
			var tex = Assets.Tiles.Starlight.ObervatoryDoodadBack.Value;
			LightingBufferRenderer.DrawWithLighting(tex, Center + Vector2.UnitY * height / 2f - Main.screenPosition, null, Color.White, 0, new Vector2(tex.Width / 2f, tex.Height), 1);
		}
	}
}