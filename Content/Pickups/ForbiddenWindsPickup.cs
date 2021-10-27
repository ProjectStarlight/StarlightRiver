using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Pickups
{
	internal class ForbiddenWindsPickup : AbilityPickup, IDrawPrimitive
    {
        private List<Vector2> cache1;
        private List<Vector2> cache2;
        private List<Vector2> cache3;
        private Trail trail1;
        private Trail trail2;
        private Trail trail3;
        private Trail trail4;
        private Trail trail5;
        private Trail trail6;

        private Vector2 point1;
        private Vector2 point2;
        private Vector2 point3;

        public override string Texture => "StarlightRiver/Assets/Abilities/ForbiddenWinds";

        public override Color GlowColor => new Color(160, 230, 255);

        public override bool CanPickup(Player player) => !player.GetHandler().Unlocked<Dash>();

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

                cache1.Clear();
                cache2.Clear();
                cache3.Clear();
            }

            if (timer < 520)
            {
                if (timer < 260)
                {
                    float progress = Helper.BezierEase(timer / 260f);
                    point1 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f) * (40 + progress * 80), 100 - timer);
                    point2 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3) * (40 + progress * 80), 100 - timer);
                    point3 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3 * 2) * (40 + progress * 80), 100 - timer);
                }

                if (timer >= 260 && timer <= 380)
                {
                    float progress = 1 - Helper.BezierEase((timer - 260) / 120f);
                    point1 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f) * progress * 120, progress * -160);
                    point2 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3) * progress * 120, progress * -160);
                    point3 = player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3 * 2) * progress * 120, progress * -160);

                    //point1 = Vector2.SmoothStep(player.Center + new Vector2((float)Math.Sin(6.28f) * 120, -160), player.Center, (timer - 300) / 60f);
                    //point2 = Vector2.SmoothStep(player.Center + new Vector2((float)Math.Sin(6.28f + 6.28f / 3) * 120, -160), player.Center, (timer - 300) / 60f);
                    //point3 = Vector2.SmoothStep(player.Center + new Vector2((float)Math.Sin(6.28f + 6.28f / 3 * 2) * 120, -160), player.Center, (timer - 300) / 60f);
                }

                if (cache1 != null && timer > 60)
                {
                    Dust.NewDustPerfect(cache1[Main.rand.Next(20, 100)], DustType<Content.Dusts.Glow>(), Vector2.Zero, 0, new Color(80, 140, 140), 0.3f);
                    Dust.NewDustPerfect(cache2[Main.rand.Next(20, 100)], DustType<Content.Dusts.Glow>(), Vector2.Zero, 0, new Color(80, 140, 140), 0.3f);
                    Dust.NewDustPerfect(cache3[Main.rand.Next(20, 100)], DustType<Content.Dusts.Glow>(), Vector2.Zero, 0, new Color(80, 140, 140), 0.3f);
                }

                ManageCache(ref cache1, point1);
                ManageCache(ref cache2, point2);
                ManageCache(ref cache3, point3);

                ManageTrail(ref trail1, cache1);
                ManageTrail(ref trail2, cache2);
                ManageTrail(ref trail3, cache3);
                ManageTrail(ref trail4, cache1, true);
                ManageTrail(ref trail5, cache2, true);
                ManageTrail(ref trail6, cache3, true);
            }

            if (timer == 500)
            {
                Main.PlaySound(SoundID.Item104);
                Main.PlaySound(SoundID.Item45);
            }

            if (timer > 500)
            {
                float timeRel = (timer - 500) / 150f;
                Dust.NewDust(player.position, player.width, player.height, DustType<Content.Dusts.Air>(), 0, 0, 0, default, 0.3f);
                Filters.Scene.Activate("Shockwave", player.Center).GetShader().UseProgress(2f).UseIntensity(100).UseDirection(new Vector2(0.005f + timeRel * 0.5f, 1 * 0.02f - timeRel * 0.02f));
            }

            if (timer == 569) //popup + codex entry
            {
                string message = StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys().Count > 0 ?
                    "Press W/A/S/D + " + StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys()[0] + " to dash." :
                    "Press W/A/S/D + [Please Bind a Key] to dash.";

                Main.LocalPlayer.GetHandler().GetAbility<Dash>(out var dash);
                UILoader.GetUIState<TextCard>().Display("Forbidden Winds", message, dash);
                Helper.UnlockEntry<WindsEntry>(Main.LocalPlayer);
                Helper.UnlockEntry<StaminaEntry>(Main.LocalPlayer);

                Filters.Scene.Activate("Shockwave", player.Center).GetShader().UseProgress(0f).UseIntensity(0);
                Filters.Scene.Deactivate("Shockwave");
            }

            // audio fade shenanigans
            Main.musicFade[Main.curMusic] = timer < 500 ? 0 : (timer - 500) / 70f;
        }

        public override void PickupEffects(Player player)
        {
            player.GetHandler().Unlock<Dash>();

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 650;
            player.GetModPlayer<StarlightPlayer>().inTutorial = true;
            player.AddBuff(BuffID.Featherfall, 660);
        }

        private void ManageCache(ref List<Vector2> cache, Vector2 point)
        {
            if (cache == null || cache.Count < 120)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 120; i++)
                {
                    cache.Add(npc.Center + new Vector2(0, 100));
                }
            }

            cache.Add(point);

            while (cache.Count > 120)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, bool wide = false)
        {
            if (wide)
            {
                trail = trail ?? new Trail(Main.instance.GraphicsDevice, 120, new TriangularTip(40 * 4), factor => Math.Min((float)Math.Sin(factor * 3.14f) * 50, 50), factor =>
                {
                    if (factor.X >= 0.95f)
                        return Color.White * 0;

                    int time = Main.LocalPlayer.GetModPlayer<StarlightPlayer>().PickupTimer;
                    float mul = 1;

                    if (time < 100)
                        mul = (time - 30) / 70f;

                    if (time > 600)
                        mul = 1 - (time - 600) / 20f;

                    return new Color(140, 150 + (int)(105 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f) * mul * 0.06f;
                });
            }
            else
            {
                trail = trail ?? new Trail(Main.instance.GraphicsDevice, 120, new TriangularTip(40 * 4), factor => Math.Min((float)Math.Sin(factor * 3.14f) * 13, 13), factor =>
                {
                    if (factor.X >= 0.95f)
                        return Color.White * 0;

                    int time = Main.LocalPlayer.GetModPlayer<StarlightPlayer>().PickupTimer;
                    float mul = 1;

                    if (time < 100)
                        mul = (time - 30) / 70f;

                    if (time > 600)
                        mul = 1 - (time - 600) / 20f;

                    return new Color(140, 150 + (int)(105 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f) * mul;
                });
            }

            trail.Positions = cache.ToArray();
            trail.NextPosition = npc.Center;
        }

		public void DrawPrimitives()
		{
			//if (Main.LocalPlayer.GetModPlayer<StarlightPlayer>().PickupTimer < 420)
			//return;

			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.01f);
			effect.Parameters["repeats"].SetValue(4f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/FireTrail"));

			trail1?.Render(effect);
			trail2?.Render(effect);
			trail3?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/GlowTrail"));

            trail4?.Render(effect);
            trail5?.Render(effect);
            trail6?.Render(effect);
        }
    }

    public class ForbiddenWindsPickupTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<ForbiddenWindsPickup>();
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) //get rid of this after demo
        {   
                Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(-80, -30);
                Utils.DrawBorderString(spriteBatch, "Dash into", pos, Color.White, 0.7f);
                Utils.DrawBorderString(spriteBatch, "bright blue", pos + new Vector2(60, 0), new Color(188, 255, 246), 0.7f);
                Utils.DrawBorderString(spriteBatch, "outlines", pos + new Vector2(127, 0), Color.White, 0.7f);
        }
    }

    public class WindsTileItem : QuickTileItem
    {
        public WindsTileItem() : base("Forbidden Winds", "Debug placer for ability pickup", TileType<ForbiddenWindsPickupTile>(), -1) { }

        public override string Texture => "StarlightRiver/Assets/Abilities/ForbiddenWinds";
    }
}