using Microsoft.Xna.Framework;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class CrystalPopper : ModNPC
    {
        private const int animFramesLoop = 6; //amount of frames in the main loop
        private readonly float AnimSpeedMult = 0.3f;

        public override string Texture => AssetDirectory.VitricNpc + Name;

        public override void Load()
        {
            for (int k = 0; k <= 4; k++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/CrystalPopperGore" + k);    //PORTTODO: Gore stuff again       
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Bat");
            Main.npcFrameCount[NPC.type] = 7;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.noGravity);
            writer.Write(NPC.target);
            writer.WritePackedVector2(NPC.velocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.noGravity = reader.ReadBoolean();
            NPC.target = reader.ReadInt32();
            NPC.velocity = reader.ReadPackedVector2();
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 42;
            NPC.knockBackResist = 0.8f;
            NPC.lifeMax = 80;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.damage = 10;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;

            NPC.direction = Main.rand.Next(2) == 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
        }

        const int maxIgnoreDamage = 1;

        private void ExitSleep()
        {
            NPC.ai[0] = 1;
            NPC.noGravity = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            switch (NPC.ai[0])
            {
                case 0://sleeping: in ground checking for Player
                    NPC.velocity.X *= 0.9f;
                    if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) <= 180)
                        ExitSleep();
                    break;

                case 1://shoot out of ground and attack
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1) NPC.velocity.Y = -20;

                    NPC.velocity.Y += 0.6f;

                    for (int k = 0; k <= 3; k++)
                        Dust.NewDust(NPC.position, 32, 32, DustID.Sandstorm);

                    if (NPC.ai[1] >= 30)
                    {
                        NPC.velocity.Y = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 2;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int k = -1; k <= 1; k++)
                                Projectile.NewProjectile(NPC.GetSpawnSourceForProjectileNPC(), NPC.Center, Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center).RotatedBy(k * 0.5f) * 6, ProjectileType<Bosses.VitricBoss.GlassSpike>(), 10, 0);
                        }

                        NPC.velocity = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * -5.5f;

                        if (Main.netMode == NetmodeID.Server)
                            NPC.netUpdate = true;
                    }
                    break;

                case 2://seek and destroy
                    NPC.velocity += Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * 0.08f;

                    if (NPC.velocity.LengthSquared() > 25) NPC.velocity = Vector2.Normalize(NPC.velocity) * 5f;

                    if (NPC.collideX && Math.Abs(NPC.velocity.X) > 1f) NPC.velocity.X = Vector2.Normalize(-NPC.velocity).X * 1.5f;
                    if (NPC.collideY && Math.Abs(NPC.velocity.Y) > 1f) NPC.velocity.Y = Vector2.Normalize(-NPC.velocity).Y * 1.5f;

                    NPC.spriteDirection = Main.player[NPC.target].Center.X - NPC.Center.X < 0 ? -1 : 1;
                    break;
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k <= 4; k++)
                    Gore.NewGoreDirect(NPC.position, Vector2.Zero, Mod.Find<ModGore>(AssetDirectory.VitricNpc + "Gore/CrystalPopperGore" + k).Type);
            }

            if (NPC.ai[0] == 0 && damage > maxIgnoreDamage)
                ExitSleep();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile tile = Framing.GetTileSafely(spawnInfo.spawnTileX, spawnInfo.spawnTileY);
            return tile.HasTile && spawnInfo.spawnTileType != TileType<VitricSpike>() && spawnInfo.player.InModBiome(ModContent.GetInstance<VitricDesertBiome>()) ? 95f : 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            switch (NPC.ai[0])
            {
                case 0: NPC.frame.Y = frameHeight * 6; break;
                case 1: NPC.frame.Y = frameHeight * 0; break;
                case 2:
                    NPC.frameCounter++;//skele frame-code
                    if ((int)(NPC.frameCounter * AnimSpeedMult) >= animFramesLoop)
                        NPC.frameCounter = 0;
                    NPC.frame.Y = (int)(NPC.frameCounter * AnimSpeedMult) * frameHeight; break;
            }
        }
    }

    /*
    internal class VitricBatBanner : ModBanner
    {
        public VitricBatBanner() : base("VitricBatBannerItem", NPCType<CrystalPopper>(), AssetDirectory.VitricNpc) { }
    }

    internal class VitricBatBannerItem : QuickBannerItem
    {
        public VitricBatBannerItem() : base("VitricBatBanner", "Sand Bat", AssetDirectory.VitricNpc) { }
    }*/
}