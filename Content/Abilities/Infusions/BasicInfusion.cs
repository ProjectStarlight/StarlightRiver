using System;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.Infusions
{
	class BasicInfusion : InfusionItem
	{
		public override InfusionTier Tier => InfusionTier.Bronze;

		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override bool Equippable => false;

		public override Type AbilityType => null;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blank Slate");
			Tooltip.SetDefault("La Tabula Rasa\nCan be imprinted with challenges at an infusion station\nComplete challenges to transform into an infusion\nNew infusions become available as you progress");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Blue;
		}
	}
}