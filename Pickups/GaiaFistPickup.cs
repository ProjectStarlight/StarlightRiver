using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Pickups
{
    internal class GaiaFistPickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Pickups/GaiaFist";
        public override Color GlowColor => new Color(180, 220, 140);

        public override bool CanPickup(Player player)
        {
            return player.GetModPlayer<AbilityHandler>().smash.Locked;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gaia's Fist");
        }

        public override void Visuals()
        {
            float timer = StarlightWorld.rottime;
            //Vector2 pos = npc.position - Main.screenPosition - (new Vector2((int)((Math.Cos(timer * 3) + 1) * 4f), (int)((Math.Sin(timer * 3) + 1) * 4f)) / 2) + new Vector2(0, (float)Math.Sin(timer) * 4);

            Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(timer) * (23 + (float)Math.Sin(timer * 10) * 4), DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);
            Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(timer) * 18, DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);
            Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(timer) * 28, DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);

            for (int k = 0; k < 2; k++)
            {
                Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(-timer + k * 0.02f) * (43 + (float)Math.Sin(timer * 10) * 4), DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);
                Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(-timer + k * 0.02f) * 38, DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);
                Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(-timer + k * 0.02f) * 48, DustType<Dusts.JungleEnergy>(), Vector2.Zero, 254, default, 0.8f);
            }
        }

        public override void PickupVisuals(int timer)
        {
            if (timer == 1)
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pickups/get")); //start the SFX
                Filters.Scene.Deactivate("ShockwaveFilter");
            }
        }

        public override void PickupEffects(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            mp.smash.Locked = false;
            mp.StatStaminaMaxPerm++;

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 570;
            player.AddBuff(BuffID.Featherfall, 580);
        }
    }
}