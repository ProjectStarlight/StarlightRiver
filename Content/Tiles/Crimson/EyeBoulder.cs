using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class EyeBoulder : DummyTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/EyeBoulder";

		public override int DummyType => DummySystem.DummyType<EyeBoulderDummy>();

		public override void SetStaticDefaults()
		{
			var anchor = new AnchorData(AnchorType.SolidTile, 3, 0);
			QuickBlock.QuickSetFurniture(this, 3, 2, DustID.Blood, SoundID.NPCHit1, true, Color.Wheat, false, false, "", anchor, variants: 2);
		}
	}

	internal class EyeBoulderDummy : Dummy
	{
		public EyeBoulderDummy() : base(ModContent.TileType<EyeBoulder>(), 3 * 16, 2 * 16) { }

		public override void PostDraw(Color lightColor)
		{
			Texture2D pupil = Assets.Tiles.Crimson.EyeBoulderPupil.Value;

			float angle = Center.AngleTo(Main.LocalPlayer.Center);
			float dist = Vector2.Distance(Main.LocalPlayer.Center, Center);
			Vector2 pos = Center + Vector2.UnitY * 2 + new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.7f) * 7;
			Rectangle frame = new(Parent.TileFrameX < 54 ? 0 : 12, dist < 32 ? 24 : dist < 120 ? 12 : 0, 12, 12);

			Main.spriteBatch.Draw(pupil, pos - Main.screenPosition, frame, Color.White, 0f, Vector2.One * 6, 1f, 0, 0);
		}
	}
}