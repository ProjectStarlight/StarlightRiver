using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class DoorBombShooter : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/DoorBombShooter";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Ice, SoundID.Tink, false, new Color(200, 255, 255));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.SquidBossOpen) && !Main.projectile.Any(n => n.active && n.type == ProjectileType<DoorBomb>()))
				Projectile.NewProjectile(new EntitySource_WorldEvent(), new Vector2(i + 1, j + 0.5f) * 16, new Vector2(1, 0), ProjectileType<DoorBomb>(), 0, 0);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(18, -42);

			if (!StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				Utils.DrawBorderString(spriteBatch, "Place blocks on", pos, Color.White, 0.7f);
				Utils.DrawBorderString(spriteBatch, "BLUE", pos + new Vector2(90, 0), Color.DeepSkyBlue, 0.7f);
				Utils.DrawBorderString(spriteBatch, "squares", pos + new Vector2(130, 0), Color.White, 0.7f);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D barrier = ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value;
			var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.2f), barrier.Width, barrier.Height);
			var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.42f), barrier.Width, barrier.Height);

			var targetRect = new Rectangle((int)pos.X - 6 * 16 + 2, (int)pos.Y + 16 * 4, 32, 64);
			spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(150, 225, 255) * 0.6f * 1);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(220, 150, 250) * 0.5f * 1);
			targetRect.Inflate(-15, 0);
			targetRect.Offset(15, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * 1);

			targetRect = new Rectangle((int)pos.X + 15 * 16 - 6, (int)pos.Y + 16 * 4, 32, 64);
			spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(150, 225, 255) * 0.6f * 1, 0, default, SpriteEffects.FlipHorizontally, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(220, 150, 250) * 0.5f * 1, 0, default, SpriteEffects.FlipHorizontally, 0);
			targetRect.Inflate(-15, 0);
			targetRect.Offset(-15, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * 1);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	[SLRDebug]
	class DoorBombShooterItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public DoorBombShooterItem() : base("Debug Shooter Placer", "", "DoorBombShooter", ItemRarityID.White) { }
	}
}