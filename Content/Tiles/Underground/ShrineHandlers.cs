using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class EvasionShrineBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/EvasionShrine");

		public override bool IsBiomeActive(Player player)
		{
			return player.GetModPlayer<ShrinePlayer>().EvasionShrineActive;
		}
	}

	class CombatShrineBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/CombatShrine");

		public override bool IsBiomeActive(Player player)
		{
			return player.GetModPlayer<ShrinePlayer>().CombatShrineActive;
		}
	}

	public class ShrinePlayer : ModPlayer
	{
		public bool CombatShrineActive;
		public bool EvasionShrineActive;
		//public bool WitShrineActive;

		public override void ResetEffects()
		{
			CombatShrineActive = false;
			EvasionShrineActive = false;
			//WitShrineActive = false;
		}
	}

	public class ShrineUtils
	{
		public static void SimulateGoldChest(Projectile source, bool twiceReforge)
		{
			int[] chestItems = new int[] { ItemID.BandofRegeneration, ItemID.MagicMirror, ItemID.CloudinaBottle, ItemID.HermesBoots, ItemID.Mace, ItemID.EnchantedBoomerang, ItemID.ShoeSpikes, ItemID.FlareGun };

			int chosenItem = Main.rand.Next(chestItems);

			if (chosenItem == ItemID.FlareGun)
			{
				int i = Item.NewItem(source.GetSource_FromAI(), source.getRect(), chosenItem, prefixGiven: -1);

				if (twiceReforge)
					Main.item[i].Prefix(-2);

				Main.item[i].GetGlobalItem<RelicItem>().isRelic = twiceReforge && Main.item[i].CanHavePrefixes();

				Item.NewItem(source.GetSource_FromAI(), source.getRect(), ItemID.Flare, 50);
			}
			else
			{
				int i = Item.NewItem(source.GetSource_FromAI(), source.getRect(), chosenItem, prefixGiven: -1);

				if (twiceReforge)
					Main.item[i].Prefix(-2);

				Main.item[i].GetGlobalItem<RelicItem>().isRelic = twiceReforge && Main.item[i].CanHavePrefixes();

			}
		}

		public static void SimulateWoodenChest(Projectile source)
		{
			int[] chestItems = new int[] { ItemID.Spear, ItemID.Blowpipe, ItemID.WoodenBoomerang, ItemID.Aglet, ItemID.ClimbingClaws, ItemID.Umbrella, 3068, ItemID.WandofSparking, ItemID.Radar, ItemID.PortableStool }; // 3068 is guide to plant fiber cortilage or whatever

			int chosenItem = Main.rand.Next(chestItems);

			Item.NewItem(source.GetSource_FromAI(), source.getRect(), chosenItem, prefixGiven: -1);
		}
	}

	/// <summary>
	/// generalized packet for starting a shrine trial. should only be processed by a server or singleplayer client.
	/// </summary>
	[Serializable]
	public class ShineStartPacket : Module
	{
		readonly int tileI;
		readonly int tileJ;

		public ShineStartPacket(int tileI, int tileJ)
		{
			this.tileI = tileI;
			this.tileJ = tileJ;
		}

		protected override void Receive()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //this is not for execution by multiplayer clients

			Tile tile = Framing.GetTileSafely(tileI, tileJ);

			ModTile mTile = ModContent.GetModTile(tile.TileType);

			if (mTile == null)
			{
				StarlightRiver.Instance.Logger.Error("at (" + tileI + ", " + tileJ + ") failed to find shrine tile");
				return; //failed to find shrine tile (this should never happen)
			}

			ShrineTile shrineTile = mTile as ShrineTile;

			int x = tileI - tile.TileFrameX / 18;
			int y = tileJ - tile.TileFrameY / 18;

			shrineTile.SetActiveFrame(x, y);

			Projectile dummy = shrineTile.Dummy(x, y);

			(dummy.ModProjectile as ShrineDummy).State = ShrineDummy.ShrineState_Active;
			NetMessage.TrySendData(MessageID.SyncProjectile, number: dummy.whoAmI); //setting netupdate to true is strangely unreliable here, so we use netmessage -- TODO: look into why; this may have ramifications elsewhere too
		}
	}
}