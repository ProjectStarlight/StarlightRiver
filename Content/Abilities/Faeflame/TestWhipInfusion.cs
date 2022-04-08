using Microsoft.Xna.Framework;
using StarlightRiver.Abilities.AbilityContent.Infusions;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Abilities.Faeflame
{
    public class TestWhip : InfusionItem<Whip>
    {
        public override InfusionTier Tier => InfusionTier.Untiered;
        public override string Texture => "StarlightRiver/Assets/Abilities/TestWhip";
        public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]Test Whip");
            Tooltip.SetDefault("[PH]Whip Infusion\nNo effect");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 14;
            Item.rare = ItemRarityID.Green;

            color = new Color(255, 235, 70);
        }

        public override void OnActivate()
        {
        }

        public override void UpdateActive()
        {
        }

        public override void OnExit()
        {
        }

        public override void UpdateActiveEffects()
        {
        }
    }

    class TestWhipImprint : InfusionImprint
    {
        public override InfusionTier Tier => InfusionTier.Bronze;
        public override string Texture => "StarlightRiver/Assets/Abilities/TestWhipImprint";
        public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";
        public override string PreviewVideo => "StarlightRiver/Assets/Videos/AstralPreview";

        public override int TransformTo => ModContent.ItemType<TestWhip>();

        public override bool Visible => Main.LocalPlayer.controlHook;

        public override void Load()
        {
            
        }

        public override void SafeSetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]Test Whip");
            Tooltip.SetDefault("No effect");
        }

        public override void SetDefaults()
        {
            objectives.Add(new InfusionObjective("Implement Objectives", 1, Color.Orange));
        }
    }
}
