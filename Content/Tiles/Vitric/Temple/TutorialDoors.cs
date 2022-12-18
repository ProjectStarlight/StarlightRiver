using StarlightRiver.Content.Abilities;
using StarlightRiver.Helpers;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class TutorialDoor1 : DummyTile
	{
		public override int DummyType => ProjectileType<TutorialDoor1Dummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
		}
	}

	class TutorialDoor1Dummy : Dummy
	{
		public TutorialDoor1Dummy() : base(TileType<TutorialDoor1>(), 16, 16 * 7) { }

		public override void Collision(Player Player)
		{
			if (Player.GetModPlayer<StarlightPlayer>().inTutorial && Player.Hitbox.Intersects(Projectile.Hitbox))
				Player.velocity.X = 1;
		}

		public override void PostDraw(Color lightColor)
		{
			Player Player = Main.LocalPlayer;

			if (!Player.GetModPlayer<StarlightPlayer>().inTutorial)
				return;

			Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor1").Value, Projectile.position - Main.screenPosition, lightColor);
		}
	}

	class TutorialDoor1Item : QuickTileItem
	{
		public TutorialDoor1Item() : base("TutorialDoor1", "Debug Item", "TutorialDoor1", 1, AssetDirectory.Debug, true) { }
	}

	class TutorialDoor2 : DummyTile
	{
		public override int DummyType => ProjectileType<TutorialDoor2Dummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(2, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
		}
	}

	class TutorialDoor2Dummy : Dummy
	{
		public TutorialDoor2Dummy() : base(TileType<TutorialDoor2>(), 16 * 2, 16 * 7) { }

		public override void Collision(Player Player)
		{
			if (Player.GetModPlayer<StarlightPlayer>().inTutorial && Player.Hitbox.Intersects(Projectile.Hitbox))
			{
				if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
				{
					Player.GetModPlayer<StarlightPlayer>().inTutorial = false;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Player.Center);

					for (int k = 0; k < 50; k++)
						Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.GlassGravity>());
				}
				else
				{
					Player.velocity.X = -1;
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Player Player = Main.LocalPlayer;

			if (!Player.GetModPlayer<StarlightPlayer>().inTutorial)
				return;

			Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2").Value, Projectile.position - Main.screenPosition, lightColor);
			Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2Glow").Value, Projectile.position - Main.screenPosition, Helper.IndicatorColor);
		}
	}

	class TutorialDoor2Item : QuickTileItem
	{
		public TutorialDoor2Item() : base("TutorialDoor2", "Debug Item", "TutorialDoor2", 1, AssetDirectory.Debug, true) { }
	}
}
