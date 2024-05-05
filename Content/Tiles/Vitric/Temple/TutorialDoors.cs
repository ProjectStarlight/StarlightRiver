using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class TutorialDoor1 : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<TutorialDoor1Dummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(2, 13, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
		}
	}

	class TutorialDoor1Dummy : Dummy
	{
		public float progress;

		public override bool DoesCollision => true;

		public TutorialDoor1Dummy() : base(TileType<TutorialDoor1>(), 16 * 2, 16 * 13) { }

		public override void Update()
		{
			if (Main.LocalPlayer.GetModPlayer<StarlightPlayer>().inTutorial)
			{
				if (progress < 1)
					progress += 0.005f;
			}
			else if (progress > 0)
			{
				progress -= 0.005f;
			}
		}

		public override void Collision(Player Player)
		{
			if (Player.GetModPlayer<StarlightPlayer>().inTutorial)
			{
				if (Player.Hitbox.Intersects(Hitbox))
					Player.velocity.X = 1;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Player Player = Main.LocalPlayer;

			if (progress > 0)
			{
				Texture2D tex = Assets.Tiles.Vitric.TutorialDoor1.Value;
				int off = (int)(tex.Height * progress);
				Vector2 pos = position - Main.screenPosition;
				var source = new Rectangle(0, 0, tex.Width, off);

				Main.spriteBatch.Draw(tex, pos, source, lightColor);
			}
		}
	}

	[SLRDebug]
	class TutorialDoor1Item : QuickTileItem
	{
		public TutorialDoor1Item() : base("TutorialDoor1", "{{Debug}} Item", "TutorialDoor1", 1, AssetDirectory.Debug, true) { }
	}

	class TutorialDoor2 : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<TutorialDoor2Dummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(2, 13, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
		}
	}

	class TutorialDoor2Dummy : Dummy
	{
		public override bool DoesCollision => true;

		public TutorialDoor2Dummy() : base(TileType<TutorialDoor2>(), 16 * 2, 16 * 13) { }

		public bool ShouldBeOn(Player player)
		{
			return player.GetModPlayer<StarlightPlayer>().inTutorial || !player.GetModPlayer<AbilityHandler>().GetAbility<Dash>(out _);
		}

		public override void Collision(Player Player)
		{
			if (ShouldBeOn(Player) && Player.Hitbox.Intersects(Hitbox))
			{
				if (AbilityHelper.CheckDash(Player, Hitbox))
				{
					Player.GetModPlayer<StarlightPlayer>().inTutorial = false;

					Player.GetModPlayer<AbilityHandler>().ActiveAbility?.Deactivate();
					Player.velocity = Vector2.Normalize(Player.velocity) * -10f;
					Player.velocity.Y -= 5;

					CameraSystem.shake += 10;

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

			if (ShouldBeOn(Player))
			{
				Main.spriteBatch.Draw(Assets.Tiles.Vitric.TutorialDoor2.Value, position - Main.screenPosition, lightColor);
				Main.spriteBatch.Draw(Assets.Tiles.Vitric.TutorialDoor2Glow.Value, position - Main.screenPosition, Helper.IndicatorColor);
			}
		}
	}

	[SLRDebug]
	class TutorialDoor2Item : QuickTileItem
	{
		public TutorialDoor2Item() : base("TutorialDoor2", "{{Debug}} Item", "TutorialDoor2", 1, AssetDirectory.Debug, true) { }
	}
}