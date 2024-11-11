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
		Tile Parent => Framing.GetTileSafely((int)Center.X / 16, (int)Center.Y / 16);

		public override Asset<Texture2D> Texture => GetStaminaTexture();

		public override Color GlowColor => new(0, 30, 90);

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

			Lighting.AddLight(Center, new Vector3(0.5f, 0.25f, 0.05f));
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.StarTexture.Value;
			float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
			float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 2f);

			Vector2 drawPos = Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.visualTimer) * 5);

			float op = Visible ? 1f : 0.1f;

			Main.spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0) * op, 0, tex.Size() / 2f, 0.2f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.1f + sin2 * 0.05f, 0, 0);

			Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 230, 255, 0) * op, 0, tex.Size() / 2f, 0.25f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 160, 255, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.2f + sin2 * 0.05f, 0, 0);

			Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 10, 60, 0) * op, 0, tex.Size() / 2f, 0.3f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(tex, drawPos, null, new Color(0, 0, 60, 0) * op, 1.57f / 2f, tex.Size() / 2f, 0.25f + sin2 * 0.05f, 0, 0);

			base.PostDraw(lightColor);
		}

		public override void PickupEffects(Player Player)
		{
			AbilityHandler ah = Player.GetHandler();

			ah.Shards.Add(Parent.TileFrameX);

			if (ah.ShardCount % 3 == 0)
				TextCard.Display("Starlight Vessel", "Your maximum starlight has increased by 1", 240, 1f);
			else
				TextCard.Display("Starlight Vessel Shard", "Collect " + (3 - ah.ShardCount % 3) + " more to increase your maximum starlight", 240, 1f);

			Player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 1;
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