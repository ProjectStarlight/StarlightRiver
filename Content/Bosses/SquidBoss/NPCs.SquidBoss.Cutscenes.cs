using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    public partial class SquidBoss : ModNPC
    {
        public void SpawnAnimation()
		{
            if (GlobalTimer == 1)
                savedPoint = Arena.FakeBoss.Center;

            if (GlobalTimer > 1 && GlobalTimer < 100)
            {
                float progress = Helper.BezierEase(GlobalTimer / 100f);
                Arena.FakeBoss.Center = Vector2.Lerp(savedPoint, new Vector2(savedPoint.X, spawnPoint.Y), progress);
            }

            if (GlobalTimer == 300)
            {
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 25;
                Helper.PlayPitched("ArenaHit", 1f, 0.5f, NPC.Center);
            }

            if (GlobalTimer > 300 && GlobalTimer < 400)
            {
                float progress = Helper.SwoopEase((GlobalTimer - 300) / 100f);
                NPC.Center = Vector2.Lerp(spawnPoint, spawnPoint + new Vector2(0, -600), progress); //rise up from the ground
            }

            if (GlobalTimer == 100)
            {
                string title = Main.rand.Next(10000) == 0 ? "Jammed Mod" : "The Venerated";
                UILoader.GetUIState<TextCard>().Display("Auroracle", title, null, 440);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = NPC.Center + new Vector2(0, -600);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 440;
            }

            for (int k = 0; k < 4; k++) //each tenticle
            {
                if (GlobalTimer == 100)
                {
                    int x;
                    int y;
                    int xb;

                    switch (k) //I handle these manually to get them to line up with the window correctly
                    {
                        case 0: x = -270; y = 0; xb = -50; break;
                        case 1: x = -420; y = -100; xb = -20; break;
                        case 3: x = 270; y = 0; xb = 50; break;
                        case 2: x = 420; y = -100; xb = 20; break;
                        default: x = 0; y = 0; xb = 0; break;
                    }

                    int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X + x, (int)NPC.Center.Y - 50, NPCType<Tentacle>(), 0, 1);
                    (Main.npc[i].ModNPC as Tentacle).Parent = this;
                    (Main.npc[i].ModNPC as Tentacle).MovementTarget = new Vector2((int)NPC.Center.X + x, (int)NPC.Center.Y - 500 - y);
                    (Main.npc[i].ModNPC as Tentacle).OffsetFromParentBody = xb;
                    (Main.npc[i].ModNPC as Tentacle).BasePoint = Main.npc[i].Center;
                    (Main.npc[i].ModNPC as Tentacle).Timer = 120 + k * 20;
                    (Main.npc[i].ModNPC as Tentacle).StalkWaviness = 0;
                    tentacles.Add(Main.npc[i]);
                }

                if (GlobalTimer == 100 + k * 30)
                {
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 5;
                    Helper.PlayPitched("ArenaHit", 0.5f, 1f, tentacles[k].Center);
                }

                if (GlobalTimer > 100 + k * 30 && GlobalTimer <= 160 + k * 30)
                {
                    var tentacle = (tentacles[k].ModNPC as Tentacle);
                    float progress = Helper.SwoopEase((GlobalTimer - (100 + k * 30)) / 60f);

                    tentacle.NPC.Center = Vector2.Lerp(tentacle.BasePoint, tentacle.MovementTarget, progress);
                    tentacle.DownwardDrawDistance = 50;
                    tentacle.StalkWaviness = progress * 0.5f;
                }
            }

            if (GlobalTimer > 500 && GlobalTimer <= 550) //tentacles returning back underwater
                foreach (NPC tentacle in tentacles)
                {
                    Tentacle mt = tentacle.ModNPC as Tentacle;
                    tentacle.Center = Vector2.SmoothStep(mt.MovementTarget, mt.BasePoint, (GlobalTimer - 500) / 50f);
                }

            if (GlobalTimer > 550 && GlobalTimer < 600)
            {
                foreach (NPC tentacle in tentacles)
                {
                    Tentacle mt = tentacle.ModNPC as Tentacle;
                    mt.DownwardDrawDistance = 28 + (int)(22 * (1 - (GlobalTimer - 550) / 50f));
                }
            }

            if (GlobalTimer > 700)
            {
                foreach (NPC tentacle in tentacles)
                {
                    Tentacle mt = tentacle.ModNPC as Tentacle;
                    mt.DownwardDrawDistance = 28;
                }

                Phase = (int)AIStates.FirstPhase;
            }
        }
    }
}
