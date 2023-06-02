using StarlightRiver.Content.Bosses.SquidBoss;
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
			if (!StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(18, -42);
				Utils.DrawBorderString(spriteBatch, "Place blocks on", pos, Color.White, 0.7f);
				Utils.DrawBorderString(spriteBatch, "BLUE", pos + new Vector2(90, 0), Color.DeepSkyBlue, 0.7f);
				Utils.DrawBorderString(spriteBatch, "squares", pos + new Vector2(130, 0), Color.White, 0.7f);
			}
		}
	}

	class DoorBombShooterItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public DoorBombShooterItem() : base("Debug Shooter Placer", "", "DoorBombShooter", ItemRarityID.White) { }
	}
}