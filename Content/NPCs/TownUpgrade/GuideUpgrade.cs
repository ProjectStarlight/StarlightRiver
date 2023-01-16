using StarlightRiver.Content.Tiles;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownUpgrade
{
	class GuideUpgrade : TownUpgrade
	{
		public GuideUpgrade() : base("Guide", "[PH]Guide Quest", "No description", "Rift Crafting", "Scholar") { }

		public override List<Loot> Requirements => new()
		{
			new Loot(ItemID.DirtBlock, 20),
			new Loot(ItemID.Gel, 10),
			new Loot(ItemID.Wood, 50)
		};

		public override void ClickButton()
		{
			Main.NewText("No message", Color.Brown);
		}
	}
}
