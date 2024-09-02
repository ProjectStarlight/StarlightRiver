﻿using StarlightRiver.Content.Tiles;
using StarlightRiver.Core.Systems.NPCUpgradeSystem;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.TownUpgrade
{
	public abstract class TownUpgrade
	{
		public readonly string buttonName;
		public readonly string NPCName;
		public readonly string questName;
		public readonly string questTip;
		public readonly string title;
		public readonly Texture2D icon;

		public bool Unlocked => NPCUpgradeSystem.townUpgrades.TryGetValue(NPCName, out bool unlocked) && unlocked;

		public virtual List<Loot> Requirements => new() { new Loot(ItemID.DirtBlock, 1) };

		protected TownUpgrade(string NPCName, string questName, string questTip, string buttonName, string title)
		{
			this.buttonName = buttonName;
			this.NPCName = NPCName;
			this.questName = questName;
			this.questTip = questTip;
			this.title = title;

			icon = Assets.NPCs.TownUpgrade.GuideIcon.Value;
		}

		public virtual void ClickButton() { }

		public static TownUpgrade FromString(string input)
		{
			TownUpgrade town = input switch
			{
				"Guide" => new GuideUpgrade(),
				"Merchant" => new MerchantUpgrade(),
				_ => null,
			};
			return town;
		}
	}
}