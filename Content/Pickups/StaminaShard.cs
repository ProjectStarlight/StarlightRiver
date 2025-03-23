using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Tiles.Overgrow;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	class StaminaShardPickup : AbilityPickup
	{
		public override Asset<Texture2D> Texture => GetStaminaTexture();

		public override Color GlowColor => new(0, 90, 120);

		public override bool Fancy => false;

		public StaminaShardPickup() : base(TileType<StaminaShardTile>()) { }

		public override bool CanPickup(Player Player)
		{
			AbilityHandler ah = Player.GetHandler();
			return !ah.Shards.Has(Parent.TileFrameX);
		}

		public override void Visuals()
		{
			if (Main.rand.NextBool(8))
				Dust.NewDustPerfect(Center + Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(16), DustType<Dusts.AuroraFast>(), Vector2.UnitY * -1, 0, new Color(100, 220, 255));

			Lighting.AddLight(Center, new Vector3(0.6f, 0.7f, 0.9f));
		}

		public override void PostDraw(Color lightColor)
		{
			if (Visible)
			{
				Texture2D tex = Assets.StarTexture.Value;
				float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
				float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 2f);

				Vector2 drawPos = Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.visualTimer) * 5);

				float op = 1f;

				Main.spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0) * op, 0, tex.Size() / 2f, 0.2f + sin * 0.05f, 0, 0);
				Main.spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.1f + sin2 * 0.05f, 0, 0);

				Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 230, 255, 0) * op, 0, tex.Size() / 2f, 0.25f + sin * 0.05f, 0, 0);
				Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 160, 255, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.2f + sin2 * 0.05f, 0, 0);

				Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 10, 60, 0) * op, 0, tex.Size() / 2f, 0.3f + sin * 0.05f, 0, 0);
				Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 0, 60, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.25f + sin2 * 0.05f, 0, 0);
			}

			base.PostDraw(lightColor);
		}

		public override void PickupEffects(Player Player)
		{
			AbilityHandler ah = Player.GetHandler();

			ah.Shards.Add(Parent.TileFrameX);

			Vector2 drawPos = Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.visualTimer) * 5);
			GUI.Stamina.StartShardAnimation(drawPos);

			SoundHelper.PlayPitched("Effects/Loot", 1f, 0, Player.Center);

			for(int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(Center, DustType<Dusts.PixelatedImpactLineDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(50, Main.rand.Next(100, 200), 255, 0), Main.rand.NextFloat(0.1f, 0.2f));
			}

			Player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 0;
		}

		private static Asset<Texture2D> GetStaminaTexture()
		{
			if (Main.gameMenu)
				return Assets.Abilities.Stamina1;

			AbilityHandler ah = Main.LocalPlayer.GetHandler();

			return (ah.ShardCount % 3 + 1) switch
			{
				1 => Assets.Abilities.Stamina1,
				2 => Assets.Abilities.Stamina2,
				3 => Assets.Abilities.Stamina3,
				_ => Assets.Abilities.Stamina1,
			};
		}
	}

	class StaminaShardTile : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<StaminaShardPickup>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			fail = true;

			if (StarlightRiver.debugMode)
			{
				Tile tile = Framing.GetTileSafely(i, j);
				tile.TileFrameX += 1;

				if (tile.TileFrameX > 2)
					tile.TileFrameX = 0;

				Main.NewText("pickup set to stamina shard number " + tile.TileFrameX, Color.Orange);//debug?
			}
		}
	}

	[SLRDebug]
	class StaminaShardTileItem : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Abilities/Stamina1";

		public StaminaShardTileItem() : base("Starlight Shard", "{{Debug}} Item", "StaminaShardTile", 1) { }
	}
}