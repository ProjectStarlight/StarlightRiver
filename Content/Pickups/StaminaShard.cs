using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Codex.Entries;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	class StaminaShardPickup : AbilityPickup
	{
		Tile Parent => Framing.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);

		public override string Texture => GetStaminaTexture();

		public override Color GlowColor => new(255, 100, 30);

		public override bool Fancy => false;

		public override bool CanPickup(Player Player)
		{
			AbilityHandler ah = Player.GetHandler();
			return !ah.Shards.Has(Parent.TileFrameX);
		}

		public override void Visuals()
		{
			if (Main.rand.NextBool(2))
				Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(16), DustType<Content.Dusts.Stamina>(), Vector2.UnitY * -1);

			Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.25f, 0.05f));
		}

		public override void PickupEffects(Player Player)
		{
			AbilityHandler ah = Player.GetHandler();

			ah.Shards.Add(Parent.TileFrameX);

			if (ah.ShardCount % 3 == 0)
				UILoader.GetUIState<TextCard>().Display("Stamina Vessel", "Your maximum stamina has increased by 1", null, 240, 0.8f);
			else
				UILoader.GetUIState<TextCard>().Display("Stamina Vessel Shard", "Collect " + (3 - ah.ShardCount % 3) + " more to increase your maximum stamina", null, 240, 0.6f);

			Player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 1;

			Helper.UnlockCodexEntry<StaminaShardEntry>(Main.LocalPlayer);
		}

		private static string GetStaminaTexture()
		{
			if (Main.gameMenu)
				return "StarlightRiver/Assets/Abilities/Stamina1";

			AbilityHandler ah = Main.LocalPlayer.GetHandler();
			return "StarlightRiver/Assets/Abilities/Stamina" + (ah.ShardCount + 1) % 3;
		}
	}

	class StaminaShardTile : AbilityPickupTile
	{
		public override int PickupType => NPCType<StaminaShardPickup>();

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			fail = true;

			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX += 1;

			if (tile.TileFrameX > 2)
				tile.TileFrameX = 0;

			Main.NewText("pickup set to stamina shard number " + tile.TileFrameX, Color.Orange);//debug?
		}
	}

	class StaminaShardTileItem : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Abilities/Stamina1";

		public StaminaShardTileItem() : base("Stamina Shard", "Debug Item", "StaminaShardTile", 1) { }
	}
}