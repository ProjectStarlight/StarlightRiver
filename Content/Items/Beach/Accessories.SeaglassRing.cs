using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Beach
{
	class SeaglassRing : SmartAccessory
	{
		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public SeaglassRing() : base("Seaglass Ring", "+10 barrier\nBarrier recharge starts slightly faster\n'The battering waves have not diminished its shine'") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			var mp = Player.GetModPlayer<BarrierPlayer>();

			mp.RechargeDelay -= 30;
			mp.MaxBarrier += 10;
		}
	}

	class SeaglassRingTile : ModTile
	{
		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Iron, SoundID.Coins, true, new Color(150, 250, 250));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.Next(20) == 0)
				Dust.NewDust(new Vector2(i, j) * 16, 16, 16, 15, 0, 0, 0, default, 0.5f);

			if (Main.rand.Next(40) == 0)
			{
				Vector2 pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
				if (Main.rand.NextBool())
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.CrystalSparkle>(), Vector2.Zero);
				else
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.CrystalSparkle2>(), Vector2.Zero);
			}
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (!fail)
				Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 16, 16), ModContent.ItemType<SeaglassRing>());
		}
	}

	class SeaglassRingFishingPlayer : ModPlayer
	{
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
		{
			if (attempt.rare && Main.rand.NextBool(15) && Player.ZoneBeach)
			{ 
				itemDrop = ModContent.ItemType<SeaglassRing>();
			}
		}
	}
}
