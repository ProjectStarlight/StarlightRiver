using StarlightRiver.Content.Tiles;
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

		public bool Unlocked => StarlightWorld.townUpgrades.TryGetValue(NPCName, out bool unlocked) && unlocked;

		public virtual List<Loot> Requirements => new() { new Loot(ItemID.DirtBlock, 1) };

		protected TownUpgrade(string NPCName, string questName, string questTip, string buttonName, string title)
		{
			this.buttonName = buttonName;
			this.NPCName = NPCName;
			this.questName = questName;
			this.questTip = questTip;
			this.title = title;

			icon = Request<Texture2D>("StarlightRiver/Assets/NPCs/TownUpgrade/" + NPCName + "Icon").Value;
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
