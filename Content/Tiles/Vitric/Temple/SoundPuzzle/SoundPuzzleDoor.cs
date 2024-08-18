using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.SoundPuzzle
{
	class SoundPuzzleDoor : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "DoorVertical";

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(1, 5, ModContent.DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Framing.GetTileSafely(i, j).IsActuated = SoundPuzzleHandler.solved;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.UnitY * -Helper.BezierEase(SoundPuzzleHandler.solveTimer / 180f) * 5 * 16;
			var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

			spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Lighting.GetColor(i, j));

			return false;
		}
	}

	[SLRDebug]
	class SoundPuzzleDoorItem : QuickTileItem
	{
		public SoundPuzzleDoorItem() : base("Sound Puzzle Temple Door", "Temple Door, Opens if sound puzzle is solved", "SoundPuzzleDoor", ItemRarityID.Blue, AssetDirectory.Debug, true) { }
	}
}