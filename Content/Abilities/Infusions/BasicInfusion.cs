using System;
using Terraria.ID;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Abilities.AbilityContent.Infusions
{
    class BasicInfusion : InfusionItem
    {
        public override InfusionTier Tier => InfusionTier.Bronze;

        public override Type AbilityType => null;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Infusion I");
            Tooltip.SetDefault("Generic Infusion\nSlightly improves stamina regeneration\nUsed to create other infusions");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }

        public override void UpdateFixed()
        {
            Player.GetHandler().StaminaRegenRate += 0.2f;
        }
    }
}
