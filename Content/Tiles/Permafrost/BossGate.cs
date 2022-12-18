using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	internal class BossGate : DummyTile
	{
		public override int DummyType => ProjectileType<BossGateDummy>();

		public override string Texture => "StarlightRiver/Assets/Invisible";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 21, 4, 0, SoundID.Tink, false, new Color(100, 120, 200));
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			Main.tileSolid[Type] = !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned);
		}
	}

	internal class BossGateDummy : Dummy
	{
		public BossGateDummy() : base(TileType<BossGate>(), 21 * 16, 4 * 16) { }

		public override void SafeSetDefaults()
		{
			Projectile.hide = true;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Permafrost/BossGate").Value;

			Vector2 off = Vector2.UnitX * (Projectile.ai[0] / 120 * Projectile.width / 2);

			if (Projectile.ai[0] > 0 && Projectile.ai[0] < 120)
				off += Vector2.One.RotatedByRandom(6.28f) * 0.5f;

			Color color = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
			Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition - off, color);
			Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition + Vector2.UnitX * Projectile.width / 2 + off, null, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
		}

		public override void Update()
		{
			if (StarlightWorld.HasFlag(WorldFlags.SquidBossDowned) && Projectile.ai[0] < 120)
				Projectile.ai[0]++;

			if (!StarlightWorld.HasFlag(WorldFlags.SquidBossDowned) && Projectile.ai[0] > 0)
				Projectile.ai[0]--;

			if (Projectile.ai[0] > 0 && Projectile.ai[0] < 120)
			{
				Dust.NewDustPerfect(Projectile.position + Vector2.UnitY * Main.rand.NextFloat(Projectile.height), DustType<Dusts.Stone>());
				Dust.NewDustPerfect(Projectile.position + new Vector2(Projectile.width, Main.rand.NextFloat(Projectile.height)), DustType<Dusts.Stone>());
			}

			if (Projectile.ai[0] == 119)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Tink with { Pitch = -1f }, Projectile.Center);
				CameraSystem.shake += 7;
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}
	}
}