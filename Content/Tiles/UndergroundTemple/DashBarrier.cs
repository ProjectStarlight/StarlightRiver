using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.DummyTileSystem;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class DashBarrier : DummyTile
	{
		public override int DummyType => ProjectileType<DashBarrierDummy>();

		public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 3, DustType<Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, true);
			MinPick = int.MaxValue;
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.NextBool(300))
			{
				var pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));

				if (Main.rand.NextBool())
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle>(), Vector2.Zero);
				else
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle2>(), Vector2.Zero);
			}

			base.SafeNearbyEffects(i, j, closer);
		}
	}

	internal class DashBarrierDummy : Dummy
	{
		public DashBarrierDummy() : base(TileType<DashBarrier>(), 32, 48) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
			{
				WorldGen.KillTile(ParentX, ParentY);
				NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 3, TileChangeType.None);

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
			}
		}
	}

	public class DashBarrierItem : QuickTileItem
	{
		public DashBarrierItem() : base("Dash Barrier", "Debug Item", "DashBarrier", -12, AssetDirectory.UndergroundTempleTile) { }
	}
}
