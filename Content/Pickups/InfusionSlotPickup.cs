using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Pickups
{
	class InfusionSlotPickup : AbilityPickup
	{
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override Color GlowColor => Color.Black;

        public override bool CanPickup(Player player) => player.GetHandler().InfusionLimit <= 0;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Unnerving Device");

		public override void SafeSetDefaults()
		{
            npc.behindTiles = true;
		}

		public override void Visuals()
        {

        }

        public override void PickupVisuals(int timer)
        {
            Player player = Main.LocalPlayer;

            //blood spray and sounds
            if(timer == 15 || (timer > 120 && timer < 220 && timer % 10 == 8))
			{
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(player.Center, DustID.Blood, Vector2.UnitY.RotatedBy(timer / 10f - 1.57f).RotatedByRandom(0.25f) * Main.rand.NextFloat(10));
                }

                player.headPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);
                player.bodyPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);
                player.legPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);

                Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), player.Center);
            }

            if (timer == 459) 
            {
                UILoader.GetUIState<TextCard>().Display("Mysterious Technology", "What has it done to you?", time: 360);
                Helper.UnlockEntry<InfusionEntry>(Main.LocalPlayer);

                player.headPosition = Vector2.Zero;
                player.bodyPosition = Vector2.Zero; 
                player.legPosition = Vector2.Zero; 
            }
        }

        public override void PickupEffects(Player player)
        {
            player.GetHandler().InfusionLimit = 1;
            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 460;
        }

		public override void DrawBehind(int index)
		{
            //Main.instance.DrawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Color drawColor)
		{
            var player = Main.LocalPlayer;
            var timer = player.GetModPlayer<StarlightPlayer>().PickupTimer;

            if (timer < 1 || timer > 459)
                return;

            var tex = ModContent.GetTexture("StarlightRiver/Assets/Abilities/BrassSpike");
            var origin = new Vector2(0, tex.Height / 2);

            float progressFirst = Math.Min(1, timer / 12f);

            if (timer > 300)
                progressFirst = 1 - (timer - 300) / 60f;

            Vector2 positionFirst = player.Center + Vector2.SmoothStep(Vector2.UnitY * 100, Vector2.Zero, progressFirst) - Main.screenPosition;

            spriteBatch.Draw(tex, positionFirst, null, Color.White, (float)Math.PI * 0.5f, origin, 1, 0, 0);

            if(timer > 120)
			{
                for(int k = 0; k < 10; k++)
				{
                    int relTime = (timer - (120 + k * 10));
                    int relTime2 = (timer - (120 + k * 2));
                    float progress = Math.Min(1, relTime / 12f);

                    if (relTime2 > 180)
                        progress = 1 - (relTime2 - 180) / 60f;

                    Vector2 position = player.Center + Vector2.SmoothStep(Vector2.UnitX.RotatedBy(k) * 140, Vector2.Zero, progress) - Main.screenPosition;

                    if (progress > 0)
                    {
                        spriteBatch.Draw(tex, position, null, Color.White, k, origin, 1, 0, 0);

                        if(relTime2 < 140)
                            Dust.NewDustPerfect(player.Center + Vector2.UnitX.RotatedBy(k) * Main.rand.Next(80, 140), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(k + 3.14f) * Main.rand.NextFloat(4), 0, new Color(255, 200, 100), 0.25f);
                    }
				}
			}
        }
	}
}
