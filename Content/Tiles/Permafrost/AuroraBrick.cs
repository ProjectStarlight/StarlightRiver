using System;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class AuroraBrick : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrick";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, DustID.Ice, SoundID.Tink, new Color(81, 192, 240), ItemType<AuroraBrickItem>());
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

			float sin2 = (float)Math.Sin(StarlightWorld.visualTimer + off * 0.01f * 0.2f);
			float cos = (float)Math.Cos(StarlightWorld.visualTimer + off * 0.01f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
			float mult = Lighting.Brightness(i, j);

			drawData.colorTint = color.MultiplyRGB(Color.White * mult);
		}

		private bool StopGrappling(On.Terraria.Projectile.orig_AI_007_GrapplingHooks_CanTileBeLatchedOnTo orig, Projectile self, Tile theTile)
		{
			if (theTile.TileType == TileType<AuroraBrick>())
			{
				self.tileCollide = true;
				return false;
			}

			return orig(self, theTile);
		}
	}

	class AuroraBrickDoor : AuroraBrick
	{
		public override void NearbyEffects(int i, int j, bool closer)
		{
			Framing.GetTileSafely(i, j).IsActuated = StarlightWorld.HasFlag(WorldFlags.SquidBossOpen);
		}
	}

	class AuroraBrickItem : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickItem";

		public AuroraBrickItem() : base("Aurora Brick", "Oooh... Preeetttyyy", "AuroraBrick", ItemRarityID.White) { }
	}

	class AuroraBrickDoorItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public AuroraBrickDoorItem() : base("Debug Brick Placer", "", "AuroraBrickDoor", ItemRarityID.White) { }
	}
}
