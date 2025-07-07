using StarlightRiver.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class AuroraBrick : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrick";

		public override void Load()
		{
			On_Projectile.AI_007_GrapplingHooks_CanTileBeLatchedOnTo += StopGrappling;
		}

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, DustID.Ice, SoundID.Tink, new Color(81, 192, 240), ItemType<AuroraBrickItem>());
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		private bool StopGrappling(On_Projectile.orig_AI_007_GrapplingHooks_CanTileBeLatchedOnTo orig, Projectile self, int x, int y)
		{
			Tile theTile = Framing.GetTileSafely(x, y);

			if (theTile.TileType == TileType<AuroraBrick>())
			{
				self.tileCollide = true;
				return false;
			}

			return orig(self, x, y);
		}
	}

	class AuroraBrickDoor : AuroraBrick
	{
		public override void NearbyEffects(int i, int j, bool closer)
		{
			bool flag = StarlightWorld.HasFlag(WorldFlags.SquidBossOpen);

			Main.tileSolid[Type] = !flag;
			Main.tileSolidTop[Type] = flag;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				drawData.finalColor *= 0f;
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				Tile tile = Main.tile[i, j];
				Texture2D tex = Assets.Tiles.Permafrost.AuroraBrickDoorOver.Value;
				Vector2 pos = new Vector2(i, j) * 16 - Main.screenPosition + Vector2.One * Main.offScreenRange;
				var frame = new Rectangle(tile.frameX, tile.frameY, 16, 16);

				spriteBatch.Draw(tex, pos, frame, Lighting.GetColor(i, j));
			}
		}
	}

	class AuroraBrickItem : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickItem";

		public AuroraBrickItem() : base("Aurora Brick", "Oooh... Preeetttyyy", "AuroraBrick", ItemRarityID.White) { }
	}

	[SLRDebug]
	class AuroraBrickDoorItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public AuroraBrickDoorItem() : base("{{Debug}} Brick Placer", "", "AuroraBrickDoor", ItemRarityID.White) { }
	}
}