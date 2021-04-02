using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Abilities.AbilityContent.Infusions;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
    class Astral : InfusionItem<Dash>
    {
        public override InfusionTier Tier => InfusionTier.Bronze;
        public override string Texture => "StarlightRiver/Assets/Abilities/Astral";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush I");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry more speed\nIncreases stamina cost to 1.3");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.Speed *= 0.75f;
            Ability.Boost = 0.5f;
            Ability.ActivationCostBonus += 0.3f;

            base.OnActivate();
            Main.PlaySound(SoundID.Item96, Player.Center);

            Ability.Time = 10;
        }

        public override void UpdateActive()
        {
            Player.velocity = Dash.SignedLesserBound(Ability.Dir * Ability.Speed, Player.velocity); // "conservation of momentum"

            Player.frozen = true;
            Player.gravity = 0;
            Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Ability.Speed);

            if (Ability.Time-- <= 0) Ability.Deactivate();
        }

        public override void UpdateActiveEffects()
        {
            Vector2 nextPos = Player.Center + Vector2.Normalize(Player.velocity) * Ability.Speed;
            for (float k = -2; k <= 2; k += 0.1f)
            {
                Vector2 pos = nextPos + Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + k) * 7 * (4 - Ability.Time);

                if (Ability.Time == 0)
                {
                    //Vector2 pos2 = nextPos + Vector2.UnitX.RotatedBy(Ability.Player.velocity.ToRotation() + k) * 60;
                    //Dust.NewDustPerfect(pos2, DustType<Dusts.BlueStamina>(), Vector2.UnitY.RotatedBy(Ability.Player.velocity.ToRotation() + k + 1.57f) * Math.Abs(k), 0, default, 3 - Math.Abs(k));
                }
                Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.4f, 0), 0, default, 1 - Ability.Time / 10f);

                if (Math.Abs(k) >= 1.5f)
                    Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.6f, -0.4f), 0, default, 2.2f - Ability.Time / 10f);
            }
        }
    }

    class Astral2 : Astral
    {
        public override InfusionTier Tier => InfusionTier.Silver;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush II");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry even more speed\nIncreases stamina cost to 1.6");
        }

        public override void OnActivate()
        {
            Ability.ActivationCostBonus += 0.3f;
            Ability.Speed *= 1.25f;
            base.OnActivate();
        }
    }

    class Astral3 : Astral
    {
        public override InfusionTier Tier => InfusionTier.Gold;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush III");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry the most speed\nIncreases stamina cost to 2");
        }

        public override void OnActivate()
        {
            Ability.ActivationCostBonus += 0.4f;
            Ability.Speed *= 1.25f;
            base.OnActivate();
        }
    }

    class AstralImprint : InfusionImprint
    {
        public override InfusionTier Tier => InfusionTier.Bronze;
        public override string Texture => "StarlightRiver/Assets/Abilities/AstralImprint";
        public override string PreviewVideo => "StarlightRiver/Assets/Videos/AstralPreview";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush");
            Tooltip.SetDefault("Dash farther and carry more speed");
        }

        public override void SetDefaults()
        {
            objectives.Add(new InfusionObjective("Jump", 100, CheckKillEnemies, Color.Orange));
        }

        public void CheckKillEnemies(InfusionObjective objective)
        {
            if (Player.controlJump) objective.progress++;
        }
    }
}
