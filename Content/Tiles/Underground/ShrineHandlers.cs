using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Linq;
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
		public static void SimulateGoldChest(Entity source, bool twiceReforge)
		{
			int[] chestItems = new int[] { ItemID.BandofRegeneration, ItemID.MagicMirror, ItemID.CloudinaBottle, ItemID.HermesBoots, ItemID.Mace, ItemID.EnchantedBoomerang, ItemID.ShoeSpikes, ItemID.FlareGun };

			int chosenItem = Main.rand.Next(chestItems);

			if (chosenItem == ItemID.FlareGun)
			{
				int i = Item.NewItem(source.GetSource_FromAI(), source.Hitbox, chosenItem, prefixGiven: -1);

				if (twiceReforge)
					Main.item[i].Prefix(-2);

				Main.item[i].GetGlobalItem<RelicItem>().isRelic = twiceReforge && Main.item[i].CanHavePrefixes();

				Item.NewItem(source.GetSource_FromAI(), source.Hitbox, ItemID.Flare, 50);
			}
			else
			{
				int i = Item.NewItem(source.GetSource_FromAI(), source.Hitbox, chosenItem, prefixGiven: -1);

				if (twiceReforge)
					Main.item[i].Prefix(-2);

				Main.item[i].GetGlobalItem<RelicItem>().isRelic = twiceReforge && Main.item[i].CanHavePrefixes();

			}
		}

		public static void SimulateWoodenChest(Entity source)
		{
			int[] chestItems = new int[] { ItemID.Spear, ItemID.Blowpipe, ItemID.WoodenBoomerang, ItemID.Aglet, ItemID.ClimbingClaws, ItemID.Umbrella, 3068, ItemID.WandofSparking, ItemID.Radar, ItemID.PortableStool }; // 3068 is guide to plant fiber cortilage or whatever

			int chosenItem = Main.rand.Next(chestItems);

			Item.NewItem(source.GetSource_FromAI(), source.Hitbox, chosenItem, prefixGiven: -1);
		}
	}
}