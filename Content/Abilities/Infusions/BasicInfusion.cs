using StarlightRiver.Content.Abilities;
using System;
using Terraria.ID;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
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
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }
    }
}
