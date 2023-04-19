﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class DullBlade : QuickMaterial
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public DullBlade() : base("Dull Blade", "Doesn't seem very sharp... yet", 1, Item.sellPrice(gold: 1), ItemRarityID.Orange) { }
	}
}