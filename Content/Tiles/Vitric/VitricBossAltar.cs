﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricBossAltar : DummyTile
    {
        public override int DummyType => ProjectileType<VitricBossAltarDummy>();

        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            (this).QuickSetFurniture(5, 7, DustType<Air>(), SoundID.Tink, false, new Color(200, 113, 113), false, false, "Ceiro's Altar");
            MinPick = int.MaxValue;
        }

        public override bool CanExplode(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.TileType == TileType<VitricBossAltar>())
                return false;

            return base.CanExplode(i, j);
        }

        public override bool SpawnConditions(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return tile.TileFrameX % 90 == 0 && tile.TileFrameY == 0;
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (Main.rand.Next(200) == 0 && tile.TileFrameX < 90 && tile.TileFrameX > 16)
            {
                Vector2 pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(pos, DustType<CrystalSparkle>(), Vector2.Zero);
                else
                    Dust.NewDustPerfect(pos, DustType<CrystalSparkle2>(), Vector2.Zero);
            }

            base.SafeNearbyEffects(i, j, closer);
        }

        public override void MouseOver(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.TileFrameX >= 90)
            {
                Player Player = Main.LocalPlayer;
                Player.cursorItemIconID = ItemType<Items.Vitric.GlassIdol>();
                Player.noThrow = 2;
                Player.cursorItemIconEnabled = true;
            }
        }

        public override bool RightClick(int i, int j)
        {
            Tile tile = (Tile)Framing.GetTileSafely(i, j).Clone();
            Player player = Main.LocalPlayer;

            if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && tile.TileFrameX >= 90 && !NPC.AnyNPCs(NPCType<VitricBoss>()) && (player.ConsumeItem(ItemType<Items.Vitric.GlassIdol>()) || player.HasItem(ItemType<Items.Vitric.GlassIdolPremiumEdition>())))
            {
                int x = i - (tile.TileFrameX - 90) / 18;
                int y = j - tile.TileFrameY / 18;
                SpawnBoss(x, y, player);
                return true;
            }

            return false;
        }

        public void SpawnBoss(int i, int j, Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = new SpawnNPC(Main.myPlayer, i * 16 + 40, j * 16 + 556, NPCType<VitricBoss>());
                packet.Send(-1, -1, false);

                return;
            }

            int n = Terraria.NPC.NewNPC(new EntitySource_BossSpawn(player), i * 16 + 40, j * 16 + 556, NPCType<VitricBoss>());
            var NPC = Main.npc[n];

            if (NPC.type == NPCType<VitricBoss>())
                (Dummy(i, j).ModProjectile as VitricBossAltarDummy).boss = Main.npc[n];
        }
    }

    class VitricBossAltarItem : QuickTileItem
    {
        public VitricBossAltarItem() : base("Vitric Boss Altar Item", "Debug Item", "VitricBossAltar", 1, AssetDirectory.Debug, true) { }
    }

    internal class VitricBossAltarDummy : Dummy
    {
        private NPC arenaLeft;
        private NPC arenaRight;

        public NPC boss;

        public ref float BarrierProgress => ref Projectile.ai[0];
        public ref float CutsceneTimer => ref Projectile.ai[1];

        public VitricBossAltarDummy() : base(TileType<VitricBossAltar>(), 80, 112) { }

        bool collisionHappened = false;

        public override void SafeSetDefaults()
        {
            Projectile.hide = true;
        }

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
            behindNPCsAndTiles.Add(index);
		}

        public override void Collision(Player Player)
        {
            Point16 parentPos = new Point16((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.TileFrameX == 0 && Abilities.AbilityHelper.CheckDash(Player, Projectile.Hitbox) && !collisionHappened)
            {
                collisionHappened = true;

                CutsceneTimer = 0;

                if (Main.netMode != NetmodeID.Server)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter);
                    for (int k = 0; k < 100; k++) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.GlassGravity>(), 0, 0, 0, default, 1.2f);

                    if (Main.myPlayer == Player.whoAmI)
                    {
                        for (int x = parentPos.X; x < parentPos.X + 5; x++)
                            for (int y = parentPos.Y; y < parentPos.Y + 7; y++)
                                Framing.GetTileSafely(x, y).TileFrameX += 90;

                        NetMessage.SendTileSquare(Player.whoAmI, parentPos.X, parentPos.Y, 5, 7, TileChangeType.None);
                    }
                }
            }
        }

        public void findParent()
        {
            boss = null;
            arenaLeft = null;
            arenaRight = null;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC NPC = Main.npc[i];
                if (NPC.active && NPC.type == ModContent.NPCType<VitricBoss>())
                    boss = NPC;

                if (NPC.active && NPC.type == ModContent.NPCType<VitricBackdropLeft>())
                    arenaLeft = NPC;

                if (NPC.active && NPC.type == ModContent.NPCType<VitricBackdropRight>())
                    arenaRight = NPC;
            }

            return;
        }

        public override void Update()
        {
            Point16 parentPos = new Point16((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && CutsceneTimer < 660) //should prevent the cutscene from reoccuring?
                CutsceneTimer = 999;

            if (boss is null || arenaLeft is null || arenaRight is null)
                findParent();
                

            //This controls spawning the rest of the arena
            if (arenaLeft is null || arenaRight is null || !arenaLeft.active || !arenaRight.active && Main.netMode != NetmodeID.MultiplayerClient)
            {
                foreach (NPC NPC in Main.npc.Where(n => n.active && //reset the arena if one of the sides somehow dies
                 (
                 n.type == NPCType<VitricBackdropLeft>() ||
                 n.type == NPCType<VitricBackdropRight>() ||
                 n.type == NPCType<VitricBossPlatformDown>() ||
                 n.type == NPCType<VitricBossPlatformDownSmall>() ||
                 n.type == NPCType<VitricBossPlatformUp>() ||
                 n.type == NPCType<VitricBossPlatformUpSmall>()
                 )))
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }

                Vector2 center = Projectile.Center + new Vector2(0, 60);
                int timerset = StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && CutsceneTimer >= 660 ? 360 : 0; //the arena should already be up if it was opened before

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int index = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)center.X + 352, (int)center.Y, NPCType<VitricBackdropRight>(), 0, timerset);
                    arenaRight = Main.npc[index];

                    if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && Main.npc[index].ModNPC is VitricBackdropRight)
                        (Main.npc[index].ModNPC as VitricBackdropRight).SpawnPlatforms(false);

                    index = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)center.X - 352, (int)center.Y, NPCType<VitricBackdropLeft>(), 0, timerset);
                    arenaLeft = Main.npc[index];

                    if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && Main.npc[index].ModNPC is VitricBackdropLeft)
                        (Main.npc[index].ModNPC as VitricBackdropLeft).SpawnPlatforms(false);
                }
            }

            if (parent.TileFrameX == 0)
                return;

            if (boss is null || !boss.active || boss.type != ModContent.NPCType<VitricBoss>())
                boss = null;

            if (parent.TileFrameX == 90 && !StarlightWorld.HasFlag(WorldFlags.VitricBossOpen))
            {
                if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<VitricDesertBiome>()))
                {
                    Core.Systems.CameraSystem.Shake += 1;
                    Dust.NewDust(Projectile.Center + new Vector2(-632, Projectile.height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
                    Dust.NewDust(Projectile.Center + new Vector2(72, Projectile.height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));

                    if (CutsceneTimer > 120 && CutsceneTimer <= 240)
                        Main.musicFade[Main.curMusic] = 1 - ((CutsceneTimer - 120) / 120f);

                    if (CutsceneTimer == 180)
                        Helper.PlayPitched("ArenaRise", 0.5f, -0.1f, Projectile.Center);
                }

                CutsceneTimer++;

                if (CutsceneTimer > 180)
                {
                    StarlightWorld.Flag(WorldFlags.VitricBossOpen);

                    if (Main.LocalPlayer.InModBiome(GetInstance<VitricDesertBiome>()))
                    {
                        Core.Systems.CameraSystem.DoPanAnimation(VitricBackdropLeft.Risetime + 120, Projectile.Center, Projectile.Center + new Vector2(0, -400));
                    }
                }
            }

            if (CutsceneTimer > 240 && CutsceneTimer < 660)
                Main.musicFade[Main.curMusic] = 0;

            CutsceneTimer++;

            //controls the drawing of the barriers
            if (BarrierProgress < 120 && boss != null && boss.active)
            {
                BarrierProgress++;

                if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<VitricDesertBiome>()))
                {
                    if (BarrierProgress % 3 == 0)
                        Core.Systems.CameraSystem.Shake += 2; //screenshake

                    if (BarrierProgress == 119) //hitting the top
                    {
                        Core.Systems.CameraSystem.Shake += 15;
                        Helper.PlayPitched("VitricBoss/CeirosPillarImpact", 0.5f, 0, Projectile.Center);
                    }
                }
            }
            else if (BarrierProgress > 0 && (boss == null || !boss.active))
                BarrierProgress--;
        }

        private bool checkIfDrawReflection()
        {
            Point16 parentPos = new Point16((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            return parent.TileFrameX < 90 && Projectile.getRect().Intersects(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight));
        }

        public override void PostDraw(Color lightColor) //actually drawing the barriers and Item indicator
        {
            var spriteBatch = Main.spriteBatch;

            Point16 parentPos = new Point16((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.TileFrameX >= 90 && !NPC.AnyNPCs(NPCType<VitricBoss>()))
            {
                Texture2D texSkull = Request<Texture2D>("StarlightRiver/Assets/Symbol").Value;
                spriteBatch.Draw(texSkull, Projectile.Center - Main.screenPosition, null, new Color(255, 100, 100) * (1 - Vector2.Distance(Main.LocalPlayer.Center, Projectile.Center) / 200f), 0, texSkull.Size() / 2, 1, 0, 0);
            }

            else if (parent.TileFrameX < 90 && ReflectionTarget.canUseTarget)
            {
                if (checkIfDrawReflection())
                {
                    ReflectionTarget.DrawReflection(spriteBatch, screenPos: Projectile.position - Main.screenPosition, normalMap: Request<Texture2D>(AssetDirectory.VitricTile + "VitricBossAltarReflectionMap").Value, flatOffset: new Vector2(-0.0075f, 0.011f), tintColor: new Color(150, 150, 255, 200), offsetScale: 0.05f);
                    ReflectionTarget.isDrawReflectablesThisFrame = true;
                }
                    
                Texture2D glow = Request<Texture2D>(AssetDirectory.VitricTile + "VitricBossAltarGlow").Value;
                spriteBatch.Draw(glow, Projectile.position - Main.screenPosition + new Vector2(-1, 7), glow.Frame(), Helper.IndicatorColorProximity(300, 600, Projectile.Center), 0, Vector2.Zero, 1, 0, 0);
            }

            //Barriers
            Vector2 center = Projectile.Center + new Vector2(0, 56);
            Texture2D tex = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBarrier").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBarrier2").Value;
            Texture2D texTop = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBarrierTop").Value;
            //Color color = new Color(180, 225, 255);

            int off = (int)(BarrierProgress / 120f * tex.Height);
            int off2 = (int)(BarrierProgress / 120f * texTop.Width / 2);

            LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X - 790 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off), tex, new Rectangle(0, 0, tex.Width, off), default);
            LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X + 606 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off), tex2, new Rectangle(0, 0, tex.Width, off), default);

            //left
            LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X - 592 - (int)Main.screenPosition.X, (int)center.Y - 1040 - (int)Main.screenPosition.Y, off2, texTop.Height), texTop, new Rectangle(texTop.Width / 2 - off2, 0, off2, texTop.Height), default);

            //right
            LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X + 608 - off2 - (int)Main.screenPosition.X, (int)center.Y - 1040 - (int)Main.screenPosition.Y, off2, texTop.Height), texTop, new Rectangle(texTop.Width / 2, 0, off2, texTop.Height), default);

            //spriteBatch.Draw(tex, new Rectangle((int)center.X - 790 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
            //new Rectangle(0, 0, tex.Width, off), color);

            //spriteBatch.Draw(tex, new Rectangle((int)center.X + 606 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
            //new Rectangle(0, 0, tex.Width, off), color);
        }
    }
}