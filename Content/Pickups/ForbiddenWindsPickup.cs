using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Pickups
{
    internal class ForbiddenWindsPickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/ForbiddenWinds";

        public override Color GlowColor => new Color(160, 230, 255);

        public override bool CanPickup(Player player)
        {
            return !player.GetHandler().Unlocked<Dash>();
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Forbidden Winds");

        public override void Visuals()
        {
            Dust dus = Dust.NewDustPerfect(new Vector2(npc.Center.X + (float)Math.Sin(StarlightWorld.rottime) * 30, npc.Center.Y - 20), DustType<Content.Dusts.Air>(), Vector2.Zero);
            dus.fadeIn = Math.Abs((float)Math.Sin(StarlightWorld.rottime));

            Dust dus2 = Dust.NewDustPerfect(new Vector2(npc.Center.X + (float)Math.Cos(StarlightWorld.rottime) * 15, npc.Center.Y), DustType<Content.Dusts.Air>(), Vector2.Zero);
            dus2.fadeIn = Math.Abs((float)Math.Cos(StarlightWorld.rottime));
        }

        public override void PickupVisuals(int timer)
        {
            Player player = Main.LocalPlayer;

            if (timer == 1)
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pickups/get")); //start the SFX
                Filters.Scene.Deactivate("Shockwave");
            }

            if (timer < 300) //spiraling out
            {
                for (int k = 0; k < 3; k++)
                {
                    float scale = timer < 50 ? timer / 50f : 1;
                    float rot = timer / 80f * 6.28f + k * 2;
                    Vector2 pos = player.Center + new Vector2(0, 30) + new Vector2((float)Math.Sin(rot) * (timer / 570f * 80f), -timer / 300f * 80 + k * 10);
                    Dust dus = Dust.NewDustPerfect(pos, DustType<Content.Dusts.Air>(), Vector2.Zero, 0, default, scale);
                    dus.fadeIn = rot > 3.14f ? 1 - Math.Abs((float)Math.Sin(timer / 100f * 6.28f + k * 2)) : 1;
                    dus.fadeIn = Math.Abs((float)Math.Sin(timer / 80f * 6.28f + k * 2));
                }
            }

            if (timer > 300 && timer < 420) //coming in
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector2 startPos = player.Center + new Vector2(0, 30) + new Vector2((float)Math.Sin(timer / 80f * 6.28f + k * 2) * (300 / 570f * 80f), -80 + k * 10);
                    Vector2 endPos = player.Center;
                    Dust dus = Dust.NewDustPerfect(Vector2.Lerp(startPos, endPos, (timer - 300) / 120f), DustType<Content.Dusts.Air>(), Vector2.Zero, 0, default, 1.2f - (timer - 300) / 120f);
                    dus.fadeIn = Math.Abs((float)Math.Sin(timer / 80f * 6.28f + k * 2));
                }
            }

            if (timer == 420)
            {
                Main.PlaySound(SoundID.Item104);
                Main.PlaySound(SoundID.Item45);
            }

            if (timer > 420)
            {
                float timeRel = (timer - 420) / 150f;
                Dust.NewDust(player.position, player.width, player.height, DustType<Content.Dusts.Air>(), 0, 0, 0, default, 0.3f);
                Filters.Scene.Activate("Shockwave", player.Center).GetShader().UseProgress(2f).UseIntensity(100).UseDirection(new Vector2(0.005f + timeRel * 0.5f, 1 * 0.02f - timeRel * 0.02f));
            }

            if (timer == 569) //popup + codex entry
            {
                string message = StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys().Count > 0 ?
                    "Press A/W/S/D + " + StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys()[0] + " to dash." :
                    "Press A/W/S/D + [Please Bind a Key] to dash.";

                Main.LocalPlayer.GetHandler().GetAbility<Dash>(out var dash);
                UILoader.GetUIState<TextCard>().Display("Forbidden Winds", message, dash);
                Helper.UnlockEntry<WindsEntry>(Main.LocalPlayer);
                Helper.UnlockEntry<StaminaEntry>(Main.LocalPlayer);
            }

            // audio fade shenanigans
            Main.musicFade[Main.curMusic] = timer < 500 ? 0 : (timer - 500) / 70f;
        }

        public override void PickupEffects(Player player)
        {
            player.GetHandler().Unlock<Dash>();

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 570;
            player.GetModPlayer<StarlightPlayer>().inTutorial = true;
            player.AddBuff(BuffID.Featherfall, 580);
        }
    }

    public class ForbiddenWindsPickupTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<ForbiddenWindsPickup>();
    }

    public class WindsTileItem : QuickTileItem
    {
        public WindsTileItem() : base("Forbidden Winds", "Debug placer for ability pickup", TileType<ForbiddenWindsPickupTile>(), -1) { }

        public override string Texture => AssetDirectory.Debug;
    }
}